// <copyright file="SerializedSnapshot.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus2.EventStore.Snapshots
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
        /// <param name="aggregateId">The Id of the aggregate to which this snapshot is applied.</param>
        /// <param name="sequenceNumber">The <see cref="SequenceNumber"/>.</param>
        /// <param name="utf8TextMemento">The <see cref="Utf8TextMemento"/>.</param>
        public SerializedSnapshot(
            string aggregateId,
            long sequenceNumber,
            ReadOnlyMemory<byte> utf8TextMemento)
        {
            this.AggregateId = aggregateId;
            this.SequenceNumber = sequenceNumber;
            this.Utf8TextMemento = utf8TextMemento;
        }

        /// <summary>
        /// Gets the Id of the aggregate to which this snapshot is applied.
        /// </summary>
        public string AggregateId { get; }

        /// <summary>
        /// Gets the sequence number for the snapshot.
        /// </summary>
        /// <remarks>
        /// This is a monotonically incrementing value for the aggregate to which the snapshot belongs.</remarks>
        public long SequenceNumber { get; }

        /// <summary>
        /// Gets the memento data for the snapshot.
        /// </summary>
        public ReadOnlyMemory<byte> Utf8TextMemento { get; }
    }
}
