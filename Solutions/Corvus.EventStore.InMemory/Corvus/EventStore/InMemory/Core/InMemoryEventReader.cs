// <copyright file="InMemoryEventReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus2.EventStore.InMemory.Core
{
    using System;
    using System.Threading.Tasks;
    using Corvus2.EventStore.Core;
    using Corvus2.EventStore.InMemory.Core.Internal;

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
        public ValueTask<EventReaderResult> ReadAsync(ReadOnlySpan<byte> utf8TextContinuationToken)
        {
            return this.store.ReadAsync(utf8TextContinuationToken);
        }

        /// <inheritdoc/>
        public ValueTask<EventReaderResult> ReadAsync(string aggregateId, long fromSequenceNumber, long toSequenceNumber, int maxItems)
        {
            return this.store.ReadAsync(aggregateId, fromSequenceNumber, toSequenceNumber, maxItems);
        }
    }
}
