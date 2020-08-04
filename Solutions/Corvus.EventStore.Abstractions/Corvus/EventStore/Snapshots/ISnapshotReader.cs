// <copyright file="ISnapshotReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Snapshots
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for classes capable of reading snapshots.
    /// </summary>
    public interface ISnapshotReader
    {
        /// <summary>
        /// Reads the specified snapshot for the given aggregate.
        /// </summary>
        /// <typeparam name="TSnapshot">The type of the snapshot to provide.</typeparam>
        /// <typeparam name="TMemento">The memento type that this reader can create snapshots for.</typeparam>
        /// <param name="defaultPayloadFactory">A factory method that can be used to create an empty snapshot if none have been stored.</param>
        /// <param name="aggregateId">The Id of the aggregate to read the snapshot for.</param>
        /// <param name="atSequenceId">The sequence Id to read the snapshot at. The snapshot returned will be the one with the highest sequence number less than or equal to this value.</param>
        /// <returns>The most recent snapshot for the aggregate. If no snapshot exists, a new snapshot will be returned containing a payload created via the defaultPayloadFactory.</returns>
        ValueTask<TSnapshot> ReadAsync<TSnapshot, TMemento>(
            Func<TMemento> defaultPayloadFactory,
            string aggregateId,
            long atSequenceId = long.MaxValue)
            where TSnapshot : ISnapshot;
    }
}
