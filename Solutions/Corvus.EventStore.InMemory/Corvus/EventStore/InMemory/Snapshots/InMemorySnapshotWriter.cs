// <copyright file="InMemorySnapshotWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Snapshots
{
    using System.Threading.Tasks;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.InMemory.Snapshots.Internal;
    using Corvus.EventStore.Snapshots;

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
        public async Task WriteAsync(SerializedSnapshot snapshot)
        {
            try
            {
                await this.store.WriteAsync(snapshot).ConfigureAwait(false);
            }
            catch (InMemorySnapshotStoreConcurrencyException ex)
            {
                throw new ConcurrencyException("Unable to write the snapshot.", ex);
            }
        }
    }
}
