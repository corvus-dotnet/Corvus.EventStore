// <copyright file="TableStorageEventMerger.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.CommitStore.Azure.TableStorage.Core.Internal;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Corvus.EventStore.Azure.TableStorage.Core.Internal;
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

        // Maximum number of messages in the inner batch
        private const int MaxInnerBatchLength = 500;

        // If we allow 99 items, and a 4MB maximum payload size that seems to permit around 40k for the row payload
        // (which is < the 64k we are allowed for a single column).
        // By the time we have dealt with the json encoding, base64 etc. our
        // base payload length will be about 4 times the raw size calculated. So this gives us a maximum for our batch payload.
        private const int MaxBatchPayloadLength = (int)(40 * 1024 / 4);

        private const int PartitionLatency = 1;

        private const string AllStreamPartitionKey = "allstreamitems";
        private const string AllStreamStateRowKey = "allstreamstate";
        private const string AllStreamCommitAggregateId = "CommitAggregateId";
        private const string AllStreamCommitSequenceNumber = "CommitSequenceNumber";
        private const string AllStreamCommitList = "CommitList";
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
                ProjectSystemProperties = true,
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
            var inputTokens = new TableContinuationToken[inputTables.Length];
            bool[] completedSources = new bool[inputTables.Length];

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

                                long initialTime = DateTimeOffset.Now.UtcTicks;

                                while (true)
                                {
                                    long currentTime = DateTimeOffset.Now.UtcTicks;

                                    startingCommitSequenceNumber = await WriteNewCommits(inputTables, inputTokens, completedSources, outputTable, commitsWeHaveSeen, startingTimestampTicks, startingCommitSequenceNumber).ConfigureAwait(false);
                                    if (commitsWeHaveSeen.Count - previousCount > 0)
                                    {
                                        long elaspedTime = DateTimeOffset.Now.UtcTicks - currentTime;

                                        Console.WriteLine();
                                        Console.WriteLine($"Written {commitsWeHaveSeen.Count - previousCount} commits in {TimeSpan.FromTicks(elaspedTime).TotalSeconds} == {(commitsWeHaveSeen.Count - previousCount) / TimeSpan.FromTicks(elaspedTime).TotalSeconds} commits/s (Total: {commitsWeHaveSeen.Count - originalCount} commits)");
                                        previousCount = commitsWeHaveSeen.Count;
                                    }

                                    // Let's go back and look at the events from the time when we sarted this run through, plus one partition of leeway.
                                    if (completedSources.All(t => t))
                                    {
                                        // Once they are all completed, we go round again with a new starting timestamp
                                        startingTimestampTicks = initialTime - TimeInAPartition;
                                        initialTime = DateTimeOffset.Now.UtcTicks;
                                        ResetCompletedSources(completedSources);
                                    }
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

        private static void ResetCompletedSources(bool[] completedSources)
        {
            for (int i = 0; i < completedSources.Length; ++i)
            {
                completedSources[i] = false;
            }
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

        private static async Task<long> WriteNewCommits(ImmutableArray<CloudTable> inputTables, TableContinuationToken[] inputTokens, bool[] completedSources, CloudTable outputTable, MemoryCache commitsWeHaveSeen, long startingTimestampTicks, long startingCommitSequenceNumber)
        {
            IEnumerable<DynamicTableEntity> flattenedCommits = await EnumerateCommits(inputTables, inputTokens, completedSources, startingTimestampTicks).ConfigureAwait(false);

            var batch = new TableBatchOperation();
            int batchCount = 0;
            long commitSequenceNumber = startingCommitSequenceNumber;
            string partitionKey = BuildPartitionKey(DateTimeOffset.Now.UtcTicks);

            var innerBatch = new List<BatchedCommit>(MaxInnerBatchLength);
            foreach (DynamicTableEntity commit in flattenedCommits)
            {
                (Guid, long) currentCommit = (commit.Properties[TableStorageEventWriter.CommitAggregateId].GuidValue!.Value, commit.Properties[TableStorageEventWriter.CommitSequenceNumber].Int64Value!.Value);

                if (commitsWeHaveSeen.TryGetValue(currentCommit, out (Guid, long) result))
                {
                    continue;
                }

                batchCount += AddInnerBatchOperation(batch, innerBatch, partitionKey, commitSequenceNumber, commit);
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
                CompleteBatch(batch, innerBatch);
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

        private static void CompleteBatch(TableBatchOperation batch, List<BatchedCommit> innerBatch)
        {
            if (innerBatch.Count == 0)
            {
                return;
            }

            // We take the PK and RK from our initial batch value (so we will have gaps in the sequence number corresponding to the batch length)
            var commitBatch = new DynamicTableEntity(innerBatch[0].PartitionKey, innerBatch[0].RowKey);
            byte[] serializedBatch = Utf8JsonCommitListSerializer.SerializeCommitList(innerBatch);
            commitBatch.Properties.Add(TableStorageEventMerger<TInputCloudTableFactory, TOutputCloudTableFactory>.AllStreamCommitList, new EntityProperty(serializedBatch));
            batch.Add(TableOperation.Insert(commitBatch));
            innerBatch.Clear();
        }

        private static int AddInnerBatchOperation(TableBatchOperation batch, List<BatchedCommit> innerBatch, string partitionKey, long commitSequenceNumber, DynamicTableEntity commit)
        {
            int result = 0;
            var allStreamCommit = new BatchedCommit(commit, partitionKey, commitSequenceNumber);
            if (innerBatch.Count == MaxInnerBatchLength || innerBatch.Sum(i => i.Length) + allStreamCommit.Length > MaxBatchPayloadLength)
            {
                CompleteBatch(batch, innerBatch);
                result = 1;
            }

            innerBatch.Add(allStreamCommit);
            return result;
        }

        private static async Task<IEnumerable<DynamicTableEntity>> EnumerateCommits(ImmutableArray<CloudTable> inputTables, TableContinuationToken?[] inputTokens, bool[] completedSources, long startingTimestampTicks)
        {
            var tasks = new List<Task<TableQuerySegment<DynamicTableEntity>?>>();
            for (int i = 0; i < inputTables.Length; ++i)
            {
                CloudTable table = inputTables[i];
                TableContinuationToken? token = inputTokens[i];

                if (completedSources[i])
                {
                    tasks.Add(Task.FromResult<TableQuerySegment<DynamicTableEntity>?>(null));
                    continue;
                }

                TableQuery query = new TableQuery()
                    .Where(
                        TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, CreateDateFromTicksAndZeroOffset(startingTimestampTicks)));

                tasks.Add(GetCommitsFor(query, table, token));
            }

            Console.WriteLine("Finding new commits.");

            var sw = Stopwatch.StartNew();
            TableQuerySegment<DynamicTableEntity>?[] commits = await Task.WhenAll(tasks).ConfigureAwait(false);

            for (int i = 0; i < commits.Length; ++i)
            {
                TableQuerySegment<DynamicTableEntity>? commit = commits[i];

                if (commit is null)
                {
                    continue;
                }

                inputTokens[i] = commit.ContinuationToken;
                if (commit.ContinuationToken is null)
                {
                    completedSources[i] = true;
                }
            }

            IEnumerable<DynamicTableEntity> flattenedCommits = commits.SelectMany(i => i?.Results ?? Enumerable.Empty<DynamicTableEntity>());
            sw.Stop();
            Console.WriteLine($"Found {flattenedCommits.Count()} in {sw.ElapsedMilliseconds / 1000.0} seconds");
            return flattenedCommits;
        }

        private static Task<TableQuerySegment<DynamicTableEntity>?> GetCommitsFor(TableQuery query, CloudTable table, TableContinuationToken? token)
        {
            return table.ExecuteQuerySegmentedAsync(query, token, Options, null);
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

            query.SelectColumns.Add(AllStreamCommitList);

            IEnumerable<DynamicTableEntity> results = outputTable.ExecuteQuery(query, Options);
            foreach (DynamicTableEntity batch in results)
            {
                List<BatchedCommit> commits = Utf8JsonCommitListSerializer.DeserializeCommitList(batch.Properties[AllStreamCommitList].BinaryValue);
                foreach (BatchedCommit result in commits)
                {
                    (Guid, long) item = (result.CommitAggregateId, result.CommitSequenceNumber);
                    SetCacheEntry(commitsWeHaveSeen, item);
                }
            }

            return commitsWeHaveSeen;
        }

        private static void SetCacheEntry(MemoryCache commitsWeHaveSeen, (Guid, long) item)
        {
            commitsWeHaveSeen.Set(item, true, TimeSpan.FromTicks(TimeInAPartition * (PartitionLatency + 1)));
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
