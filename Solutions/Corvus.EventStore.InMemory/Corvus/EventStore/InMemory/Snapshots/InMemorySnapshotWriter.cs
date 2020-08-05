// <copyright file="InMemorySnapshotWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus2.EventStore.InMemory.Snapshots
{
    using System.Threading.Tasks;
    using Corvus2.EventStore.InMemory.Snapshots.Internal;
    using Corvus2.EventStore.Snapshots;

    /// <summary>
    /// In-memory implementation of <see cref="ISnapshotWriter"/>.
    /// </summary>
    public readonly struct InMemorySnapshotWriter : ISnapshotWriter
    {
        private readonly InMemorySnapshotStore store;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemorySnapshotWriter"/> struct.
        /// </summary>
        /// <param name="store">The underlying snapshot store.</param>
        public InMemorySnapshotWriter(InMemorySnapshotStore store)
        {
            this.store = store;
        }

        /// <inheritdoc/>
        public Task WriteAsync(in SerializedSnapshot snapshot)
        {
            return this.store.WriteAsync(snapshot);
        }
    }
}
