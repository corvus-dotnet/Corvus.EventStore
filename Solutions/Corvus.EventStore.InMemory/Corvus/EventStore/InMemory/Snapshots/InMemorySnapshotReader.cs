// <copyright file="InMemorySnapshotReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Snapshots
{
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
        public ValueTask<SerializedSnapshot> ReadAsync(string aggregateId, long atSequenceId = long.MaxValue)
        {
            return this.store.ReadAsync(aggregateId, atSequenceId);
        }
    }
}
