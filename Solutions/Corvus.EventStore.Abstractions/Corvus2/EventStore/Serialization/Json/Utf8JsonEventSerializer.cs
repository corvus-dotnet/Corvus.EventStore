// <copyright file="Utf8JsonEventSerializer.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus2.EventStore.Serialization.Json
{
    using System;
    using System.Text.Json;
    using Corvus2.EventStore.Core;

    /// <summary>
    /// An <see cref="IEventSerializer"/> that uses utf8 JSON text.
    /// </summary>
    public readonly struct Utf8JsonEventSerializer : IEventSerializer
    {
        /// <inheritdoc/>
        public TEvent Deserialize<TEvent, TPayload>(SerializedEvent @event, Func<string, string, long, long, string, TPayload, TEvent> factory)
            where TEvent : IEvent
        {
            var reader = new Utf8JsonReader(@event.Utf8TextPayload.Span);
            TPayload payload = JsonSerializer.Deserialize<TPayload>(ref reader);
            return factory(@event.AggregateId, @event.EventType, @event.SequenceNumber, @event.Timestamp, @event.PartitionKey, payload);
        }

        /// <inheritdoc/>
        public SerializedEvent Serialize<TEvent, TPayload>(TEvent @event)
            where TEvent : IEvent
        {
            TPayload payload = @event.GetPayload<TPayload>();
            byte[] utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
            return new SerializedEvent(
                @event.AggregateId,
                @event.EventType,
                @event.SequenceNumber,
                @event.Timestamp,
                @event.PartitionKey,
                utf8Bytes);
        }
    }
}
