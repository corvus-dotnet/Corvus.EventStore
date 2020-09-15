// <copyright file="Snapshot{TMemento}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore
{
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
        /// <param name="memento">The <see cref="Memento"/>.</param>
        public Snapshot(long commitSequenceNumber, long eventSequenceNumber, TMemento memento)
        {
            this.CommitSequenceNumber = commitSequenceNumber;
            this.EventSequenceNumber = eventSequenceNumber;
            this.Memento = memento;
        }

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
