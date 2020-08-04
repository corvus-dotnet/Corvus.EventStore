// <copyright file="IAggregateWriterSnapshotPolicy.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Aggregates
{
    /// <summary>
    /// Defines the policy for creating new snapshots when events for an <see cref="IAggregateRoot{TAggregate}"/> are written to
    /// the event store.
    /// </summary>
    /// <typeparam name="TAggregate">The type of aggregate to which this snapshot policy applies.</typeparam>
    public interface IAggregateWriterSnapshotPolicy<TAggregate>
        where TAggregate : IAggregateRoot<TAggregate>
    {
        /// <summary>
        /// Determines whether a new snapshot should be created for the given aggregate and timestamp.
        /// </summary>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="timestamp">The nominal current wall clock timestamp as determined by the caller.</param>
        /// <returns>True if a new snapshot should be created, false otherwise.</returns>
        bool ShouldSnapshot(TAggregate aggregate, long timestamp);
    }
}
