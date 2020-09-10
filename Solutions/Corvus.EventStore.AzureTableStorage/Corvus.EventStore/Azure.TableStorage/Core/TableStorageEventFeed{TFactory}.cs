// <copyright file="TableStorageEventFeed{TFactory}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Corvus.EventStore.Azure.TableStorage.Core.Internal;
    using Corvus.EventStore.Core;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// An event feed over an "AllStream" cloud table.
    /// </summary>
    /// <typeparam name="TFactory">The type of the <see cref="IAllStreamCloudTableFactory"/>.</typeparam>
    public class TableStorageEventFeed<TFactory> : IEventFeed
        where TFactory : IAllStreamCloudTableFactory
    {
        private readonly TFactory factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageEventFeed{TFactory}"/> class.
        /// </summary>
        /// <param name="factory">The <see cref="IAllStreamCloudTableFactory"/>.</param>
        internal TableStorageEventFeed(TFactory factory)
        {
            this.factory = factory;
        }

        /// <inheritdoc/>
        public ValueTask<EventFeedResult> Get(EventFeedFilter filter, int maxItems)
        {
            return this.Get(filter.AggregateIds, filter.PartitionKeys, maxItems, TableStorageEventMerger.GetPartitionForTimestamp(this.factory.GetCreationTimestamp()), 0L, null);
        }

        /// <inheritdoc/>
        public ValueTask<EventFeedResult> Get(ReadOnlyMemory<byte> fromCheckpoint)
        {
            Checkpoint cp = JsonSerializer.Deserialize<Checkpoint>(fromCheckpoint.Span);
            return this.Get(cp.AggregateIds, cp.PartitionKeys, cp.MaxItems, cp.AllStreamPartition, cp.AllStreamSequenceNumber, cp.TableContinuationToken);
        }

        private async ValueTask<EventFeedResult> Get(ImmutableArray<Guid> aggregateIds, ImmutableArray<string> partitionKeys, int maxItems, long allStreamPartition, long allStreamSequenceNumber, TableContinuationToken? tableContinuationToken)
        {
            var commits = new List<Commit>();
            long currentTimestamp = DateTimeOffset.Now.UtcTicks;

            CloudTable cloudTable = this.factory.GetTable();
            TableQuery<DynamicTableEntity> query = TableStorageEventMerger.GetQueryForPartition(allStreamPartition, allStreamSequenceNumber);

            var options = new TableRequestOptions { TableQueryMaxItemCount = maxItems };
            TableQuerySegment<DynamicTableEntity> result = await cloudTable.ExecuteQuerySegmentedAsync(query, tableContinuationToken, options, null).ConfigureAwait(false);

            bool hasMoreInSegment = false;

            foreach (DynamicTableEntity batch in result.Results)
            {
                List<BatchedCommit> batchedCommits = TableStorageEventMerger.GetBatchedCommits(batch);
                foreach (BatchedCommit batchedCommit in batchedCommits)
                {
                    // Filter the items.
                    if (batchedCommit.CommitSequenceNumber < allStreamSequenceNumber)
                    {
                        continue;
                    }

                    if (!aggregateIds.IsEmpty && !aggregateIds.Contains(batchedCommit.CommitAggregateId))
                    {
                        continue;
                    }

                    if (!partitionKeys.IsEmpty && !partitionKeys.Contains(batchedCommit.CommitPartitionKey))
                    {
                        continue;
                    }

                    commits.Add(new Commit(batchedCommit.CommitAggregateId, batchedCommit.CommitPartitionKey, batchedCommit.CommitSequenceNumber, batchedCommit.CommitTimestamp, Utf8JsonEventListSerializer.DeserializeEventList(batchedCommit.CommitEvents.Span)));
                    if (commits.Count == maxItems)
                    {
                        hasMoreInSegment = true;
                        break;
                    }
                }
            }

            TableContinuationToken? newToken = tableContinuationToken;

            if (!hasMoreInSegment && result.ContinuationToken is null && TableStorageEventMerger.GetPartitionForTimestamp(currentTimestamp) > allStreamPartition)
            {
                // Increment the partition if we have no more results in this segment, and there are no more segments in this partition, and we are not looking at the latest partition.
                allStreamPartition += 1;
                newToken = null;
            }
            else if (!hasMoreInSegment)
            {
                newToken = result.ContinuationToken;
            }

            if (commits.Count > 0)
            {
                // Update to the next commit sequence number
                allStreamSequenceNumber = commits[^1].SequenceNumber + 1;
            }

            var checkpoint = new Checkpoint(allStreamPartition, allStreamSequenceNumber, aggregateIds, partitionKeys, maxItems, result.ContinuationToken);

            return new EventFeedResult(commits, JsonSerializer.SerializeToUtf8Bytes(checkpoint));
        }

        private struct Checkpoint
        {
            public Checkpoint(long allStreamPartition, long allStreamSequenceNumber, ImmutableArray<Guid> aggregateIds, ImmutableArray<string> partitionKeys, int maxItems, TableContinuationToken? tableContinuationToken)
            {
                this.AllStreamPartition = allStreamPartition;
                this.AllStreamSequenceNumber = allStreamSequenceNumber;
                this.AggregateIds = aggregateIds;
                this.PartitionKeys = partitionKeys;
                this.MaxItems = maxItems;
                this.TableContinuationToken = tableContinuationToken;
            }

            public long AllStreamPartition { get; set; }

            public long AllStreamSequenceNumber { get; set; }

            public ImmutableArray<Guid> AggregateIds { get; set; }

            public ImmutableArray<string> PartitionKeys { get; set; }

            public int MaxItems { get; set; }

            public TableContinuationToken? TableContinuationToken { get; }
        }
    }
}
