// <copyright file="TableStorageAggregateReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Aggregates
{
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Corvus.EventStore.InMemory.Core;
    using Corvus.EventStore.InMemory.Snapshots;

    /// <summary>
    /// Methods to help manage an in-memory version of <see cref="IAggregateReader"/>.
    /// </summary>
    public static class TableStorageAggregateReader
    {
        /// <summary>
        /// Get an instance of an aggregate reader configured for in-memory use.
        /// </summary>
        /// <param name="eventContainerFactory">The <see cref="IEventCloudTableFactory"/>.</param>
        /// <param name="snapshotContainerFactory">The <see cref="ISnapshotCloudTableFactory"/>.</param>
        /// <returns>An instance of an aggregate reader configured for in-memory use.</returns>
        public static AggregateReader<TableStorageEventReader, TableStorageSnapshotReader> GetInstance(IEventCloudTableFactory eventContainerFactory, ISnapshotCloudTableFactory snapshotContainerFactory)
        {
            return new AggregateReader<TableStorageEventReader, TableStorageSnapshotReader>(new TableStorageEventReader(snapshotContainerFactory), new TableStorageSnapshotReader(snapshotContainerFactory));
        }
    }
}
