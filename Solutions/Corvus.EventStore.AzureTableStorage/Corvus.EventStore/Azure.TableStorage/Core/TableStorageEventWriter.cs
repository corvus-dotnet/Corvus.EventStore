// <copyright file="TableStorageEventWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core
{
    using System;
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
        /// <summary>
        /// The name of the commit aggregate ID property.
        /// </summary>
        public const string CommitAggregateId = "Commit" + nameof(Commit.AggregateId);

        /// <summary>
        /// The name of the commit parittion key property.
        /// </summary>
        public const string CommitPartitionKey = "Commit" + nameof(Commit.PartitionKey);

        /// <summary>
        /// The name of the commit timestamp property.
        /// </summary>
        public const string CommitTimestamp = "Commit" + nameof(Commit.Timestamp);

        /// <summary>
        /// The name of the commit events property.
        /// </summary>
        public const string CommitEvents = "Commit" + nameof(Commit.Events);

        /// <summary>
        /// The name of the commit sequence number property.
        /// </summary>
        public const string CommitSequenceNumber = "Commit" + nameof(Commit.SequenceNumber);

        private static readonly TableRequestOptions Options =
            new TableRequestOptions
            {
            };

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
            commitEntity.Properties[CommitAggregateId] = new EntityProperty(commit.AggregateId);
            commitEntity.Properties[CommitPartitionKey] = new EntityProperty(commit.PartitionKey);
            commitEntity.Properties[CommitSequenceNumber] = new EntityProperty(commit.SequenceNumber);
            commitEntity.Properties[CommitTimestamp] = new EntityProperty(commit.Timestamp);
            commitEntity.Properties[CommitEvents] = new EntityProperty(Utf8JsonEventListSerializer.SerializeEventList(commit.Events));

            CloudTable table = this.cloudTableFactory.GetTable(commit.AggregateId, commit.PartitionKey);

            var insertOperation = TableOperation.Insert(commitEntity);
            try
            {
                _ = await table.ExecuteAsync(insertOperation, Options, null).ConfigureAwait(false);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 409)
                {
                    TableResult result = await table.ExecuteAsync(TableOperation.Retrieve<DynamicTableEntity>(commitEntity.PartitionKey, commitEntity.RowKey)).ConfigureAwait(false);
                    var storedEntity = (DynamicTableEntity)result.Result;
                    if (storedEntity.Properties[CommitTimestamp].Int64Value != commitEntity.Properties[CommitTimestamp].Int64Value ||
                        storedEntity.Properties[CommitEvents].BinaryValue != commitEntity.Properties[CommitEvents].BinaryValue)
                    {
                        throw new ConcurrencyException($"Unable to write the commit for aggregateID {commit.AggregateId} with sequence number {commit.SequenceNumber}.", ex);
                    }

                    // We actually stored it successfully, so just continue.
                    return;
                }

                // Rethrow if we had a general storage exception.
                throw;
            }
        }
    }
}
