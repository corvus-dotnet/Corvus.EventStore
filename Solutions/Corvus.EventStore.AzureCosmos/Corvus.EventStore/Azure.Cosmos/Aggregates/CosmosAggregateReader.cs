// <copyright file="CosmosAggregateReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.Aggregates
{
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.Azure.Cosmos.ContainerFactories;
    using Corvus.EventStore.Azure.Cosmos.Core;
    using Corvus.EventStore.Azure.Cosmos.Snapshots;

    /// <summary>
    /// Methods to help manage an in-memory version of <see cref="IAggregateReader"/>.
    /// </summary>
    public static class CosmosAggregateReader
    {
        /// <summary>
        /// Get an instance of an aggregate reader configured for in-memory use.
        /// </summary>
        /// <param name="eventContainerFactory">The <see cref="IEventContainerFactory"/>.</param>
        /// <param name="snapshotContainerFactory">The <see cref="ISnapshotContainerFactory"/>.</param>
        /// <returns>An instance of an aggregate reader configured for in-memory use.</returns>
        public static AggregateReader<CosmosEventReader, CosmosSnapshotReader> GetInstance(IEventContainerFactory eventContainerFactory, ISnapshotContainerFactory snapshotContainerFactory)
        {
            return new AggregateReader<CosmosEventReader, CosmosSnapshotReader>(new CosmosEventReader(eventContainerFactory), new CosmosSnapshotReader(snapshotContainerFactory));
        }
    }
}
