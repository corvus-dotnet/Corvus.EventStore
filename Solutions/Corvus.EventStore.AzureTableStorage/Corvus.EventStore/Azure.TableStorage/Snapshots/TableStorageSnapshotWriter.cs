// <copyright file="TableStorageSnapshotWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Snapshots
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.InMemory.Aggregates;
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// In-memory implementation of <see cref="ISnapshotWriter"/>.
    /// </summary>
    public readonly struct TableStorageSnapshotWriter : ISnapshotWriter
    {
        private readonly ISnapshotCloudTableFactory cloudTableFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageSnapshotWriter"/> struct.
        /// </summary>
        /// <param name="cloudTableFactory">The factory for the container for the snapshots.</param>
        public TableStorageSnapshotWriter(ISnapshotCloudTableFactory cloudTableFactory)
        {
            this.cloudTableFactory = cloudTableFactory;
        }

        /// <inheritdoc/>
        public Task WriteAsync(SerializedSnapshot snapshot)
        {
            throw new NotImplementedException();
        }
    }
}
