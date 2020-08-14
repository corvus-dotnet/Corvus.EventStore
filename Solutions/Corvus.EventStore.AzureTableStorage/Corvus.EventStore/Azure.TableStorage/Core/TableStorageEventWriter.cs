﻿// <copyright file="TableStorageEventWriter.cs" company="Endjin Limited">
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
        private static readonly TableRequestOptions Options =
            new TableRequestOptions
            {
                MaximumExecutionTime = TimeSpan.FromMilliseconds(1500),
                ServerTimeout = TimeSpan.FromMilliseconds(500),
                RetryPolicy = new NoRetry(),
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
            commitEntity.Properties["Commit" + nameof(Commit.AggregateId)] = new EntityProperty(commit.AggregateId);
            commitEntity.Properties["Commit" + nameof(Commit.PartitionKey)] = new EntityProperty(commit.PartitionKey);
            commitEntity.Properties["Commit" + nameof(Commit.SequenceNumber)] = new EntityProperty(commit.SequenceNumber);
            commitEntity.Properties["Commit" + nameof(Commit.Timestamp)] = new EntityProperty(commit.Timestamp);
            commitEntity.Properties["Commit" + nameof(Commit.Events)] = new EntityProperty(Utf8JsonEventListSerializer.SerializeEventListToString(commit.Events));

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
                    if (storedEntity.Properties["Commit" + nameof(Commit.Timestamp)].Int64Value != commitEntity.Properties["Commit" + nameof(Commit.Timestamp)].Int64Value ||
                        storedEntity.Properties["Commit" + nameof(Commit.Events)].StringValue != commitEntity.Properties["Commit" + nameof(Commit.Events)].StringValue)
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
