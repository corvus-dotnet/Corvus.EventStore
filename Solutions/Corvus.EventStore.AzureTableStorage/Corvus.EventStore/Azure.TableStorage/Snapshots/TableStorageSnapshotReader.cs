// <copyright file="TableStorageSnapshotReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Snapshots
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.InMemory.Aggregates;
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// In-memory implementation of <see cref="ISnapshotReader"/>.
    /// </summary>
    public readonly struct TableStorageSnapshotReader : ISnapshotReader
    {
        private readonly ISnapshotCloudTableFactory cloudTableFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageSnapshotReader"/> struct.
        /// </summary>
        /// <param name="cloudTableFactory">The factory for the cloud table for this store.</param>
        public TableStorageSnapshotReader(ISnapshotCloudTableFactory cloudTableFactory)
        {
            this.cloudTableFactory = cloudTableFactory;
        }

        /// <inheritdoc/>
        public ValueTask<SerializedSnapshot> ReadAsync(Guid aggregateId, string partitionKey, long atSequenceId = long.MaxValue)
        {
            throw new NotImplementedException();
        }
    }
}
