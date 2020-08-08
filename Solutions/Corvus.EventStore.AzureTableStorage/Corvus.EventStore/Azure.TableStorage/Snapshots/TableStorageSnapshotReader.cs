// <copyright file="TableStorageSnapshotReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Snapshots
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Corvus.EventStore.Azure.TableStorage.Core.Internal;
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
            CloudTable cloudTable = await this.cloudTableFactory.GetTableAsync(aggregateId, partitionKey).ConfigureAwait(false);
            TableQuery<SerializedSnapshotEntity>? query = new TableQuery<SerializedSnapshotEntity>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, CommitEntity.BuildPK(aggregateId)),
                    TableOperators.And,
                    //// Note that this is GreaterThanOrEqual because our RK is a reversed version of the sequence number to permit for "most recent"
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, CommitEntity.BuildRK(atSequenceId))));

            TableQuerySegment<SerializedSnapshotEntity> result = await cloudTable.ExecuteQuerySegmentedAsync(query, null);
            if (!result.Any())
            {
                return SerializedSnapshot.Empty(aggregateId, partitionKey);
            }

            return result.First().OriginalEntity;
        }
    }
}
