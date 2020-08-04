// <copyright file="InMemoryEventWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.InMemory.Core.Internal;

    /// <summary>
    /// In-memory implementation of <see cref="IEventWriter"/>.
    /// </summary>
    public readonly struct InMemoryEventWriter : IEventWriter
    {
        private readonly InMemoryEventStore store;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventWriter"/> struct.
        /// </summary>
        /// <param name="store">The underlying store.</param>
        public InMemoryEventWriter(InMemoryEventStore store)
        {
            this.store = store;
        }

        /// <inheritdoc/>
        public ValueTask WriteAsync<TEvent, TPayload>(in TEvent @event)
            where TEvent : IEvent
        {
            return this.store.WriteAsync(InMemoryEvent.CreateFrom<TEvent, TPayload>(@event));
        }

        /// <inheritdoc/>
        public ValueTask WriteBatchAsync(in IEnumerable<Action<IEventBatchWriter>> eventWrites)
        {
            return this.store.WriteBatchAsync(eventWrites);
        }
    }
}
