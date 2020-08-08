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
            var commitEntity = new CommitEntity(commit);
            CloudTable table = await this.cloudTableFactory.GetTableAsync(commit.AggregateId, commit.PartitionKey).ConfigureAwait(false);

            var insertOperation = TableOperation.Insert(commitEntity);
            try
            {
                _ = await table.ExecuteAsync(insertOperation).ConfigureAwait(false);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 400)
                {
                    throw new ConcurrencyException($"Unable to write the commit for aggregateID {commit.AggregateId} with sequence number {commit.SequenceNumber}.", ex);
                }

                // Rethrow if we had a general storage exception.
                throw;
            }
        }
    }
}
