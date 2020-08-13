// <copyright file="TableStorageEventMerger.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
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

            return ReliableTaskRunner.Run(token =>
            {
                return Task.Factory.StartNew(
                    async () =>
                    {
                        // First, load the last checkpoint
                        var operation = TableOperation.Retrieve<DynamicTableEntity>(pk, rk);
                        TableResult tableResult = await outputTable.ExecuteAsync(operation).ConfigureAwait(false);
                        DynamicTableEntity checkpointsEntity = (DynamicTableEntity)tableResult.Result ?? new DynamicTableEntity(pk, rk);

                        long[] checkpointTimestamps = Enumerable.Repeat(MinAzureUtcDateTicks, inputTables.Length).ToArray();

                        long allStreamIndex = 0;

                        if (tableResult.HttpStatusCode == 200)
                        {
                            for (int i = 0; i < checkpointTimestamps.Length; ++i)
                            {
                                checkpointTimestamps[i] = checkpointsEntity.Properties[$"checkpointtimestamp{i}"].Int64Value!.Value;
                            }

                            allStreamIndex = checkpointsEntity.Properties["allStreamIndex"].Int64Value!.Value;
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

                        while (true)
                        {
                            bool firstTime = true;

                            // Create the queries for each of the tables in the input stream.
                            for (int i = 0; i < queries.Length; ++i)
                            {
                                queries[i] =
                                    new TableQuery<DynamicTableEntity>()
                                         .Where(TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThan, new DateTimeOffset(checkpointTimestamps[i], TimeSpan.Zero)))
                                         .OrderBy("Timestamp");
                            }

                            var batch = new TableBatchOperation();
                            int batchCount = 0;
                            Task? batchTask = null;

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

                                TableQuerySegment<DynamicTableEntity>?[] segments = await Task.WhenAll(tasks).ConfigureAwait(false);

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
                                        result.PartitionKey = pk;
                                        result.RowKey = allStreamIndex.ToString("D21");
                                        allStreamIndex += 1;
                                        checkpointTimestamps[i] = result.Timestamp.UtcTicks;
                                        var insertOperation = TableOperation.InsertOrReplace(result);

                                        batch.Add(insertOperation);
                                        batchCount++;
                                        if (batchCount == MaxBatchSize)
                                        {
                                            // Add an extra item to the last batch to update the checkpoint.
                                            checkpointsEntity.Properties[$"checkpointtimestamp{i}"] = new EntityProperty(checkpointTimestamps[i]);
                                            checkpointsEntity.Properties["allStreamIndex"] = new EntityProperty(allStreamIndex);
                                            var writeOperation = TableOperation.InsertOrReplace(checkpointsEntity);
                                            batch.Add(writeOperation);
                                            if (batchTask != null)
                                            {
                                                // Wait for the previous insert to complete.
                                                await batchTask.ConfigureAwait(false);
                                            }

                                            // Stash it away and carry on.
                                            batchTask = outputTable.ExecuteBatchAsync(batch);
                                            batch = new TableBatchOperation();
                                            batchCount = 0;
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
                                await batchTask.ConfigureAwait(false);

                                if (batch.Count > 0)
                                {
                                    for (int i = 0; i < checkpointTimestamps.Length; ++i)
                                    {
                                        checkpointsEntity.Properties[$"checkpointtimestamp{i}"] = new EntityProperty(checkpointTimestamps[i]);
                                    }

                                    checkpointsEntity.Properties["allStreamIndex"] = new EntityProperty(allStreamIndex);
                                    var writeOperation = TableOperation.InsertOrReplace(checkpointsEntity);
                                    batch.Add(writeOperation);
                                    await outputTable.ExecuteBatchAsync(batch).ConfigureAwait(false);
                                }
                            }
                        }
                    }, token);
            });
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
