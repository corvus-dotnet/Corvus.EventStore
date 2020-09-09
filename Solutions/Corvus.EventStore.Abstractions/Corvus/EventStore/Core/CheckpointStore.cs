// <copyright file="CheckpointStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a default checkpoint store which does nothing.
    /// </summary>
    public static class CheckpointStore
    {
        /// <summary>
        /// The default checkpoint store.
        /// </summary>
        public static readonly ICheckpointStore Default = default(NullCheckpointStore);

        private readonly struct NullCheckpointStore : ICheckpointStore
        {
            private static readonly Task<ReadOnlyMemory<byte>?> EmptyResponse = Task.FromResult((ReadOnlyMemory<byte>?)null);

            public Task<ReadOnlyMemory<byte>?> ReadCheckpoint(Guid identity)
            {
                return EmptyResponse;
            }

            public Task ResetCheckpoint(Guid identity)
            {
                return Task.CompletedTask;
            }

            public Task SaveCheckpoint(Guid identity, ReadOnlyMemory<byte> checkpoint)
            {
                return Task.CompletedTask;
            }
        }
    }
}
