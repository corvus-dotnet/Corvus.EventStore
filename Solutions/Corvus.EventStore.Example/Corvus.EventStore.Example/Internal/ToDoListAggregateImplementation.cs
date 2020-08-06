// <copyright file="ToDoListAggregateImplementation.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example
{
    using System;
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Example.Internal;

    /// <summary>
    /// The aggregate root for a ToDo list containing ToDo items.
    /// </summary>
    internal readonly struct ToDoListAggregateImplementation : IAggregateImplementationWithMemento<ToDoListAggregateImplementation, ToDoListMemento>
    {
        /// <inheritdoc/>
        public ToDoListMemento ApplyEvent<TPayload>(in ToDoListMemento memento, in Event<TPayload> @event)
        {
            return @event switch
            {
                Event<ToDoItemAddedEventPayload> tdia => memento.With(tdia.Payload),
                Event<ToDoItemRemovedEventPayload> tdir => memento.With(tdir.Payload),
                _ => throw new InvalidOperationException($"The event type of {@event.EventType} for the event with event sequence number {@event.SequenceNumber} was not recognized."),
            };
        }

        /// <inheritdoc/>
        public ToDoListMemento ApplySerializedEvent(in ToDoListMemento memento, in SerializedEvent @event)
        {
            return @event.EventType switch
            {
                ToDoItemAddedEventPayload.EventType => this.HandleToDoItemAdded(memento, @event),
                ToDoItemRemovedEventPayload.EventType => this.HandleToDoItemRemoved(memento, @event),
                _ => throw new InvalidOperationException($"The event with sequence number {@event.SequenceNumber} had event type {@event.EventType} which was not recognized as a valid event type for the ToDoListAggregate."),
            };
        }

        private ToDoListMemento HandleToDoItemAdded(in ToDoListMemento memento, in SerializedEvent @event)
        {
            return memento.With(AggregateWithMemento<ToDoListAggregateImplementation, ToDoListMemento>.Deserialize<ToDoItemAddedEventPayload>(@event).Payload);
        }

        private ToDoListMemento HandleToDoItemRemoved(in ToDoListMemento memento, in SerializedEvent @event)
        {
            return memento.With(AggregateWithMemento<ToDoListAggregateImplementation, ToDoListMemento>.Deserialize<ToDoItemRemovedEventPayload>(@event).Payload);
        }
    }
}
