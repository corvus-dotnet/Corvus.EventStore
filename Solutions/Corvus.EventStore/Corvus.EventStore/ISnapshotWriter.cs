// <copyright file="ISnapshotWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface implemented by types which can write a snapshot for an aggregate root at a particular sequence number.
    /// </summary>
    public interface ISnapshotWriter
    {
        /// <summary>
        /// Write a snapshot for the given aggregate Id and memento.
        /// </summary>
        /// <typeparam name="TMemento">The type of the memento to write.</typeparam>
        /// <param name="aggregateId">The ID of the aggregate root for which this is a memento.</param>
        /// <param name="commitSequenceNumber">The sequence number of the commit corresponding to this snapshot.</param>
        /// <param name="eventSequenceNumber">The sequence number of the event corresponding to this snapshot.</param>
        /// <param name="memento">The memento to write as a snapshot for this aggregate root.</param>
        /// <returns>A <see cref="Task"/> which completes once the snapshot is written.</returns>
        Task Write<TMemento>(Guid aggregateId, long commitSequenceNumber, long eventSequenceNumber, in TMemento memento);
    }
}
