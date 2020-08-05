// <copyright file="ISnapshotSerializer.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus2.EventStore.Serialization
{
    using System;
    using Corvus2.EventStore.Snapshots;

    /// <summary>
    /// Serializes an Snapshot to and from a <see cref="SerializedSnapshot"/>.
    /// </summary>
    public interface ISnapshotSerializer
    {
        /// <summary>
        /// Deserializes an Snapshot from a <see cref="SerializedSnapshot"/>.
        /// </summary>
        /// <typeparam name="TSnapshot">The type of the snapshot to deserialize.</typeparam>
        /// <typeparam name="TMemento">The type of the memento to deserialize.</typeparam>
        /// <param name="snapshot">The snapshot to deserialize.</param>
        /// <param name="factory">The factory method for creating an instance of the target Snapshot type.</param>
        /// <returns>The deserialized Snapshot.</returns>
        TSnapshot Deserialize<TSnapshot, TMemento>(SerializedSnapshot snapshot, Func<string, long, TMemento, TSnapshot> factory)
            where TSnapshot : ISnapshot;

        /// <summary>
        /// Serializes and Snapshot to a <see cref="SerializedSnapshot"/>.
        /// </summary>
        /// <typeparam name="TSnapshot">The type of the Snapshot to serialize.</typeparam>
        /// <typeparam name="TPayload">The type of the payload to serialize.</typeparam>
        /// <param name="snapshot">The Snapshot to serialize.</param>
        /// <returns>A <see cref="SerializedSnapshot"/> representing the given Snapshot.</returns>
        SerializedSnapshot Serialize<TSnapshot, TPayload>(TSnapshot snapshot)
            where TSnapshot : ISnapshot;
    }
}
