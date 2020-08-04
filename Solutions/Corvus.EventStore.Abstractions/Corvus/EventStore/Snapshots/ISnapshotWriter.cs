// <copyright file="ISnapshotWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Snapshots
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for classes capable of writing snapshots.
    /// </summary>
    public interface ISnapshotWriter
    {
        /// <summary>
        /// Writes the given snapshot to the store.
        /// </summary>
        /// <param name="snapshot">The snapshot to store.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <typeparam name="TSnapshot">The type of the snapshot being written.</typeparam>
        /// <typeparam name="TMemento">The type of the memento being written.</typeparam>
        ValueTask WriteAsync<TSnapshot, TMemento>(in TSnapshot snapshot)
            where TSnapshot : ISnapshot;
    }
}
