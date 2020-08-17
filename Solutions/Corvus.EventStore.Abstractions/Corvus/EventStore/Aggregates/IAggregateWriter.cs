// <copyright file="IAggregateWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Aggregates
{
    using System.Threading.Tasks;

    /// <summary>
    /// An interface for aggregate writers.
    /// </summary>
    public interface IAggregateWriter
    {
        /// <summary>
        /// Commits uncommitted events for the supplied aggregate to the event store, using the supplied snapshot policy to
        /// determine whether a new snapshot should also be stored.
        /// </summary>
        /// <typeparam name="TAggregate">The type of aggregate being stored.</typeparam>
        /// <typeparam name="TSnapshotPolicy">The type of snapshot policy.</typeparam>
        /// <param name="aggregate">The aggregate to store.</param>
        /// <param name="timestamp">The (optional) nominal current wall clock timestamp as determined by the caller.</param>
        /// <param name="snapshotPolicy">The (optional) snapshot policy to use to determine whether a new shapshot should be created.</param>
        /// <returns>The aggregate with all new events committed.</returns>
        ValueTask<TAggregate> CommitAsync<TAggregate, TSnapshotPolicy>(TAggregate aggregate, long timestamp = -1, TSnapshotPolicy snapshotPolicy = default)
            where TAggregate : IAggregateRoot<TAggregate>
            where TSnapshotPolicy : struct, IAggregateWriterSnapshotPolicy<TAggregate>;
    }
}