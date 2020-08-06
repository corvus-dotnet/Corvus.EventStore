// <copyright file="SerializedSnapshot.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Snapshots
{
    using System;

    /// <summary>
    /// Represents an snapshot serialized to a UTF8-encoded text array.
    /// </summary>
    public readonly struct SerializedSnapshot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedSnapshot"/> struct.
        /// </summary>
        /// <param name="aggregateId">The <see cref="AggregateId"/>.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="commitSequenceNumber">The <see cref="CommitSequenceNumber"/>.</param>
        /// <param name="eventSequenceNumber">The <see cref="EventSequenceNumber"/>.</param>
        /// <param name="utf8TextMemento">The <see cref="Memento"/>.</param>
        public SerializedSnapshot(
            Guid aggregateId,
            string partitionKey,
            long commitSequenceNumber,
            long eventSequenceNumber,
            in ReadOnlyMemory<byte> utf8TextMemento)
        {
            this.AggregateId = aggregateId;
            this.PartitionKey = partitionKey;
            this.CommitSequenceNumber = commitSequenceNumber;
            this.EventSequenceNumber = eventSequenceNumber;
            this.Memento = utf8TextMemento;
            this.IsEmpty = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedSnapshot"/> struct.
        /// </summary>
        /// <param name="isEmpty">Indicates whether the snapshot is empty.</param>
        /// <param name="aggregateId">The Id of the aggregate to which this snapshot is applied.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="commitSequenceNumber">The <see cref="CommitSequenceNumber"/>.</param>
        /// <param name="eventSequenceNumber">The <see cref="EventSequenceNumber"/>.</param>
        /// <param name="memento">The <see cref="Memento"/>.</param>
        public SerializedSnapshot(
            bool isEmpty,
            Guid aggregateId,
            string partitionKey,
            long commitSequenceNumber,
            long eventSequenceNumber,
            in ReadOnlyMemory<byte> memento)
        {
            this.IsEmpty = isEmpty;
            this.AggregateId = aggregateId;
            this.PartitionKey = partitionKey;
            this.CommitSequenceNumber = commitSequenceNumber;
            this.EventSequenceNumber = eventSequenceNumber;
            this.Memento = memento;
        }

        /// <summary>
        /// Gets the Id of the aggregate to which this snapshot is applied.
        /// </summary>
        public Guid AggregateId { get; }

        /// <summary>
        /// Gets the partitionkey of the aggregate to which this snapshot is applied.
        /// </summary>
        public string PartitionKey { get; }

        /// <summary>
        /// Gets a value indicating whether this is an empty snapshot.
        /// </summary>
        public bool IsEmpty { get; }

        /// <summary>
        /// Gets the commit sequence number for the snapshot.
        /// </summary>
        /// <remarks>
        /// This is a monotonically incrementing value for the aggregate to which the snapshot belongs.</remarks>
        public long CommitSequenceNumber { get; }

        /// <summary>
        /// Gets the event sequence number for the snapshot.
        /// </summary>
        /// <remarks>
        /// This is a monotonically incrementing value for the aggregate to which the snapshot belongs.</remarks>
        public long EventSequenceNumber { get; }

        /// <summary>
        /// Gets the memento data for the snapshot.
        /// </summary>
        public ReadOnlyMemory<byte> Memento { get; }

        /// <summary>
        /// Gets an empty serialized snapshot.
        /// </summary>
        /// <param name="aggregateId">The aggregate ID.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>An empty snapshot for the given aggregate and partition.</returns>
        public static SerializedSnapshot Empty(Guid aggregateId, string partitionKey) => new SerializedSnapshot(true, aggregateId, partitionKey, -1, -1, ReadOnlyMemory<byte>.Empty);
    }
}
