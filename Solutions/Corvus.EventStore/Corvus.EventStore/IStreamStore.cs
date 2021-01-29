// <copyright file="IStreamStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Implemented by types which can store and retrieve JSON commits.
    /// </summary>
    public interface IStreamStore
    {
        /// <summary>
        /// Write the given stream to the store, using the specified partition key.
        /// </summary>
        /// <param name="stream">The stream to write.</param>
        /// <param name="aggregateId">The aggregate ID that is being written.</param>
        /// <param name="commitSequenceNumber">The commit sequence number that is being written.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="storeMetadata">Metadata about the commit from the store.</param>
        /// <returns>A <see cref="Task"/> which provides the store metadata returned from the result.</returns>
        /// <exception cref="ConcurrencyException">Thrown if a commit was already made for the given aggregate ID and sequence number.</exception>
        Task<ReadOnlyMemory<byte>> Write(Stream stream, Guid aggregateId, long commitSequenceNumber, string partitionKey, ReadOnlyMemory<byte> storeMetadata);
    }
}
