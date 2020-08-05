// <copyright file="Snapshot.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Snapshots
{
    /// <summary>
    /// Represents a snapshot of an aggregate at a point in its history.
    /// </summary>
    /// <typeparam name="TMemento">The type of the memento in the snapshot.</typeparam>
    public readonly struct Snapshot<TMemento>
        where TMemento : new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot{TMemento}"/> struct.
        /// </summary>
        /// <param name="aggregateId">The <see cref="AggregateId"/>.</param>
        /// <param name="sequenceNumber">The <see cref="SequenceNumber"/>.</param>
        /// <param name="memento">The <see cref="Memento"/>.</param>
        public Snapshot(string aggregateId, long sequenceNumber, in TMemento memento)
        {
            this.AggregateId = aggregateId;
            this.SequenceNumber = sequenceNumber;
            this.Memento = memento;
        }

        /// <summary>
        /// Gets the Id of the aggregate from which this snapshot was generated.
        /// </summary>
        public string AggregateId { get; }

        /// <summary>
        /// Gets the sequence number for the snapshot.
        /// </summary>
        public long SequenceNumber { get; }

        /// <summary>
        /// Gets the memoized version of the aggregate for the snapshot.
        /// </summary>
        public TMemento Memento { get; }
    }
}