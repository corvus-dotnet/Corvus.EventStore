// <copyright file="InMemoryAggregateWriter.cs" company="Endjin Limited">
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
    /// Methods to help manage an in-memory version of <see cref="IAggregateWriter"/>.
    /// </summary>
    public static class InMemoryAggregateWriter
    {
        /// <summary>
        /// Get an instance of an aggregate reader configured for in-memory use.
        /// </summary>
        /// <param name="eventStore">The <see cref="InMemoryEventStore"/>.</param>
        /// <param name="snapshotStore">The <see cref="InMemorySnapshotStore"/>.</param>
        /// <returns>An instance of an aggregate reader configured for in-memory use.</returns>
        public static AggregateWriter<InMemoryEventWriter, InMemorySnapshotWriter> GetInstance(InMemoryEventStore eventStore, InMemorySnapshotStore snapshotStore)
        {
            return new AggregateWriter<InMemoryEventWriter, InMemorySnapshotWriter>(new InMemoryEventWriter(eventStore), new InMemorySnapshotWriter(snapshotStore));
        }
    }
}
