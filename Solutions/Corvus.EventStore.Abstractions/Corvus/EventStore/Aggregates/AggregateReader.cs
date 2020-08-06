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
    public readonly struct AggregateReader<TEventReader, TSnapshotReader> : IAggregateReader
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

        /// <inheritdoc/>
        public async ValueTask<TAggregate> ReadAsync<TAggregate>(
            Func<SerializedSnapshot, TAggregate> aggregateFactory,
            Guid aggregateId,
            string partitionKey,
            int batchSize = 100,
            long sequenceNumber = long.MaxValue)
            where TAggregate : IAggregateRoot<TAggregate>
        {
            SerializedSnapshot serializedSnapshot = await this.snapshotReader.ReadAsync(aggregateId, partitionKey, sequenceNumber).ConfigureAwait(false);
            TAggregate aggregate = aggregateFactory(serializedSnapshot);

            if (aggregate.CommitSequenceNumber < sequenceNumber)
            {
                EventReaderResult newEvents = await this.eventReader.ReadCommitsAsync(
                    aggregate.AggregateId,
                    aggregate.PartitionKey,
                    aggregate.CommitSequenceNumber + 1,
                    sequenceNumber,
                    batchSize).ConfigureAwait(false);

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
