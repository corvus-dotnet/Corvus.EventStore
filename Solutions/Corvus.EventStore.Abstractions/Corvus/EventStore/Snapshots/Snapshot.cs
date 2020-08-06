// <copyright file="Snapshot.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Snapshots
{
    using System;

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
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="commitSequenceNumber">The <see cref="CommitSequenceNumber"/>.</param>
        /// <param name="eventSequenceNumber">The <see cref="EventSequenceNumber"/>.</param>
        /// <param name="memento">The <see cref="Memento"/>.</param>
        public Snapshot(Guid aggregateId, string partitionKey, long commitSequenceNumber, long eventSequenceNumber, in TMemento memento)
        {
            this.AggregateId = aggregateId;
            this.PartitionKey = partitionKey;
            this.CommitSequenceNumber = commitSequenceNumber;
            this.EventSequenceNumber = eventSequenceNumber;
            this.Memento = memento;
        }

        /// <summary>
        /// Gets the Id of the aggregate from which this snapshot was generated.
        /// </summary>
        public Guid AggregateId { get; }

        /// <summary>
        /// Gets the sequence number of the commit which this snapshot represents.
        /// </summary>
        public long CommitSequenceNumber { get; }

        /// <summary>
        /// Gets the event sequence number for the snapshot.
        /// </summary>
        /// <remarks>
        /// This is a monotonically incrementing value for the aggregate to which the snapshot belongs.</remarks>
        public long EventSequenceNumber { get; }

        /// <summary>
        /// Gets the partitionkey of the aggregate to which this snapshot is applied.
        /// </summary>
        public string PartitionKey { get; }

        /// <summary>
        /// Gets the memoized version of the aggregate for the snapshot at he given <see cref="CommitSequenceNumber"/>.
        /// </summary>
        public TMemento Memento { get; }
    }
}