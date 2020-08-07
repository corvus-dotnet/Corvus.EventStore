// <copyright file="ToDoListEventHandler.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example.Internal.EventHandlers
{
    using System;
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Example.Internal.Events;
    using Corvus.EventStore.Example.Internal.Mementos;

    /// <summary>
    /// The event handler for a ToDo list containing ToDo items.
    /// </summary>
    /// <remarks>
    /// Notice that we are using direct dispatch here for efficiency. For separation of concerns and extensibility, you could choose to
    /// use our contenttype dispatch pattern and wrap all these things up into individual handler types. But I'm not sure you gain much.
    /// </remarks>
    internal readonly struct ToDoListEventHandler : IAggregateEventHandler<ToDoListEventHandler, ToDoListMemento>
    {
        /// <inheritdoc/>
        public ToDoListMemento HandleEvent<TPayload>(in ToDoListMemento memento, in Event<TPayload> @event)
        {
            return @event switch
            {
                Event<ToDoItemAddedEventPayload> tdia => memento.With(tdia.Payload),
                Event<ToDoItemRemovedEventPayload> tdir => memento.With(tdir.Payload),
                Event<ToDoListOwnerSetEventPayload> tdir => memento.With(tdir.Payload),
                Event<ToDoListStartDateSetEventPayload> tdir => memento.With(tdir.Payload),
                _ => throw new InvalidOperationException($"The event type of {@event.EventType} for the event with event sequence number {@event.SequenceNumber} was not recognized."),
            };
        }

        /// <inheritdoc/>
        public ToDoListMemento HandleSerializedEvent(in ToDoListMemento memento, in SerializedEvent @event)
        {
            return @event.EventType switch
            {
                ToDoItemAddedEventPayload.EventType => this.HandleToDoItemAdded(memento, @event),
                ToDoItemRemovedEventPayload.EventType => this.HandleToDoItemRemoved(memento, @event),
                ToDoListOwnerSetEventPayload.EventType => this.HandleOwnerSet(memento, @event),
                ToDoListStartDateSetEventPayload.EventType => this.HandleStartDateSet(memento, @event),
                _ => throw new InvalidOperationException($"The event with sequence number {@event.SequenceNumber} had event type {@event.EventType} which was not recognized as a valid event type for the ToDoListAggregate."),
            };
        }

        private ToDoListMemento HandleToDoItemAdded(in ToDoListMemento memento, in SerializedEvent @event)
        {
            return memento.With(AggregateWithMemento<ToDoListEventHandler, ToDoListMemento>.Deserialize<ToDoItemAddedEventPayload>(@event).Payload);
        }

        private ToDoListMemento HandleToDoItemRemoved(in ToDoListMemento memento, in SerializedEvent @event)
        {
            return memento.With(AggregateWithMemento<ToDoListEventHandler, ToDoListMemento>.Deserialize<ToDoItemRemovedEventPayload>(@event).Payload);
        }

        private ToDoListMemento HandleOwnerSet(in ToDoListMemento memento, in SerializedEvent @event)
        {
            return memento.With(AggregateWithMemento<ToDoListEventHandler, ToDoListMemento>.Deserialize<ToDoListOwnerSetEventPayload>(@event).Payload);
        }

        private ToDoListMemento HandleStartDateSet(in ToDoListMemento memento, in SerializedEvent @event)
        {
            return memento.With(AggregateWithMemento<ToDoListEventHandler, ToDoListMemento>.Deserialize<ToDoListStartDateSetEventPayload>(@event).Payload);
        }
    }
}
