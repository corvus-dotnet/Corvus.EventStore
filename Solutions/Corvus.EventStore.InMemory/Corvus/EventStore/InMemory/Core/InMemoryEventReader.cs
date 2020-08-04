// <copyright file="InMemoryEventReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Core
{
    using System.Threading.Tasks;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.InMemory.Core.Internal;

    /// <summary>
    /// In-memory implementation of <see cref="IEventReader"/>.
    /// </summary>
    public readonly struct InMemoryEventReader : IEventReader
    {
        private readonly InMemoryEventStore store;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventReader"/> struct.
        /// </summary>
        /// <param name="store">The underlying store.</param>
        public InMemoryEventReader(InMemoryEventStore store)
        {
            this.store = store;
        }

        /// <inheritdoc/>
        public ValueTask<IEventReaderResult> ReadAsync(string aggregateId, long fromSequenceNumber, long toSequenceNumber, int maxItems)
        {
            return this.store.ReadAsync(aggregateId, fromSequenceNumber, toSequenceNumber, maxItems);
        }

        /// <inheritdoc/>
        public ValueTask<IEventReaderResult> ReadAsync(string continuationToken)
        {
            return this.store.ReadAsync(continuationToken);
        }
    }
}
