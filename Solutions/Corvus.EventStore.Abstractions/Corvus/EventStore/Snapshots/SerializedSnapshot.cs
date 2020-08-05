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
        /// Gets an empty serialized snapshot.
        /// </summary>
        public static readonly SerializedSnapshot Empty = new SerializedSnapshot(true, string.Empty, -1, ReadOnlyMemory<byte>.Empty);

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedSnapshot"/> struct.
        /// </summary>
        /// <param name="aggregateId">The Id of the aggregate to which this snapshot is applied.</param>
        /// <param name="sequenceNumber">The <see cref="SequenceNumber"/>.</param>
        /// <param name="utf8TextMemento">The <see cref="Memento"/>.</param>
        public SerializedSnapshot(
            string aggregateId,
            long sequenceNumber,
            ReadOnlyMemory<byte> utf8TextMemento)
        {
            this.AggregateId = aggregateId;
            this.SequenceNumber = sequenceNumber;
            this.Memento = utf8TextMemento;
            this.IsEmpty = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedSnapshot"/> struct.
        /// </summary>
        /// <param name="isEmpty">Indicates whether the snapshot is empty.</param>
        /// <param name="aggregateId">The Id of the aggregate to which this snapshot is applied.</param>
        /// <param name="sequenceNumber">The <see cref="SequenceNumber"/>.</param>
        /// <param name="utf8TextMemento">The <see cref="Memento"/>.</param>
        public SerializedSnapshot(
            bool isEmpty,
            string aggregateId,
            long sequenceNumber,
            ReadOnlyMemory<byte> utf8TextMemento)
        {
            this.IsEmpty = isEmpty;
            this.AggregateId = aggregateId;
            this.SequenceNumber = sequenceNumber;
            this.Memento = utf8TextMemento;
            this.IsEmpty = false;
        }

        /// <summary>
        /// Gets the Id of the aggregate to which this snapshot is applied.
        /// </summary>
        public string AggregateId { get; }

        /// <summary>
        /// Gets a value indicating whether this is an empty snapshot.
        /// </summary>
        public bool IsEmpty { get; }

        /// <summary>
        /// Gets the sequence number for the snapshot.
        /// </summary>
        /// <remarks>
        /// This is a monotonically incrementing value for the aggregate to which the snapshot belongs.</remarks>
        public long SequenceNumber { get; }

        /// <summary>
        /// Gets the memento data for the snapshot.
        /// </summary>
        public ReadOnlyMemory<byte> Memento { get; }
    }
}
