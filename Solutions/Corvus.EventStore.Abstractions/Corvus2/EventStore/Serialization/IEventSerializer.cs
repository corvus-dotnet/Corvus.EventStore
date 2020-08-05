// <copyright file="IEventSerializer.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus2.EventStore.Serialization
{
    using System;
    using Corvus2.EventStore.Core;

    /// <summary>
    /// Serializes an event to and from a <see cref="SerializedEvent"/>.
    /// </summary>
    public interface IEventSerializer
    {
        /// <summary>
        /// Deserializes an event from a SerializedEvent.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to deserialize.</typeparam>
        /// <typeparam name="TPayload">The type of the payload to deserialize.</typeparam>
        /// <param name="event">The event to deserialize.</param>
        /// <param name="factory">The factory method for creating an instance of the target event type.</param>
        /// <returns>The deserialized event.</returns>
        TEvent Deserialize<TEvent, TPayload>(SerializedEvent @event, Func<string, string, long, long, string, TPayload, TEvent> factory)
            where TEvent : IEvent;

        /// <summary>
        /// Serializes and event to a <see cref="SerializedEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to serialize.</typeparam>
        /// <typeparam name="TPayload">The type of the payload to serialize.</typeparam>
        /// <param name="event">The event to serialize.</param>
        /// <returns>A <see cref="SerializedEvent"/> representing the given event.</returns>
        SerializedEvent Serialize<TEvent, TPayload>(TEvent @event)
            where TEvent : IEvent;
    }
}
