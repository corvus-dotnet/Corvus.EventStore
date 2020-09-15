// <copyright file="NullSnapshotReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// A <see cref="ISnapshotReader"/> that will always return a null snapshot.
    /// </summary>
    public readonly struct NullSnapshotReader : ISnapshotReader
    {
        /// <summary>
        /// Gets an instance of a <see cref="NullSnapshotReader"/>.
        /// </summary>
        public static NullSnapshotReader Instance = default;

        /// <inheritdoc/>
        public Task<Snapshot<TMemento>?> Read<TMemento>(Guid aggregateId, long commitSequenceNumber)
        {
            return Task.FromResult<Snapshot<TMemento>?>(null);
        }
    }
}
