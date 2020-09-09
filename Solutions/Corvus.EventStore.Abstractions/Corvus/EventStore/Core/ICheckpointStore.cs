// <copyright file="ICheckpointStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// A store which persits a checkpoint for an EventFeed.
    /// </summary>
    public interface ICheckpointStore
    {
        /// <summary>
        /// Save the checkpoint for the consumer with a given identity.
        /// </summary>
        /// <param name="identity">The unique identity of the consumer of the feed.</param>
        /// <param name="checkpoint">The checkpoint to store.</param>
        /// <returns>A <see cref="Task"/> which completes when the checkpoint is saved.</returns>
        Task SaveCheckpoint(Guid identity, ReadOnlyMemory<byte> checkpoint);

        /// <summary>
        /// Reads the checkpoint for the consumer with a given identity.
        /// </summary>
        /// <param name="identity">The identity of the consumer of the feed.</param>
        /// <returns>The checkpoint for that feed, or <c>null</c> if there was no stored checkpoint.</returns>
        Task<ReadOnlyMemory<byte>?> ReadCheckpoint(Guid identity);

        /// <summary>
        /// Resets the checkpoint for the consumer with a given identity.
        /// </summary>
        /// <param name="identity">The identity of the consumer of the feed.</param>
        /// <returns>A <see cref="Task"/> which completes when the checkpoint is reset.</returns>
        /// <remarks>
        /// This causes subsequent reads from the store for this identity to return <c>null</c> until a new
        /// checkpoint is saved.
        /// </remarks>
        Task ResetCheckpoint(Guid identity);
    }
}
