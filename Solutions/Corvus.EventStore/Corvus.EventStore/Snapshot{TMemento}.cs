// <copyright file="Snapshot{TMemento}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore
{
    using System;

    /// <summary>
    /// A snapshot read by a <see cref="ISnapshotReader"/>.
    /// </summary>
    /// <typeparam name="TMemento">The type of the memento stored in the snapshot.</typeparam>
    public readonly struct Snapshot<TMemento>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Snapshot{TMemento}"/> struct.
        /// </summary>
        /// <param name="commitSequenceNumber">The <see cref="CommitSequenceNumber"/>.</param>
        /// <param name="eventSequenceNumber">The <see cref="EventSequenceNumber"/>.</param>
        /// <param name="storeMetadata">The <see cref="StoreMetadata"/>.</param>
        /// <param name="memento">The <see cref="Memento"/>.</param>
        public Snapshot(long commitSequenceNumber, long eventSequenceNumber, ReadOnlyMemory<byte> storeMetadata, TMemento memento)
        {
            this.CommitSequenceNumber = commitSequenceNumber;
            this.EventSequenceNumber = eventSequenceNumber;
            this.Memento = memento;
            this.StoreMetadata = storeMetadata;
        }

        /// <summary>
        /// Gets the store index for the record.
        /// </summary>
        public ReadOnlyMemory<byte> StoreMetadata { get; }

        /// <summary>
        /// Gets the commit sequence number for this snapshot.
        /// </summary>
        public long CommitSequenceNumber { get; }

        /// <summary>
        /// Gets the event sequence number for this snapshot.
        /// </summary>
        public long EventSequenceNumber { get; }

        /// <summary>
        /// Gets the memento for this snapshot.
        /// </summary>
        public TMemento Memento { get; }
    }
}
