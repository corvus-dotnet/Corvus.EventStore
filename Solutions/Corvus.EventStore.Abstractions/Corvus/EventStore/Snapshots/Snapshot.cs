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
        /// <param name="sequenceId">The <see cref="SequenceId"/>.</param>
        /// <param name="payload">The <see cref="Payload"/>.</param>
        public Snapshot(string aggregateId, long sequenceId, TMemento payload)
        {
            this.SequenceId = sequenceId;
            this.Payload = payload;
            this.AggregateId = aggregateId;
        }

        /// <summary>
        /// Gets the Id of the aggregate from which this snapshot was generated.
        /// </summary>
        public string AggregateId { get; }

        /// <summary>
        /// Gets the sequence number for the snapshot.
        /// </summary>
        public long SequenceId { get; }

        /// <summary>
        /// Gets the memoized version of the aggregate for the snapshot.
        /// </summary>
        public TMemento Payload { get; }

        /// <summary>
        /// Gets the memoized version of the aggregate for the snapshot.
        /// </summary>
        /// <returns>The memento.</returns>
        public TMemento GetPayload() => this.Payload;
    }
}
