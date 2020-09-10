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
        /// <param name="atCommitSequenceNumber">The commit sequence nuber at which to read the snapshot. The snapshot returned will be the one with the highest commit sequence number less than or equal to this value.</param>
        /// <returns>The most recent snapshot for the aggregate. If no snapshot exists, a new snapshot will be returned containing a default payload.</returns>
        ValueTask<SerializedSnapshot> ReadAsync(
            Guid aggregateId,
            string partitionKey,
            long atCommitSequenceNumber = long.MaxValue);
    }
}
