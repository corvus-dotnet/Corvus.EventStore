// <copyright file="InMemoryEventFeed.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Core
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.InMemory.Core.Internal;

    /// <summary>
    /// An in-memory event feed for a store.
    /// </summary>
    public class InMemoryEventFeed : IEventFeed
    {
        private readonly InMemoryEventStore store;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventFeed"/> class.
        /// </summary>
        /// <param name="store">The store over which to implement the feed.</param>
        public InMemoryEventFeed(InMemoryEventStore store)
        {
            this.store = store;
        }

        /// <inheritdoc/>
        public ValueTask<EventFeedResult> Get(EventFeedFilter filter, int maxItems)
        {
            return this.store.GetFeed(filter, maxItems);
        }

        /// <inheritdoc/>
        public ValueTask<EventFeedResult> Get(ReadOnlyMemory<byte> fromCheckpoint)
        {
            return this.store.GetFeed(fromCheckpoint);
        }
    }
}
