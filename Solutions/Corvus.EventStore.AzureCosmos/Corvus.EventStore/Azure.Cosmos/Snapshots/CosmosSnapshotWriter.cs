// <copyright file="CosmosSnapshotWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.Snapshots
{
    using System.Net;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.Cosmos.ContainerFactories;
    using Corvus.EventStore.Azure.Cosmos.Snapshots.Internal;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Snapshots;
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// In-memory implementation of <see cref="ISnapshotWriter"/>.
    /// </summary>
    public readonly struct CosmosSnapshotWriter : ISnapshotWriter
    {
        private readonly ISnapshotContainerFactory containerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosSnapshotWriter"/> struct.
        /// </summary>
        /// <param name="containerFactory">The factory for the container for the snapshots.</param>
        public CosmosSnapshotWriter(ISnapshotContainerFactory containerFactory)
        {
            this.containerFactory = containerFactory;
        }

        /// <inheritdoc/>
        public async Task WriteAsync(SerializedSnapshot snapshot)
        {
            Container container = this.containerFactory.GetContainer();
            try
            {
                await container.CreateItemAsync(new SnapshotDocument(snapshot));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                throw new ConcurrencyException($"Unable to write the snapshot for aggregateID {snapshot.AggregateId} with commit sequence number {snapshot.CommitSequenceNumber}.", ex);
            }
        }
    }
}
