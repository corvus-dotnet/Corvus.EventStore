// <copyright file="AggregateWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Aggregates
{
    using System.Threading.Tasks;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// Writes aggregate roots, including writing snapshots.
    /// </summary>
    /// <typeparam name="TEventWriter">The type of the underlying event writer.</typeparam>
    /// <typeparam name="TSnapshotWriter">The type of the underlying snapshot writer.</typeparam>
    public readonly struct AggregateWriter<TEventWriter, TSnapshotWriter>
        where TEventWriter : IEventWriter
        where TSnapshotWriter : ISnapshotWriter
    {
        private readonly TEventWriter eventWriter;
        private readonly TSnapshotWriter snapshotWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateWriter{TEventWriter, TSnapshotWriter}"/> struct.
        /// </summary>
        /// <param name="eventWriter">The underlying event reader.</param>
        /// <param name="snapshotWriter">The underlying snapshot reader.</param>
        public AggregateWriter(TEventWriter eventWriter, TSnapshotWriter snapshotWriter)
        {
            this.eventWriter = eventWriter;
            this.snapshotWriter = snapshotWriter;
        }

        /// <summary>
        /// Writes new events for the supplied aggregate to the event store, using the supplied snapshot policy to
        /// determine whether a new snapshot should also be stored.
        /// </summary>
        /// <typeparam name="TAggregate">The type of aggregate being stored.</typeparam>
        /// <typeparam name="TSnapshotPolicy">The type of snapshot policy.</typeparam>
        /// <param name="aggregate">The aggregate to store.</param>
        /// <param name="timestamp">The nominal current wall clock timestamp as determined by the caller.</param>
        /// <param name="snapshotPolicy">The snapshot policy to use to determine whether a new shapshot should be created.</param>
        /// <returns>The aggregate with all new events committed.</returns>
        public async ValueTask<TAggregate> WriteAsync<TAggregate, TSnapshotPolicy>(
            TAggregate aggregate,
            long timestamp,
            TSnapshotPolicy snapshotPolicy = default)
            where TAggregate : IAggregateRoot<TAggregate>
            where TSnapshotPolicy : struct, IAggregateWriterSnapshotPolicy<TAggregate>
        {
            aggregate = await aggregate.CommitAsync(this.eventWriter).ConfigureAwait(false);

            if (snapshotPolicy.ShouldSnapshot(aggregate, timestamp))
            {
                await aggregate.StoreSnapshotAsync(this.snapshotWriter).ConfigureAwait(false);
            }

            return aggregate;
        }
    }
}
