// <copyright file="IEventSerializer.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Serialization
{
    using Corvus.EventStore.Core;

    /// <summary>
    /// Serializes an event to and from a <see cref="SerializedEvent"/>.
    /// </summary>
    public interface IEventSerializer
    {
        /// <summary>
        /// Deserializes an event from a SerializedEvent.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload to deserialize.</typeparam>
        /// <param name="event">The event to deserialize.</param>
        /// <returns>The deserialized event.</returns>
        Event<TPayload> Deserialize<TPayload>(SerializedEvent @event);

        /// <summary>
        /// Serializes and event to a <see cref="SerializedEvent"/>.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload to serialize.</typeparam>
        /// <param name="event">The event to serialize.</param>
        /// <returns>A <see cref="SerializedEvent"/> representing the given event.</returns>
        SerializedEvent Serialize<TPayload>(Event<TPayload> @event);
    }
}
