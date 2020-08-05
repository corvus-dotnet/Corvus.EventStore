// <copyright file="InMemoryEventWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Core
{
    using System;
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
        public ValueTask WriteAsync<TEvent1, TPayload1, TEvent2, TPayload2>(in TEvent1 @event1, in TEvent2 @event2)
            where TEvent1 : IEvent
            where TEvent2 : IEvent
        {
            return this.store.WriteAsync(InMemoryEvent.CreateFrom<TEvent1, TPayload1>(@event1), InMemoryEvent.CreateFrom<TEvent2, TPayload2>(@event2));
        }

        /// <inheritdoc/>
        public ValueTask WriteAsync<TEvent1, TPayload1, TEvent2, TPayload2, TEvent3, TPayload3>(in TEvent1 @event1, in TEvent2 @event2, in TEvent3 @event3)
            where TEvent1 : IEvent
            where TEvent2 : IEvent
            where TEvent3 : IEvent
        {
            return this.store.WriteAsync(InMemoryEvent.CreateFrom<TEvent1, TPayload1>(@event1), InMemoryEvent.CreateFrom<TEvent2, TPayload2>(@event2), InMemoryEvent.CreateFrom<TEvent3, TPayload3>(@event3));
        }

        /// <inheritdoc/>
        public ValueTask WriteAsync<TEvent1, TPayload1, TEvent2, TPayload2, TEvent3, TPayload3, TEvent4, TPayload4>(in TEvent1 @event1, in TEvent2 @event2, in TEvent3 @event3, in TEvent4 @event4)
            where TEvent1 : IEvent
            where TEvent2 : IEvent
            where TEvent3 : IEvent
            where TEvent4 : IEvent
        {
            return this.store.WriteAsync(InMemoryEvent.CreateFrom<TEvent1, TPayload1>(@event1), InMemoryEvent.CreateFrom<TEvent2, TPayload2>(@event2), InMemoryEvent.CreateFrom<TEvent3, TPayload3>(@event3), InMemoryEvent.CreateFrom<TEvent4, TPayload4>(@event4));
        }

        /// <inheritdoc/>
        public ValueTask WriteAsync<TEvent1, TPayload1, TEvent2, TPayload2, TEvent3, TPayload3, TEvent4, TPayload4, TEvent5, TPayload5>(in TEvent1 @event1, in TEvent2 @event2, in TEvent3 @event3, in TEvent4 @event4, in TEvent5 @event5)
            where TEvent1 : IEvent
            where TEvent2 : IEvent
            where TEvent3 : IEvent
            where TEvent4 : IEvent
            where TEvent5 : IEvent
        {
            return this.store.WriteAsync(InMemoryEvent.CreateFrom<TEvent1, TPayload1>(@event1), InMemoryEvent.CreateFrom<TEvent2, TPayload2>(@event2), InMemoryEvent.CreateFrom<TEvent3, TPayload3>(@event3), InMemoryEvent.CreateFrom<TEvent4, TPayload4>(@event4), InMemoryEvent.CreateFrom<TEvent5, TPayload5>(@event5));
        }

        /// <inheritdoc/>
        public ValueTask WriteBatchAsync(params Action<IEventBatchWriter>[] eventWrites)
        {
            return this.store.WriteBatchAsync(eventWrites);
        }
    }
}
