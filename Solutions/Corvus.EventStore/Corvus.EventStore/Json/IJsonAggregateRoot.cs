// <copyright file="IJsonAggregateRoot.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Json
{
    using System;
    using System.Text.Json;
    using Corvus.EventStore;

    /// <summary>
    /// An aggregate root which supports custom Json serialization of its payload.
    /// </summary>
    /// <typeparam name="TMemento">The type of the memento for the aggregate root.</typeparam>
    /// <typeparam name="T">The type implementing this interface.</typeparam>
    public interface IJsonAggregateRoot<TMemento, T> : IAggregateRoot<TMemento, T>
        where T : IJsonAggregateRoot<TMemento, T>
    {
        /// <summary>
        /// Apply an event to an aggregate root using the specified payload writer.
        /// </summary>
        /// <typeparam name="TPayload">The payload to write in the event.</typeparam>
        /// <typeparam name="TPayloadWriter">The type of the writer to use to write the payload.</typeparam>
        /// <typeparam name="TEventHandler">The type of the <see cref="IEventHandler{TMemento}"/>.</typeparam>
        /// <param name="eventType">The event type as <see cref="JsonEncodedText"/>.</param>
        /// <param name="payload">The payload to write.</param>
        /// <param name="payloadWriter">An <see cref="Action"/> that can write the given payload to the stream.</param>
        /// <param name="eventHandler">The <see cref="IEventHandler{TMemento}"/> that can apply the event to the memento.</param>
        /// <returns>The aggregate root with the event applied.</returns>
        T ApplyEvent<TPayload, TPayloadWriter, TEventHandler>(JsonEncodedText eventType, TPayload payload, TPayloadWriter payloadWriter, TEventHandler eventHandler)
            where TPayloadWriter : IJsonEventPayloadWriter<TPayload>
            where TEventHandler : IJsonEventHandler<TMemento>;

        /// <summary>
        /// Apply an event to an aggregate root. The payload is capable of writing itself to the output stream.
        /// </summary>
        /// <typeparam name="TPayload">The payload to write in the event.</typeparam>
        /// <typeparam name="TEventHandler">The type of the <see cref="IEventHandler{TMemento}"/>.</typeparam>
        /// <param name="eventType">The event type as <see cref="JsonEncodedText"/>.</param>
        /// <param name="payload">The payload to write.</param>
        /// <param name="eventHandler">The <see cref="IEventHandler{TMemento}"/> that can apply the event to the memento.</param>
        /// <returns>The aggregate root with the event applied.</returns>
        T ApplyEvent<TPayload, TEventHandler>(JsonEncodedText eventType, TPayload payload, TEventHandler eventHandler)
            where TPayload : IJsonEventPayloadWriter<TPayload>
            where TEventHandler : IJsonEventHandler<TMemento>;
    }
}
