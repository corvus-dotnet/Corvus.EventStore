// <copyright file="CosmosAggregateWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.Aggregates
{
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.Azure.Cosmos.ContainerFactories;
    using Corvus.EventStore.Azure.Cosmos.Core;
    using Corvus.EventStore.Azure.Cosmos.Snapshots;

    /// <summary>
    /// Methods to help manage an in-memory version of <see cref="IAggregateWriter"/>.
    /// </summary>
    public static class CosmosAggregateWriter
    {
        /// <summary>
        /// Get an instance of an aggregate reader configured for Cosmos DB container use..
        /// </summary>
        /// <param name="eventContainerFactory">The <see cref="IEventContainerFactory"/>.</param>
        /// <param name="snapshotContainerFactory">The <see cref="ISnapshotContainerFactory"/>.</param>
        /// <returns>An instance of an aggregate reader configured for Cosmos DB container use.</returns>
        public static AggregateWriter<CosmosEventWriter, CosmosSnapshotWriter> GetInstance(IEventContainerFactory eventContainerFactory, ISnapshotContainerFactory snapshotContainerFactory)
        {
            return new AggregateWriter<CosmosEventWriter, CosmosSnapshotWriter>(new CosmosEventWriter(eventContainerFactory), new CosmosSnapshotWriter(snapshotContainerFactory));
        }
    }
}
