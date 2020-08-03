// <copyright file="Snapshot.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Snapshots
{
    /// <summary>
    /// Represents a snapshot of an aggregate at a point in its history.
    /// </summary>
    /// <typeparam name="TMemento">The type of the memento produced by the source aggregate.</typeparam>
    public readonly struct Snapshot<TMemento> : ISnapshot<TMemento>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot{TMemento}"/> struct.
        /// </summary>
        /// <param name="aggregateId">The Id of the aggregate that created the snapshot.</param>
        /// <param name="sequenceNumber">The <see cref="SequenceNumber"/>.</param>
        /// <param name="payload">The <see cref="Payload"/>.</param>
        public Snapshot(string aggregateId, long sequenceNumber, TMemento payload)
        {
            this.SequenceNumber = sequenceNumber;
            this.Payload = payload;
            this.AggregateId = aggregateId;
        }

        /// <inheritdoc/>
        public string AggregateId { get; }

        /// <inheritdoc/>
        public long SequenceNumber { get; }

        /// <summary>
        /// Gets the memoized version of the aggregate for the snapshot.
        /// </summary>
        public TMemento Payload { get; }

        /// <inheritdoc/>
        public TMemento GetPayload() => this.Payload;
    }
}
