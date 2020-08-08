// <copyright file="TableStorageAggregateWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Aggregates
{
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Corvus.EventStore.Azure.TableStorage.Core;
    using Corvus.EventStore.Azure.TableStorage.Snapshots;

    /// <summary>
    /// Methods to help manage an in-memory version of <see cref="IAggregateWriter"/>.
    /// </summary>
    public static class TableStorageAggregateWriter
    {
        /// <summary>
        /// Get an instance of an aggregate reader configured for in-memory use.
        /// </summary>
        /// <param name="eventCloudTableFactory">The <see cref="IEventCloudTableFactory"/>.</param>
        /// <param name="snapshotCloudTableFactory">The <see cref="ISnapshotCloudTableFactory"/>.</param>
        /// <returns>An instance of an aggregate reader configured for in-memory use.</returns>
        public static AggregateWriter<TableStorageEventWriter, TableStorageSnapshotWriter> GetInstance(IEventCloudTableFactory eventCloudTableFactory, ISnapshotCloudTableFactory snapshotCloudTableFactory)
        {
            return new AggregateWriter<TableStorageEventWriter, TableStorageSnapshotWriter>(new TableStorageEventWriter(eventCloudTableFactory), new TableStorageSnapshotWriter(snapshotCloudTableFactory));
        }
    }
}
