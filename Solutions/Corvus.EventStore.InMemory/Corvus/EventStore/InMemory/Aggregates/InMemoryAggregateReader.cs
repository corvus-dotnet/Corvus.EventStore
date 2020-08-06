// <copyright file="InMemoryAggregateReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Aggregates
{
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.InMemory.Core;
    using Corvus.EventStore.InMemory.Core.Internal;
    using Corvus.EventStore.InMemory.Snapshots;
    using Corvus.EventStore.InMemory.Snapshots.Internal;

    /// <summary>
    /// Methods to help manage an in-memory version of <see cref="IAggregateReader"/>.
    /// </summary>
    public static class InMemoryAggregateReader
    {
        /// <summary>
        /// Get an instance of an aggregate reader configured for in-memory use.
        /// </summary>
        /// <param name="eventStore">The <see cref="InMemoryEventStore"/>.</param>
        /// <param name="snapshotStore">The <see cref="InMemorySnapshotStore"/>.</param>
        /// <returns>An instance of an aggregate reader configured for in-memory use.</returns>
        public static AggregateReader<InMemoryEventReader, InMemorySnapshotReader> GetInstance(InMemoryEventStore eventStore, InMemorySnapshotStore snapshotStore)
        {
            return new AggregateReader<InMemoryEventReader, InMemorySnapshotReader>(new InMemoryEventReader(eventStore), new InMemorySnapshotReader(snapshotStore));
        }
    }
}
