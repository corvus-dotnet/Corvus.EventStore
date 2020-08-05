// <copyright file="ISnapshotSerializer.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Serialization
{
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// Serializes an Snapshot to and from a <see cref="SerializedSnapshot"/>.
    /// </summary>
    public interface ISnapshotSerializer
    {
        /// <summary>
        /// Deserializes an Snapshot from a <see cref="SerializedSnapshot"/>.
        /// </summary>
        /// <typeparam name="TMemento">The type of the memento to deserialize.</typeparam>
        /// <param name="snapshot">The snapshot to deserialize.</param>
        /// <returns>The deserialized Snapshot.</returns>
        Snapshot<TMemento> Deserialize<TMemento>(SerializedSnapshot snapshot)
            where TMemento : new();

        /// <summary>
        /// Serializes and Snapshot to a <see cref="SerializedSnapshot"/>.
        /// </summary>
        /// <typeparam name="TMemento">The type of the memento to deserialize.</typeparam>
        /// <param name="snapshot">The Snapshot to serialize.</param>
        /// <returns>A <see cref="SerializedSnapshot"/> representing the given Snapshot.</returns>
        SerializedSnapshot Serialize<TMemento>(Snapshot<TMemento> snapshot)
            where TMemento : new();
    }
}
