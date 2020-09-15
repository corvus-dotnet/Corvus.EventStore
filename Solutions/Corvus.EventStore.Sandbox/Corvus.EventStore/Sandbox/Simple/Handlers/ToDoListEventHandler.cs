// <copyright file="ToDoListEventHandler.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox.Handlers
{
    using System;
    using Corvus.EventStore.Json;
    using Corvus.EventStore.Sandbox.Events;
    using Corvus.EventStore.Sandbox.Mementos;
    using Corvus.Extensions;

    /// <summary>
    /// An event handler which can apply events to the ToDoListMemento.
    /// </summary>
    /// <remarks>Note that this uses the generic IEventHandler mechanism, rather than an optimized type-specific handler such as <see cref="IJsonEventHandler{TMemento}"/>.</remarks>
    internal class ToDoListEventHandler : IEventHandler<ToDoListMemento>
    {
        /// <summary>
        /// Gets the default instance of the <see cref="ToDoListEventHandler"/>.
        /// </summary>
        public static ToDoListEventHandler Instance => new ToDoListEventHandler();

        /// <inheritdoc/>
        public ToDoListMemento HandleEvent<TPayload>(Guid aggregateId, long commitSequenceNumber, string eventType, long eventSequenceNumber, ToDoListMemento memento, TPayload payload)
        {
            return eventType switch
            {
                ToDoItemAddedEventPayload.EventType => memento.With(CastTo<ToDoItemAddedEventPayload>.From(payload)),
                ToDoItemRemovedEventPayload.EventType => memento.With(CastTo<ToDoItemRemovedEventPayload>.From(payload)),
                ToDoListOwnerSetEventPayload.EventType => memento.With(CastTo<ToDoListOwnerSetEventPayload>.From(payload)),
                ToDoListStartDateSetEventPayload.EventType => memento.With(CastTo<ToDoListStartDateSetEventPayload>.From(payload)),
                _ => throw new InvalidOperationException($"The event for aggregate {aggregateId} in commit {commitSequenceNumber} with event sequence number {eventSequenceNumber} had event type {eventType} which was not recognized as a valid event type for the ToDoListAggregate."),
            };
        }

        /// <inheritdoc/>
        public ToDoListMemento HandleSerializedEvent(Guid aggregateId, long commitSequenceNumber, string eventType, long eventSequenceNumber, ToDoListMemento memento, IPayloadReader payloadReader)
        {
            return eventType switch
            {
                ToDoItemAddedEventPayload.EventType => memento.With(payloadReader.Read<ToDoItemAddedEventPayload>()),
                ToDoItemRemovedEventPayload.EventType => memento.With(payloadReader.Read<ToDoItemRemovedEventPayload>()),
                ToDoListOwnerSetEventPayload.EventType => memento.With(payloadReader.Read<ToDoListOwnerSetEventPayload>()),
                ToDoListStartDateSetEventPayload.EventType => memento.With(payloadReader.Read<ToDoListStartDateSetEventPayload>()),
                _ => throw new InvalidOperationException($"The event for aggregate {aggregateId} in commit {commitSequenceNumber} with event sequence number {eventSequenceNumber} had event type {eventType} which was not recognized as a valid event type for the ToDoListAggregate."),
            };
        }
    }
}
