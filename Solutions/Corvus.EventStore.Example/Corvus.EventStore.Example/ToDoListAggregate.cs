// <copyright file="ToDoListAggregate.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Example.Internal;
    using Corvus.EventStore.Serialization;
    using Corvus.EventStore.Serialization.Json;
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// The aggregate root for a ToDo list containing ToDo items.
    /// </summary>
    internal readonly struct ToDoListAggregate : IAggregateRoot<ToDoListAggregate>
    {
        private readonly ImmutableArray<SerializedEvent> uncommittedEvents;

        // We are using this memento to maintain internal state as we go along. You don't have to. You could create it on demand. Or never!
        // You may choose to always rehydrate your state from elsewhere when needed.
        private readonly ToDoListMemento memento;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoListAggregate"/> struct.
        /// </summary>
        /// <param name="aggregateId">The <see cref="AggregateId"/>.</param>
        /// <param name="sequenceNumber">The <see cref="SequenceNumber"/>.</param>
        /// <param name="uncommittedEvents">The <see cref="uncommittedEvents"/>.</param>
        /// <param name="memento">The <see cref="memento"/>.</param>
        private ToDoListAggregate(string aggregateId, long sequenceNumber, in ImmutableArray<SerializedEvent> uncommittedEvents, in ToDoListMemento memento)
        {
            this.AggregateId = aggregateId;
            this.SequenceNumber = sequenceNumber;
            this.uncommittedEvents = uncommittedEvents;
            this.memento = memento;
        }

        /// <summary>
        /// Gets or sets the event serializer to use for the aggregate.
        /// </summary>
        public static IEventSerializer EventSerializer { get; set; } = default(Utf8JsonEventSerializer);

        /// <summary>
        /// Gets or sets the snapshot serializer to use for the aggregate.
        /// </summary>
        public static ISnapshotSerializer SnapshotSerializer { get; set; } = default(Utf8JsonSnapshotSerializer);

        /// <inheritdoc/>
        public string AggregateId { get; }

        /// <inheritdoc/>
        public long SequenceNumber { get; }

        /// <inheritdoc/>
        public ToDoListAggregate ApplyEvent<TPayload>(in Event<TPayload> @event)
        {
            this.Validate(@event);

            // Update our memento if we want to.
            ToDoListMemento updatedMemento = this.ApplyEventToMemento(@event);

            // Add our uncommitted event
            SerializedEvent serializedEvent = EventSerializer.Serialize(@event);
            return new ToDoListAggregate(this.AggregateId, this.SequenceNumber + 1, this.uncommittedEvents.Add(serializedEvent), updatedMemento);
        }

        /// <inheritdoc/>
        public ToDoListAggregate ApplySerializedEvents(in IEnumerable<SerializedEvent> events)
        {
            events.ValidateEvents(this.AggregateId, this.SequenceNumber);

            ToDoListAggregate aggregate = this;

            foreach (SerializedEvent @event in events)
            {
                aggregate = aggregate.ApplySerializedEvent(@event);
            }

            return aggregate;
        }

        /// <inheritdoc/>
        public async ValueTask<ToDoListAggregate> StoreAsync<TEventWriter>(TEventWriter writer)
            where TEventWriter : IEventWriter
        {
            if (this.uncommittedEvents.Length == 0)
            {
                return this;
            }

            await writer.WriteBatchAsync(this.uncommittedEvents).ConfigureAwait(false);
            return new ToDoListAggregate(this.AggregateId, this.SequenceNumber, ImmutableArray<SerializedEvent>.Empty, this.memento);
        }

        /// <inheritdoc/>
        public Task StoreSnapshotAsync<TSnapshotWriter>(TSnapshotWriter writer)
            where TSnapshotWriter : ISnapshotWriter
        {
            var snapshot = new Snapshot<ToDoListMemento>(this.AggregateId, this.SequenceNumber, this.memento);
            return writer.WriteAsync(SnapshotSerializer.Serialize(snapshot));
        }

        private ToDoListMemento ApplyEventToMemento<TPayload>(in Event<TPayload> @event)
        {
            return @event switch
            {
                Event<ToDoItemAddedEventPayload> tdia => this.memento.With(tdia.Payload),
                Event<ToDoItemRemovedEventPayload> tdir => this.memento.With(tdir.Payload),
                _ => throw new InvalidOperationException($"The event type of {@event.EventType} for the event for the aggregate with ID {@event.AggregateId} with sequence number {@event.SequenceNumber} was not recognized."),
            };
        }

        private ToDoListAggregate ApplySerializedEvent(in SerializedEvent @event)
        {
            return @event.EventType switch
            {
                ToDoItemAddedEventPayload.EventType => this.HandleToDoItemAdded(@event),
                ToDoItemRemovedEventPayload.EventType => this.HandleToDoItemRemoved(@event),
                _ => throw new InvalidOperationException($"The event with sequence number {@event.SequenceNumber} had event type {@event.EventType} which was not recognized as a valid event type for the ToDoListAggregate."),
            };
        }

        private ToDoListAggregate HandleToDoItemAdded(in SerializedEvent @event)
        {
            return this.HandleToDoItemAdded(EventSerializer.Deserialize<ToDoItemAddedEventPayload>(@event));
        }

        private ToDoListAggregate HandleToDoItemRemoved(in SerializedEvent @event)
        {
            return this.HandleToDoItemRemoved(EventSerializer.Deserialize<ToDoItemRemovedEventPayload>(@event));
        }

        private ToDoListAggregate HandleToDoItemAdded(in Event<ToDoItemAddedEventPayload> @event)
        {
            return new ToDoListAggregate(this.AggregateId, this.SequenceNumber + 1, this.uncommittedEvents, this.memento.With(@event.Payload));
        }

        private ToDoListAggregate HandleToDoItemRemoved(in Event<ToDoItemRemovedEventPayload> @event)
        {
            return new ToDoListAggregate(this.AggregateId, this.SequenceNumber + 1, this.uncommittedEvents, this.memento.With(@event.Payload));
        }

        private void Validate<TPayload>(in Event<TPayload> @event)
        {
            if (@event.SequenceNumber != this.SequenceNumber + 1)
            {
                throw new ArgumentException($"The event sequence number was incorrect. Expected {this.SequenceNumber + 1}, actual {@event.SequenceNumber}");
            }
        }
    }
}
