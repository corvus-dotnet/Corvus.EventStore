// <copyright file="Commit.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System.Collections.Immutable;

    /// <summary>
    /// A commit in the event store.
    /// </summary>
    public readonly struct Commit
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Commit"/> struct.
        /// </summary>
        /// <param name="aggregateId">The <see cref="AggregateId"/>.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="sequenceNumber">The <see cref="SequenceNumber"/>.</param>
        /// <param name="timestamp">The <see cref="Timestamp"/>.</param>
        /// <param name="events">The <see cref="Events"/>.</param>
        public Commit(string aggregateId, string partitionKey, long sequenceNumber, long timestamp, ImmutableArray<SerializedEvent> events)
        {
            this.AggregateId = aggregateId;
            this.PartitionKey = partitionKey;
            this.SequenceNumber = sequenceNumber;
            this.Timestamp = timestamp;
            this.Events = events;
        }

        /// <summary>
        /// Gets the ID of the aggregate for this commit.
        /// </summary>
        public string AggregateId { get; }

        /// <summary>
        /// Gets the partition key for this commit.
        /// </summary>
        public string PartitionKey { get; }

        /// <summary>
        /// Gets the monotonically increasing sequence number of the commit.
        /// </summary>
        public long SequenceNumber { get; }

        /// <summary>
        /// Gets the timestamp of the commit.
        /// </summary>
        /// <remarks>There is no guarantee that the timestamp of this commit will be after that of
        /// the previous commit, as clock drift between instances may come in to play.
        /// Sequence numbers determine order, but the timestamp may be useful for diagnostics or reporting.</remarks>
        public long Timestamp { get; }

        /// <summary>
        /// Gets the ordered list of events in the commit.
        /// </summary>
        public ImmutableArray<SerializedEvent> Events { get; }
    }
}
