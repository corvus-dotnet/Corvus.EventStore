// <copyright file="TableStorageSnapshotReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Snapshots
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Corvus.EventStore.Azure.TableStorage.Snapshots.Internal;
    using Corvus.EventStore.Snapshots;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// In-memory implementation of <see cref="ISnapshotReader"/>.
    /// </summary>
    public readonly struct TableStorageSnapshotReader : ISnapshotReader
    {
        private readonly ISnapshotCloudTableFactory cloudTableFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageSnapshotReader"/> struct.
        /// </summary>
        /// <param name="cloudTableFactory">The factory for the cloud table for this store.</param>
        public TableStorageSnapshotReader(ISnapshotCloudTableFactory cloudTableFactory)
        {
            this.cloudTableFactory = cloudTableFactory;
        }

        /// <inheritdoc/>
        public async ValueTask<SerializedSnapshot> ReadAsync(Guid aggregateId, string partitionKey, long atSequenceId = long.MaxValue)
        {
            CloudTable cloudTable = this.cloudTableFactory.GetTable(aggregateId, partitionKey);
            TableQuery? query = new TableQuery().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, TableHelpers.BuildPK(aggregateId)),
                    TableOperators.And,
                    //// Note that this is GreaterThanOrEqual because our RK is a reversed version of the sequence number to permit for "most recent"
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, TableHelpers.BuildRK(atSequenceId))));

            TableQuerySegment<DynamicTableEntity> result = await cloudTable.ExecuteQuerySegmentedAsync(query, null);
            if (!result.Any())
            {
                return SerializedSnapshot.Empty(aggregateId, partitionKey);
            }

            DynamicTableEntity entity = result.First();
            return new SerializedSnapshot(
                entity.Properties["Snapshot" + nameof(SerializedSnapshot.AggregateId)].GuidValue!.Value,
                entity.Properties["Snapshot" + nameof(SerializedSnapshot.PartitionKey)].StringValue,
                entity.Properties["Snapshot" + nameof(SerializedSnapshot.CommitSequenceNumber)].Int64Value!.Value,
                entity.Properties["Snapshot" + nameof(SerializedSnapshot.EventSequenceNumber)].Int64Value!.Value,
                entity.Properties["Snapshot" + nameof(SerializedSnapshot.Memento)].BinaryValue);
        }
    }
}
