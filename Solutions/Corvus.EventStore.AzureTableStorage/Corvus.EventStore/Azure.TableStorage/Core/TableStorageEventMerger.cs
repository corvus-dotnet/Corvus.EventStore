// <copyright file="TableStorageEventMerger.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Corvus.Extensions;
    using Corvus.Retry;
    using Microsoft.Azure.Cosmos.Table;
    using Microsoft.Extensions.Caching.Memory;

    /// <summary>
    /// Merges commits from one cloud table factory into an output cloud table factory, across all aggregates.
    /// </summary>
    /// <typeparam name="TInputCloudTableFactory">The type of the input <see cref="IEventCloudTableFactory"/>.</typeparam>
    /// <typeparam name="TOutputCloudTableFactory">The type of the output <see cref="IEventCloudTableFactory"/>.</typeparam>
    public readonly struct TableStorageEventMerger<TInputCloudTableFactory, TOutputCloudTableFactory> : IAsyncDisposable
        where TInputCloudTableFactory : IEventCloudTableFactory
        where TOutputCloudTableFactory : IAllStreamCloudTableFactory
    {
        private const int MaxBatchSize = 99;

        private const int PartitionLatency = 1;

        private const string AllStreamPartitionKey = "allstreamitems";
        private const string AllStreamStateRowKey = "allstreamstate";
        private const string AllStreamCommitAggregateId = "CommitAggregateId";
        private const string AllStreamCommitSequenceNumber = "CommitSequenceNumber";
        private const string AllStreamStateNextSequenceNumber = "NextSequenceNumber";
        private const string AllStreamStateStartingTimestampTicks = "StartingTimestampTicks";

        private static readonly long MinAzureUtcDateTicks = new DateTimeOffset(1601, 1, 1, 0, 0, 0, TimeSpan.Zero).UtcTicks;
        private static readonly long TimeInAPartition = TimeSpan.FromMinutes(10).Ticks;

        private static readonly TableRequestOptions Options =
            new TableRequestOptions
            {
                LocationMode = LocationMode.PrimaryOnly,
                ConsistencyLevel = Microsoft.Azure.Cosmos.ConsistencyLevel.Session,
                PayloadFormat = TablePayloadFormat.Json,
                RetryPolicy = new NoRetry(),
                TableQueryEnableScan = true,
            };

        private readonly ReliableTaskRunner mergeRunner;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageEventMerger{TInputCloudTableFactory, TOutputCloudTableFactory}"/> struct.
        /// </summary>
        /// <param name="inputFactory">The <see cref="InputFactory"/>.</param>
        /// <param name="outputFactory">The <see cref="OutputFactory"/>.</param>
        internal TableStorageEventMerger(TInputCloudTableFactory inputFactory, TOutputCloudTableFactory outputFactory)
        {
            this.InputFactory = inputFactory;
            this.OutputFactory = outputFactory;
            this.mergeRunner = Start(inputFactory, outputFactory);
        }

        /// <summary>
        /// Gets the input cloud table factory.
        /// </summary>
        public TInputCloudTableFactory InputFactory { get; }

        /// <summary>
        /// Gets the output cloud table factory.
        /// </summary>
        public TOutputCloudTableFactory OutputFactory { get; }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            return new ValueTask(this.mergeRunner.StopAsync());
        }

        /// <summary>
        /// Starts merging events.
        /// </summary>
        private static ReliableTaskRunner Start(TInputCloudTableFactory inputFactory, TOutputCloudTableFactory outputFactory)
        {
            ImmutableArray<CloudTable> inputTables = inputFactory.GetTables();
            CloudTable outputTable = outputFactory.GetTable();

            return ReliableTaskRunner.Run(
                token =>
                {
                    return Task.Factory.StartNew<Task<Task>>(
                        async () =>
                        {
                            try
                            {
                                Console.WriteLine();
                                Console.WriteLine("Getting starting sequence number.");
                                (long startingCommitSequenceNumber, long startingTimestampTicks, long lastPartition) = GetStartPointers(outputTable);
                                Console.WriteLine($"Starting sequence number: {startingCommitSequenceNumber}");
                                Console.WriteLine($"Starting timestamp: {CreateDateFromTicksAndZeroOffset(startingTimestampTicks)}");

                                Console.WriteLine();
                                Console.WriteLine("Building existing commit list.");
                                MemoryCache commitsWeHaveSeen = BuildCommitsWeHaveSeen(outputTable, lastPartition);
                                Console.WriteLine();
                                Console.WriteLine($"Build commit list - {commitsWeHaveSeen.Count} commits.");

                                int previousCount = commitsWeHaveSeen.Count;

                                int originalCount = commitsWeHaveSeen.Count;

                                while (true)
                                {
                                    long currentTime = DateTimeOffset.Now.UtcTicks;
                                    startingCommitSequenceNumber = await WriteNewCommits(inputTables, outputTable, commitsWeHaveSeen, startingTimestampTicks, startingCommitSequenceNumber).ConfigureAwait(false);
                                    if (commitsWeHaveSeen.Count - previousCount > 0)
                                    {
                                        long elaspedTime = DateTimeOffset.Now.UtcTicks - currentTime;

                                        Console.WriteLine();
                                        Console.WriteLine($"Written {commitsWeHaveSeen.Count - previousCount} commits (Total: {commitsWeHaveSeen.Count - originalCount} commits in {TimeSpan.FromTicks(elaspedTime).TotalSeconds} == {(commitsWeHaveSeen.Count - previousCount) / TimeSpan.FromTicks(elaspedTime).TotalSeconds} commits/s)");
                                        previousCount = commitsWeHaveSeen.Count;
                                    }

                                    // Let's go back and look at the events from the time when we sarted this run through, plus one partition of leeway.
                                    startingTimestampTicks = currentTime - TimeInAPartition;
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                // Move this into a logger
                                Console.WriteLine(ex);
                                throw;
                            }
                        });
                },
                new Retry.Policies.AnyExceptionPolicy());
        }

        private static (long nextSequenceNumber, long startingTimestampTicks, long lastPartition) GetStartPointers(CloudTable outputTable)
        {
            TableQuery query =
                new TableQuery()
                    .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, BuildPartitionKey(long.MaxValue)),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, AllStreamStateRowKey)))
                    .Take(1);

            var results = outputTable.ExecuteQuery(query, Options).ToList();

            if (results.Count > 0)
            {
                return (results[0].Properties[AllStreamStateNextSequenceNumber].Int64Value!.Value, results[0].Properties[AllStreamStateStartingTimestampTicks].Int64Value!.Value, GetPartitionFromPartitionKey(results[0].PartitionKey));
            }

            return (0, MinAzureUtcDateTicks, 0);
        }

        private static long GetPartitionFromPartitionKey(string partitionKey)
        {
            return long.MaxValue - long.Parse(partitionKey.Substring(AllStreamPartitionKey.Length));
        }

        private static async Task<long> WriteNewCommits(ImmutableArray<CloudTable> inputTables, CloudTable outputTable, MemoryCache commitsWeHaveSeen, long startingTimestampTicks, long startingCommitSequenceNumber)
        {
            IEnumerable<DynamicTableEntity> flattenedCommits = await EnumerateCommits(inputTables, startingTimestampTicks).ConfigureAwait(false);

            var batch = new TableBatchOperation();
            int batchCount = 0;
            long commitSequenceNumber = startingCommitSequenceNumber;
            string partitionKey = BuildPartitionKey(DateTimeOffset.Now.UtcTicks);

            foreach (DynamicTableEntity commit in flattenedCommits)
            {
                (Guid, long) currentCommit = (commit.Properties[TableStorageEventWriter.CommitAggregateId].GuidValue!.Value, commit.Properties[TableStorageEventWriter.CommitSequenceNumber].Int64Value!.Value);

                if (commitsWeHaveSeen.TryGetValue(currentCommit, out (Guid, long) result))
                {
                    continue;
                }

                batchCount++;

                AddInsertOperationToBatch(batch, partitionKey, commitSequenceNumber, commit);
                commitSequenceNumber++;
                SetCacheEntry(commitsWeHaveSeen, currentCommit);

                if (batchCount == MaxBatchSize)
                {
                    await CommitBatch(outputTable, batch, partitionKey, commitSequenceNumber, startingTimestampTicks).ConfigureAwait(false);
                    batchCount = 0;
                    batch = new TableBatchOperation();
                    partitionKey = BuildPartitionKey(DateTimeOffset.Now.UtcTicks);
                }
            }

            if (batchCount > 0)
            {
                await CommitBatch(outputTable, batch, partitionKey, commitSequenceNumber, startingTimestampTicks).ConfigureAwait(false);
            }

            return commitSequenceNumber;
        }

        private static async Task CommitBatch(CloudTable outputTable, TableBatchOperation batch, string partitionKey, long commitSequenceNumber, long startingTimestampTicks)
        {
            var state = new DynamicTableEntity(partitionKey, AllStreamStateRowKey);
            state.Properties.Add(AllStreamStateNextSequenceNumber, new EntityProperty(commitSequenceNumber));
            state.Properties.Add(AllStreamStateStartingTimestampTicks, new EntityProperty(startingTimestampTicks));
            batch.Add(TableOperation.InsertOrReplace(state));
            await outputTable.ExecuteBatchAsync(batch, Options, null).ConfigureAwait(false);
        }

        private static void AddInsertOperationToBatch(TableBatchOperation batch, string partitionKey, long commitSequenceNumber, DynamicTableEntity commit)
        {
            var allStreamCommit = new DynamicTableEntity(partitionKey, commitSequenceNumber.ToString("D21"));
            allStreamCommit.Properties.AddRange(commit.Properties);
            batch.Add(TableOperation.Insert(allStreamCommit));
        }

        private static async Task<IEnumerable<DynamicTableEntity>> EnumerateCommits(ImmutableArray<CloudTable> inputTables, long startingTimestampTicks)
        {
            var tasks = new List<Task<List<DynamicTableEntity>>>();
            foreach (CloudTable table in inputTables)
            {
                TableQuery query = new TableQuery()
                    .Where(
                        TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, CreateDateFromTicksAndZeroOffset(startingTimestampTicks)));
                tasks.Add(Task.Factory.StartNew(() => GetCommitsFor(query, table)));
            }

            List<DynamicTableEntity>[] commits = await Task.WhenAll(tasks).ConfigureAwait(false);
            IEnumerable<DynamicTableEntity> flattenedCommits = commits.SelectMany(i => i);
            return flattenedCommits;
        }

        private static List<DynamicTableEntity> GetCommitsFor(TableQuery query, CloudTable table)
        {
            return table.ExecuteQuery(query, Options).ToList();
        }

        private static MemoryCache BuildCommitsWeHaveSeen(CloudTable outputTable, long latestPartition, int partitionLatency = PartitionLatency)
        {
            var commitsWeHaveSeen = new MemoryCache(optionsAccessor: new MemoryCacheOptions { ExpirationScanFrequency = TimeSpan.FromTicks(TimeInAPartition) });

            TableQuery<DynamicTableEntity> query =
                new TableQuery<DynamicTableEntity>
                {
                    SelectColumns = new List<string> { TableStorageEventMerger<TInputCloudTableFactory, TOutputCloudTableFactory>.AllStreamCommitAggregateId, TableStorageEventMerger<TInputCloudTableFactory, TOutputCloudTableFactory>.AllStreamCommitSequenceNumber },
                }
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, GetPartitionKeyFor(latestPartition + partitionLatency)),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.NotEqual, AllStreamStateRowKey)));

            IEnumerable<DynamicTableEntity> results = outputTable.ExecuteQuery(query, Options);
            foreach (DynamicTableEntity result in results)
            {
                (Guid, long) item = (result.Properties[AllStreamCommitAggregateId].GuidValue!.Value, result.Properties[AllStreamCommitSequenceNumber].Int64Value!.Value);
                SetCacheEntry(commitsWeHaveSeen, item);
            }

            return commitsWeHaveSeen;
        }

        private static (Guid, long) SetCacheEntry(MemoryCache commitsWeHaveSeen, (Guid, long) item)
        {
            return commitsWeHaveSeen.Set(item, item, TimeSpan.FromTicks(TimeInAPartition * PartitionLatency));
        }

        private static string BuildPartitionKey(long startingTimestampTicks)
        {
            long partition = GetPartitionForTimestamp(startingTimestampTicks);
            return GetPartitionKeyFor(partition);
        }

        private static string GetPartitionKeyFor(long partition)
        {
            // We are doing a reverse order for our partition key
            return AllStreamPartitionKey + (long.MaxValue - partition).ToString("D21");
        }

        private static long GetPartitionForTimestamp(long startingTimestampTicks)
        {
            return startingTimestampTicks / TimeInAPartition;
        }

        private static DateTimeOffset CreateDateFromTicksAndZeroOffset(long startingTimestampTicks)
        {
            return new DateTimeOffset(startingTimestampTicks, TimeSpan.Zero);
        }
    }

    /// <summary>
    /// A factory for <see cref="TableStorageEventMerger{TInputCloudTableFactory, TOutputCloudTableFactory}"/>.
    /// </summary>
    public static class TableStorageEventMerger
    {
        /// <summary>
        /// Creates a <see cref="TableStorageEventMerger{TInputCloudTableFactory, TOutputCloudTableFactory}"/> from the given input and output factories.
        /// </summary>
        /// <typeparam name="TInputCloudTableFactory1">The type of the input cloud table factory.</typeparam>
        /// <typeparam name="TOutputCloudTableFactory1">The type of the output cloud table factory.</typeparam>
        /// <param name="inputCloudTableFactory">The input cloud table factory.</param>
        /// <param name="outputCloudTableFactory">The output cloud table factory.</param>
        /// <returns>The instance of the <see cref="TableStorageEventMerger{TInputCloudTableFactory, TOutputCloudTableFactory}"/>.</returns>
        /// <remarks>
        /// You should ensure that the "starting" timestamp is sufficiently far in the past to ensure that you are missing no events across any partition.
        /// </remarks>
        public static TableStorageEventMerger<TInputCloudTableFactory1, TOutputCloudTableFactory1> From<TInputCloudTableFactory1, TOutputCloudTableFactory1>(TInputCloudTableFactory1 inputCloudTableFactory, TOutputCloudTableFactory1 outputCloudTableFactory)
            where TInputCloudTableFactory1 : IEventCloudTableFactory
            where TOutputCloudTableFactory1 : IAllStreamCloudTableFactory
        {
            return new TableStorageEventMerger<TInputCloudTableFactory1, TOutputCloudTableFactory1>(inputCloudTableFactory, outputCloudTableFactory);
        }
    }
}
