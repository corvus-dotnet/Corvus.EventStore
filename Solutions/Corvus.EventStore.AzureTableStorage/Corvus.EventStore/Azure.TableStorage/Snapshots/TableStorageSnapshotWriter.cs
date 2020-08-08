// <copyright file="TableStorageSnapshotWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Snapshots
{
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Corvus.EventStore.Azure.TableStorage.Snapshots.Internal;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Snapshots;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// In-memory implementation of <see cref="ISnapshotWriter"/>.
    /// </summary>
    public readonly struct TableStorageSnapshotWriter : ISnapshotWriter
    {
        private readonly ISnapshotCloudTableFactory cloudTableFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageSnapshotWriter"/> struct.
        /// </summary>
        /// <param name="cloudTableFactory">The factory for the container for the snapshots.</param>
        public TableStorageSnapshotWriter(ISnapshotCloudTableFactory cloudTableFactory)
        {
            this.cloudTableFactory = cloudTableFactory;
        }

        /// <inheritdoc/>
        public async Task WriteAsync(SerializedSnapshot snapshot)
        {
            CloudTable table = await this.cloudTableFactory.GetTableAsync(snapshot.AggregateId, snapshot.PartitionKey).ConfigureAwait(false);
            var entity = new DynamicTableEntity(TableHelpers.BuildPK(snapshot.AggregateId), TableHelpers.BuildRK(snapshot.CommitSequenceNumber));
            entity.Properties["Snapshot" + nameof(snapshot.AggregateId)] = new EntityProperty(snapshot.AggregateId);
            entity.Properties["Snapshot" + nameof(snapshot.PartitionKey)] = new EntityProperty(snapshot.PartitionKey);
            entity.Properties["Snapshot" + nameof(snapshot.CommitSequenceNumber)] = new EntityProperty(snapshot.CommitSequenceNumber);
            entity.Properties["Snapshot" + nameof(snapshot.EventSequenceNumber)] = new EntityProperty(snapshot.EventSequenceNumber);
            entity.Properties["Snapshot" + nameof(snapshot.Memento)] = new EntityProperty(snapshot.Memento.ToArray());
            var insertOperation = TableOperation.Insert(entity);
            try
            {
                _ = await table.ExecuteAsync(insertOperation).ConfigureAwait(false);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 400)
                {
                    throw new ConcurrencyException($"Unable to write the snapshot for aggregateID {snapshot.AggregateId} with commit sequence number {snapshot.CommitSequenceNumber}.", ex);
                }

                // Rethrow if we had a general storage exception.
                throw;
            }
        }
    }
}
