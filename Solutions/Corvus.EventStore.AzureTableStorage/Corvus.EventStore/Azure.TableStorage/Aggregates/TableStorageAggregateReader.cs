// <copyright file="TableStorageAggregateReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Aggregates
{
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Corvus.EventStore.Azure.TableStorage.Core;
    using Corvus.EventStore.Azure.TableStorage.Snapshots;

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
            return new AggregateReader<TableStorageEventReader, TableStorageSnapshotReader>(new TableStorageEventReader(eventContainerFactory), new TableStorageSnapshotReader(snapshotContainerFactory));
        }
    }
}
