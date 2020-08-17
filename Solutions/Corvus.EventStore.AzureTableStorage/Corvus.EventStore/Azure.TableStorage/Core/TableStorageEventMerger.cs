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
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Corvus.EventStore.Core;
    using Corvus.Extensions;
    using Corvus.Retry;
    using Microsoft.Azure.Cosmos.Table;

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

        // This is the maximum amount of time we may have to look back on any instance
        // for items with aged timestamps due to retrying commits.
        private static readonly long MaxTimeoutDelay = TimeSpan.FromMilliseconds(30000).Ticks;

        // Ensure we flush at least once a second
        private static readonly TimeSpan FlushTimeout = TimeSpan.FromMilliseconds(1000);

        private static readonly TimeSpan DelayOnNoResults = TimeSpan.FromMilliseconds(250);

        private static readonly long MinAzureUtcDateTicks = new DateTimeOffset(1601, 1, 1, 0, 0, 0, TimeSpan.Zero).UtcTicks;

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
            string pk = "allstreamitems";
            string rk = "latestinternalcheckpoint";

            ImmutableArray<CloudTable> inputTables = inputFactory.GetTables();
            CloudTable outputTable = outputFactory.GetTable();

            return ReliableTaskRunner.Run(
                token =>
                {
                    return Task.Factory.StartNew<Task<Task>>(
                        async () =>
                        {
                            // First, load the last checkpoint
                            var operation = TableOperation.Retrieve<DynamicTableEntity>(pk, rk);
                            TableResult tableResult = await outputTable.ExecuteAsync(operation).ConfigureAwait(false);
                            DynamicTableEntity checkpointsEntity = (DynamicTableEntity)tableResult.Result ?? new DynamicTableEntity(pk, rk);

                            long[] checkpointTimestamps = Enumerable.Repeat(MinAzureUtcDateTicks + MaxTimeoutDelay, inputTables.Length).ToArray();

                            long allStreamIndex = 0;

                            ImmutableHashSet<(Guid, long)> lastFoundItems = ImmutableHashSet<(Guid, long)>.Empty;

                            if (tableResult.HttpStatusCode == 200)
                            {
                                for (int i = 0; i < checkpointTimestamps.Length; ++i)
                                {
                                    checkpointTimestamps[i] = checkpointsEntity.Properties[$"checkpointtimestamp{i}"].Int64Value!.Value;
                                }

                                allStreamIndex = checkpointsEntity.Properties["allStreamIndex"].Int64Value!.Value;

                                string filter = string.Empty;

                                for (int sourceIndex = 0; sourceIndex < checkpointTimestamps.Length; ++sourceIndex)
                                {
                                    string newFilter =
                                    TableQuery.CombineFilters(
                                        TableQuery.GenerateFilterConditionForDate("CommitOriginalTimestamp", QueryComparisons.GreaterThan, new DateTimeOffset(checkpointTimestamps[sourceIndex] - MaxTimeoutDelay, TimeSpan.Zero)),
                                        TableOperators.And,
                                        TableQuery.GenerateFilterConditionForInt("CommitOriginalSource", QueryComparisons.Equal, sourceIndex));
                                    if (filter.Length == 0)
                                    {
                                        filter = newFilter;
                                    }
                                    else
                                    {
                                        filter = TableQuery.CombineFilters(filter, TableOperators.Or, newFilter);
                                    }
                                }

                                Console.WriteLine(filter);

                                TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>().Where(filter);
                                IEnumerable<DynamicTableEntity> results = outputTable.ExecuteQuery(query);
                                ImmutableHashSet<(Guid, long)>.Builder builder = ImmutableHashSet.CreateBuilder<(Guid, long)>();

                                foreach (DynamicTableEntity? result in results)
                                {
                                    builder.Add((result.Properties["Commit" + nameof(Commit.AggregateId)].GuidValue!.Value, result.Properties["Commit" + nameof(Commit.SequenceNumber)].Int64Value!.Value));
                                }

                                lastFoundItems = builder.ToImmutable();
                            }
                            else
                            {
                                checkpointsEntity = new DynamicTableEntity(pk, rk);

                                // Set up the allstream index and checkpoints
                                for (int i = 0; i < checkpointTimestamps.Length; ++i)
                                {
                                    checkpointsEntity.Properties[$"checkpointtimestamp{i}"] = new EntityProperty(checkpointTimestamps[i]);
                                }

                                checkpointsEntity.Properties["allStreamIndex"] = new EntityProperty(allStreamIndex);
                            }

                            var queries = new TableQuery<DynamicTableEntity>[inputTables.Length];
                            var currentTokens = new TableContinuationToken?[inputTables.Length];
                            var tasks = new Task<TableQuerySegment<DynamicTableEntity>?>[inputTables.Length];

                            DateTimeOffset lastFlush = DateTimeOffset.Now;
                            int batchCount = 0;
                            var batch = new TableBatchOperation();
                            Task<TableBatchResult>? batchTask = null;

                            while (true)
                            {
                                ImmutableHashSet<(Guid, long)>.Builder currentFoundItems = ImmutableHashSet.CreateBuilder<(Guid, long)>();
                                try
                                {
                                    bool firstTime = true;

                                    // Create the queries for each of the tables in the input stream.
                                    for (int i = 0; i < queries.Length; ++i)
                                    {
                                        queries[i] =
                                            new TableQuery<DynamicTableEntity>()
                                                 .Where(
                                                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThan, new DateTimeOffset(checkpointTimestamps[i] - MaxTimeoutDelay, TimeSpan.Zero)))
                                                 .OrderBy("Timestamp");
                                    }

                                    bool foundResults = false;

                                    do
                                    {
                                        // Execute the queries
                                        for (int i = 0; i < queries.Length; ++i)
                                        {
                                            if (firstTime || currentTokens[i] != null)
                                            {
                                                tasks[i] = inputTables[i].ExecuteQuerySegmentedAsync(queries[i], currentTokens[i]);
                                            }
                                            else
                                            {
                                                tasks[i] = Task.FromResult<TableQuerySegment<DynamicTableEntity>?>(null);
                                            }
                                        }

                                        var sw = Stopwatch.StartNew();
                                        TableQuerySegment<DynamicTableEntity>?[] segments = await Task.WhenAll(tasks).ConfigureAwait(false);
                                        sw.Stop();

                                        for (int i = 0; i < segments.Length; ++i)
                                        {
                                            TableQuerySegment<DynamicTableEntity>? segment = segments[i];
                                            if (segment is null)
                                            {
                                                currentTokens[i] = null;
                                                continue;
                                            }

                                            currentTokens[i] = segment.ContinuationToken;
                                            foreach (DynamicTableEntity result in segment.Results.OrderBy(r => r.Timestamp))
                                            {
                                                (Guid, long) foundItem = (result.Properties["Commit" + nameof(Commit.AggregateId)].GuidValue!.Value, result.Properties["Commit" + nameof(Commit.SequenceNumber)].Int64Value!.Value);
                                                currentFoundItems.Add(foundItem);

                                                if (!lastFoundItems.Contains(foundItem))
                                                {
                                                    var dte = new DynamicTableEntity(pk, allStreamIndex.ToString("D21"));
                                                    dte.Properties.AddRange(result.Properties);
                                                    dte.Properties.Add("CommitOriginalTimestamp", new EntityProperty(result.Timestamp));
                                                    dte.Properties.Add("CommitSource", new EntityProperty(i));
                                                    foundResults = true;
                                                    allStreamIndex += 1;
                                                    checkpointTimestamps[i] = Math.Max(checkpointTimestamps[i], result.Timestamp.UtcTicks);
                                                    var insertOperation = TableOperation.Insert(dte);
                                                    batch.Add(insertOperation);
                                                    batchCount++;

                                                    if (batchCount == MaxBatchSize)
                                                    {
                                                        // Only update the all stream index while we are mid-query segment
                                                        checkpointsEntity.Properties["allStreamIndex"] = new EntityProperty(allStreamIndex);
                                                        var checkpointOperation = TableOperation.InsertOrReplace(checkpointsEntity);
                                                        batch.Add(checkpointOperation);
                                                        if (batchTask != null)
                                                        {
                                                            // Wait for the previous insert to complete.
                                                            await HandleBatchResult(batchTask).ConfigureAwait(false);
                                                        }

                                                        // Stash it away and carry on.
                                                        batchTask = outputTable.ExecuteBatchAsync(batch);
                                                        batch = new TableBatchOperation();
                                                        batchCount = 0;
                                                        lastFlush = DateTimeOffset.Now;
                                                    }
                                                }
                                            }
                                        }

                                        firstTime = false;

                                        if (token.IsCancellationRequested)
                                        {
                                            return Task.CompletedTask;
                                        }
                                    }
                                    while (currentTokens.Any(currentToken => currentToken != null));

                                    if (batchTask != null)
                                    {
                                        // Wait for the previous insert to complete.
                                        await HandleBatchResult(batchTask).ConfigureAwait(false);
                                        batchTask = null;
                                    }

                                    if (DateTimeOffset.Now - lastFlush > FlushTimeout)
                                    {
                                        UpdateCheckpointsEntity(checkpointsEntity, checkpointTimestamps, allStreamIndex);
                                        var writeOperation = TableOperation.InsertOrReplace(checkpointsEntity);
                                        batch.Add(writeOperation);
                                        await outputTable.ExecuteBatchAsync(batch).ConfigureAwait(false);
                                        batch = new TableBatchOperation();
                                        batchCount = 0;
                                        lastFlush = DateTimeOffset.Now;
                                    }

                                    if (!foundResults)
                                    {
                                        await Task.Delay(DelayOnNoResults).ConfigureAwait(false);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }

                                lastFoundItems = currentFoundItems.ToImmutable();
                            }
                        }, token);
                },
                new Retry.Policies.AnyExceptionPolicy());
        }

        private static async Task HandleBatchResult(Task<TableBatchResult> batchTask)
        {
            TableBatchResult batchResults = await batchTask.ConfigureAwait(false);
            foreach (TableResult batchResult in batchResults)
            {
                if (batchResult.HttpStatusCode < 200 || batchResult.HttpStatusCode > 299)
                {
                    throw new Exception($"Batch failed with HttpStatusCode {batchResult.HttpStatusCode}.");
                }
            }
        }

        private static void UpdateCheckpointsEntity(DynamicTableEntity checkpointsEntity, long[] checkpointTimestamps, long allStreamIndex)
        {
            for (int j = 0; j < checkpointTimestamps.Length; ++j)
            {
                checkpointsEntity.Properties[$"checkpointtimestamp{j}"] = new EntityProperty(checkpointTimestamps[j]);
            }

            checkpointsEntity.Properties["allStreamIndex"] = new EntityProperty(allStreamIndex);
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
        public static TableStorageEventMerger<TInputCloudTableFactory1, TOutputCloudTableFactory1> From<TInputCloudTableFactory1, TOutputCloudTableFactory1>(TInputCloudTableFactory1 inputCloudTableFactory, TOutputCloudTableFactory1 outputCloudTableFactory)
            where TInputCloudTableFactory1 : IEventCloudTableFactory
            where TOutputCloudTableFactory1 : IAllStreamCloudTableFactory
        {
            return new TableStorageEventMerger<TInputCloudTableFactory1, TOutputCloudTableFactory1>(inputCloudTableFactory, outputCloudTableFactory);
        }
    }
}
