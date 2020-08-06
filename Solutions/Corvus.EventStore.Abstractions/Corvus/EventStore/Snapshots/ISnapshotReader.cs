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
        /// <param name="aggregateId">The Id of the aggregate to read the snapshot for.</param>
        /// <param name="partitionKey">The partition key of the aggregate.</param>
        /// <param name="atSequenceId">The sequence Id to read the snapshot at. The snapshot returned will be the one with the highest sequence number less than or equal to this value.</param>
        /// <returns>The most recent snapshot for the aggregate. If no snapshot exists, a new snapshot will be returned containing a payload created via the defaultPayloadFactory.</returns>
        ValueTask<SerializedSnapshot> ReadAsync(
            Guid aggregateId,
            string partitionKey,
            long atSequenceId = long.MaxValue);
    }
}
