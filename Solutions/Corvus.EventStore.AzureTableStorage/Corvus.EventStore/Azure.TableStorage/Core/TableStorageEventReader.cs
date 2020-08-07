// <copyright file="TableStorageEventReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Core
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.InMemory.Aggregates;

    /// <summary>
    /// In-memory implementation of <see cref="IEventReader"/>.
    /// </summary>
    public readonly struct TableStorageEventReader : IEventReader
    {
        private readonly ISnapshotCloudTableFactory cloudTableFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageEventReader"/> struct.
        /// </summary>
        /// <param name="cloudTableFactory">The underlying store.</param>
        public TableStorageEventReader(ISnapshotCloudTableFactory cloudTableFactory)
        {
            this.cloudTableFactory = cloudTableFactory;
        }

        /// <inheritdoc/>
        public ValueTask<EventReaderResult> ReadCommitsAsync(ReadOnlySpan<byte> continuationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public ValueTask<EventReaderResult> ReadCommitsAsync(Guid aggregateId, string partitionKey, long fromSequenceNumber, long toSequenceNumber, int maxItems)
        {
            throw new NotImplementedException();
        }
    }
}
