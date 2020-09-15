// <copyright file="ToDoListJsonEventHandler.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox.Handlers
{
    using System;
    using System.Text.Json;
    using Corvus.EventStore.Json;
    using Corvus.EventStore.Sandbox.Events;
    using Corvus.EventStore.Sandbox.Mementos;
    using Corvus.Extensions;

    /// <summary>
    /// An event handler which can apply events to the ToDoListMementoJson.
    /// </summary>
    /// <remarks>Note that this uses the optimized <see cref="IJsonEventHandler{TMemento}"/>. We avoid allocating strings for our event type comparisons during dispatch,
    /// and do not have to parse (just skip over) the elements in the serailized events that we don't need to apply the event to our aggregate.
    /// </remarks>
    internal readonly struct ToDoListJsonEventHandler : IJsonEventHandler<ToDoListMementoJson>
    {
        /// <summary>
        /// Gets the default instance of the <see cref="ToDoListJsonEventHandler"/>.
        /// </summary>
        public static ToDoListJsonEventHandler Instance => default;

        /// <inheritdoc/>
        public ToDoListMementoJson HandleEvent<TPayload>(Guid aggregateId, long commitSequenceNumber, JsonEncodedText eventType, long eventSequenceNumber, ToDoListMementoJson memento, TPayload payload)
        {
            if (eventType.Equals(ToDoItemAddedEventJsonPayload.EncodedEventType))
            {
                return memento.WithToDoItemAdded(CastTo<ToDoItemAddedEventJsonPayload>.From(payload).Id);
            }

            if (eventType.Equals(ToDoItemRemovedEventJsonPayload.EncodedEventType))
            {
                return memento.WithToDoItemRemoved(CastTo<ToDoItemRemovedEventJsonPayload>.From(payload).ToDoItemId);
            }

            if (eventType.Equals(ToDoListOwnerSetEventJsonPayload.EncodedEventType))
            {
                return memento.WithOwner(CastTo<ToDoListOwnerSetEventJsonPayload>.From(payload).Owner);
            }

            if (eventType.Equals(ToDoListStartDateSetEventJsonPayload.EncodedEventType))
            {
                return memento.WithStartDate(CastTo<ToDoListStartDateSetEventJsonPayload>.From(payload).StartDate);
            }

            throw new InvalidOperationException($"The event for aggregate {aggregateId} in commit {commitSequenceNumber} with event sequence number {eventSequenceNumber} had event type {eventType} which was not recognized as a valid event type for the ToDoListAggregate.");
        }

        /// <inheritdoc/>
        public ToDoListMementoJson HandleSerializedEvent(ref Utf8JsonStreamReader streamReader, Guid aggregateId, long commitSequenceNumber, long expectedEventSequenceNumber, ToDoListMementoJson memento)
        {
            long eventSequenceNumber = JsonEventHandler.ReadEventSequenceNumber(ref streamReader);

            if (expectedEventSequenceNumber != eventSequenceNumber)
            {
                throw new JsonException($"Unexpected event sequence number for aggregate {aggregateId} in commit {commitSequenceNumber}. Expected {expectedEventSequenceNumber} but was {eventSequenceNumber}.");
            }

            // Find the event type
            JsonEventHandler.FindEventType(ref streamReader);

            if (streamReader.Match(ToDoItemAddedEventJsonPayload.EventType))
            {
                JsonEventHandler.FindPayload(ref streamReader);
                memento = memento.WithToDoItemAdded(ToDoItemAddedEventJsonPayload.Converter.ReadToDoItemId(ref streamReader));
            }
            else if (streamReader.Match(ToDoItemRemovedEventJsonPayload.EventType))
            {
                JsonEventHandler.FindPayload(ref streamReader);
                memento = memento.WithToDoItemRemoved(ToDoItemRemovedEventJsonPayload.Converter.ReadToDoItemId(ref streamReader));
            }
            else if (streamReader.Match(ToDoListOwnerSetEventJsonPayload.EventType))
            {
                JsonEventHandler.FindPayload(ref streamReader);
                memento = memento.WithOwner(ToDoListOwnerSetEventJsonPayload.Converter.ReadOwner(ref streamReader));
            }
            else if (streamReader.Match(ToDoListStartDateSetEventJsonPayload.EventType))
            {
                JsonEventHandler.FindPayload(ref streamReader);
                memento = memento.WithStartDate(ToDoListStartDateSetEventJsonPayload.Converter.ReadStartDate(ref streamReader));
            }
            else
            {
                throw new InvalidOperationException($"The event for aggregate {aggregateId} in commit {commitSequenceNumber} with event sequence number {eventSequenceNumber} had event type {streamReader.GetString()} which was not recognized as a valid event type for the ToDoListAggregate.");
            }

            JsonEventHandler.ReadToEndOfEvent(ref streamReader);
            return memento;
        }
    }
}
