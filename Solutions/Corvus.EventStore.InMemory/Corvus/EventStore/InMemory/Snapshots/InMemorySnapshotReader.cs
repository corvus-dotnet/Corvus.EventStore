// <copyright file="InMemorySnapshotReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Snapshots
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.InMemory.Snapshots.Internal;
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// In-memory implementation of <see cref="ISnapshotReader"/>.
    /// </summary>
    public readonly struct InMemorySnapshotReader : ISnapshotReader
    {
        private readonly InMemorySnapshotStore store;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemorySnapshotReader"/> struct.
        /// </summary>
        /// <param name="store">The underlying snapshot store.</param>
        public InMemorySnapshotReader(InMemorySnapshotStore store)
        {
            this.store = store;
        }

        /// <inheritdoc/>
        public async ValueTask<TSnapshot> ReadAsync<TSnapshot, TMemento>(Func<TMemento> defaultPayloadFactory, string aggregateId, long atSequenceId = long.MaxValue)
            where TSnapshot : ISnapshot
        {
            InMemorySnapshot inMemorySnapshot = await this.store.ReadAsync<TMemento>(defaultPayloadFactory, aggregateId, atSequenceId).ConfigureAwait(false);
            return inMemorySnapshot is TSnapshot snapshot ? snapshot : throw new InvalidOperationException($"The requested snapshot type {typeof(TSnapshot)} is not compatible with {inMemorySnapshot.GetType()}");
        }
    }
}
