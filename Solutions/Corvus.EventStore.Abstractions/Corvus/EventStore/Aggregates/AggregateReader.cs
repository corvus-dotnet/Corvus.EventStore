// <copyright file="AggregateReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Aggregates
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// Reads aggregate roots using a combination of snapshot and events.
    /// </summary>
    /// <typeparam name="TEventReader">The type of the underlying event reader.</typeparam>
    /// <typeparam name="TSnapshotReader">The type of the underlying snapshot reader.</typeparam>
    public readonly struct AggregateReader<TEventReader, TSnapshotReader>
        where TEventReader : IEventReader
        where TSnapshotReader : ISnapshotReader
    {
        private readonly TEventReader eventReader;
        private readonly TSnapshotReader snapshotReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateReader{TEventReader, TSnapshotReader}"/> struct.
        /// </summary>
        /// <param name="eventReader">The underlying event reader.</param>
        /// <param name="snapshotReader">The underlying snapshot reader.</param>
        public AggregateReader(TEventReader eventReader, TSnapshotReader snapshotReader)
        {
            this.eventReader = eventReader;
            this.snapshotReader = snapshotReader;
        }

        /// <summary>
        /// Reads the aggregate with the specified Id.
        /// </summary>
        /// <typeparam name="TAggregate">The type of aggregate being read.</typeparam>
        /// <param name="aggregateFactory">A factory method for creating an aggregate from a snapshot.</param>
        /// <param name="aggregateId">The Id of the aggregate.</param>
        /// <param name="sequenceNumber">The maximum sequence number to retrieve.</param>
        /// <returns>The specified aggregate.</returns>
        /// <remarks>
        /// If a sequence number is supplied, the aggregate will be recreated using events with sequence numbers lower
        /// than or equal to the specified value.
        /// </remarks>
        public async ValueTask<TAggregate> ReadAsync<TAggregate>(
            Func<SerializedSnapshot, TAggregate> aggregateFactory,
            string aggregateId,
            long sequenceNumber = long.MaxValue)
            where TAggregate : IAggregateRoot<TAggregate>
        {
            SerializedSnapshot serializedSnapshot = await this.snapshotReader.ReadAsync(aggregateId, sequenceNumber).ConfigureAwait(false);
            TAggregate aggregate = aggregateFactory(serializedSnapshot);

            if (aggregate.CommitSequenceNumber < sequenceNumber)
            {
                EventReaderResult newEvents = await this.eventReader.ReadCommitsAsync(
                    aggregate.AggregateId,
                    aggregate.CommitSequenceNumber + 1,
                    sequenceNumber,
                    int.MaxValue).ConfigureAwait(false);

                while (true)
                {
                    aggregate = aggregate.ApplyCommits(newEvents.Commits);

                    if (newEvents.ContinuationToken is null)
                    {
                        break;
                    }

                    newEvents = await this.eventReader.ReadCommitsAsync(newEvents.ContinuationToken.Value.Span).ConfigureAwait(false);
                }
            }

            return aggregate;
        }
    }
}
