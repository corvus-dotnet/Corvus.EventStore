// <copyright file="ISnapshotReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface implemented by types which can read a snapshot for an aggregate root at a particular sequence number.
    /// </summary>
    public interface ISnapshotReader
    {
        /// <summary>
        /// Write a snapshot for the given aggregate Id and memento.
        /// </summary>
        /// <typeparam name="TMemento">The type of the memento to write.</typeparam>
        /// <param name="aggregateId">The ID of the aggregate root for which this is a memento.</param>
        /// <param name="commitSequenceNumber">The sequence number of the commit corresponding to this snapshot.</param>
        /// <returns>A <see cref="Task"/> which provides the latest <see cref="Snapshot{TMemento}"/> whose sequence number is less than or equal to the requested commit sequence number.</returns>
        Task<Snapshot<TMemento>?> Read<TMemento>(Guid aggregateId, long commitSequenceNumber);
    }
}
