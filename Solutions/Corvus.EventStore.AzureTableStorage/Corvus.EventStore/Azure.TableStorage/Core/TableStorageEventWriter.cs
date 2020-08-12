// <copyright file="TableStorageEventWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core
{
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Corvus.EventStore.Azure.TableStorage.Core.Internal;
    using Corvus.EventStore.Core;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// In-memory implementation of <see cref="IEventWriter"/>.
    /// </summary>
    public readonly struct TableStorageEventWriter : IEventWriter
    {
        private readonly IEventCloudTableFactory cloudTableFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageEventWriter"/> struct.
        /// </summary>
        /// <param name="cloudTableFactory">The factory for the underlying cloud table.</param>
        public TableStorageEventWriter(IEventCloudTableFactory cloudTableFactory)
        {
            this.cloudTableFactory = cloudTableFactory;
        }

        /// <inheritdoc/>
        public async Task WriteCommitAsync(Commit commit)
        {
            var commitEntity = new DynamicTableEntity(TableHelpers.BuildPK(commit.AggregateId), TableHelpers.BuildRK(commit.SequenceNumber));
            commitEntity.Properties["Commit" + nameof(Commit.AggregateId)] = new EntityProperty(commit.AggregateId);
            commitEntity.Properties["Commit" + nameof(Commit.PartitionKey)] = new EntityProperty(commit.PartitionKey);
            commitEntity.Properties["Commit" + nameof(Commit.SequenceNumber)] = new EntityProperty(commit.SequenceNumber);
            commitEntity.Properties["Commit" + nameof(Commit.Timestamp)] = new EntityProperty(commit.Timestamp);
            commitEntity.Properties["Commit" + nameof(Commit.Events)] = new EntityProperty(Utf8JsonEventListSerializer.SerializeEventListToString(commit.Events));

            CloudTable table = this.cloudTableFactory.GetTable(commit.AggregateId, commit.PartitionKey);

            var insertOperation = TableOperation.Insert(commitEntity);
            try
            {
                _ = await table.ExecuteAsync(insertOperation).ConfigureAwait(false);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 409)
                {
                    throw new ConcurrencyException($"Unable to write the commit for aggregateID {commit.AggregateId} with sequence number {commit.SequenceNumber}.", ex);
                }

                // Rethrow if we had a general storage exception.
                throw;
            }
        }
    }
}
