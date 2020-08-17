// <copyright file="AggregateWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Aggregates
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// Writes aggregate roots, including writing snapshots.
    /// </summary>
    /// <typeparam name="TEventWriter">The type of the underlying event writer.</typeparam>
    /// <typeparam name="TSnapshotWriter">The type of the underlying snapshot writer.</typeparam>
    public readonly struct AggregateWriter<TEventWriter, TSnapshotWriter> : IAggregateWriter
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

        /// <inheritdoc/>
        public async ValueTask<TAggregate> CommitAsync<TAggregate, TSnapshotPolicy>(
            TAggregate aggregate,
            long timestamp = -1,
            TSnapshotPolicy snapshotPolicy = default)
            where TAggregate : IAggregateRoot<TAggregate>
            where TSnapshotPolicy : struct, IAggregateWriterSnapshotPolicy<TAggregate>
        {
            aggregate = await aggregate.CommitAsync(this.eventWriter).ConfigureAwait(false);

            if (snapshotPolicy.ShouldSnapshot(aggregate, timestamp == -1 ? DateTimeOffset.Now.UtcTicks : timestamp))
            {
                try
                {
                    await aggregate.StoreSnapshotAsync(this.snapshotWriter).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // We should log the fact that the snapshot storage has failed, but not
                    // blow up - the fact that the events have been committed means we are
                    // successful
                }
            }

            return aggregate;
        }
    }
}
