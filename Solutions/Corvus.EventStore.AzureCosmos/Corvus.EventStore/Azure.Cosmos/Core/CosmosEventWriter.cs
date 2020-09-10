// <copyright file="CosmosEventWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.Core
{
    using System.Net;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.Cosmos.ContainerFactories;
    using Corvus.EventStore.Azure.Cosmos.Core.Internal;
    using Corvus.EventStore.Core;
    using global::Azure.Cosmos;

    /// <summary>
    /// Implements an event writer over Cosmos DBV SQL API.
    /// </summary>
    public readonly struct CosmosEventWriter : IEventWriter
    {
        private readonly IEventContainerFactory containerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosEventWriter"/> struct.
        /// </summary>
        /// <param name="containerFactory">The container factory to use.</param>
        public CosmosEventWriter(IEventContainerFactory containerFactory)
        {
            this.containerFactory = containerFactory;
        }

        /// <inheritdoc/>
        public async Task WriteCommitAsync(Commit commit)
        {
            Container container = this.containerFactory.GetContainer();
            try
            {
                await container.CreateItemAsync(new CommitDocument(commit), new PartitionKey(commit.PartitionKey)).ConfigureAwait(false);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                throw new ConcurrencyException($"Conflict: there is already commit for aggregate {commit.AggregateId} with sequence number {commit.SequenceNumber}", ex);
            }
        }
    }
}
