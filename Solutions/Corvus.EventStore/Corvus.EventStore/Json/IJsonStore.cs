// <copyright file="IJsonStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Json
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Implemented by types which can store and retrieve JSON commits.
    /// </summary>
    public interface IJsonStore
    {
        /// <summary>
        /// Write the given stream to the store, using the specified partition key.
        /// </summary>
        /// <param name="stream">The stream to write.</param>
        /// <param name="aggregateId">The aggregate ID that is being written.</param>
        /// <param name="commitSequenceNumber">The commit sequence number that is being written.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <param name="etag">The etag if avilable.</param>
        /// <returns>A <see cref="Task"/> which completes onece the item is written.</returns>
        /// <exception cref="ConcurrencyException">Thrown if a commit was already made for the given aggregate ID and sequence number.</exception>
        Task Write(Stream stream, Guid aggregateId, long commitSequenceNumber, string partitionKey, string etag);
    }
}
