// <copyright file="Utf8JsonEventSerializer.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Serialization.Json
{
    using System.Text.Json;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Serialization;

    /// <summary>
    /// An <see cref="IEventSerializer"/> that uses utf8 JSON text.
    /// </summary>
    public readonly struct Utf8JsonEventSerializer : IEventSerializer
    {
        private readonly JsonSerializerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="Utf8JsonEventSerializer"/> struct.
        /// </summary>
        /// <param name="options">The <see cref="JsonSerializerOptions"/>.</param>
        public Utf8JsonEventSerializer(JsonSerializerOptions options)
        {
            this.options = options;
        }

        /// <inheritdoc/>
        public Event<TPayload> Deserialize<TPayload>(in SerializedEvent @event)
        {
            var reader = new Utf8JsonReader(@event.Payload.Span);
            TPayload payload = JsonSerializer.Deserialize<TPayload>(ref reader, this.options);
            return new Event<TPayload>(@event.EventType, @event.SequenceNumber, @event.Timestamp, payload);
        }

        /// <inheritdoc/>
        public SerializedEvent Serialize<TPayload>(in Event<TPayload> @event)
        {
            byte[] utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(@event.Payload, this.options);
            return new SerializedEvent(
                @event.EventType,
                @event.SequenceNumber,
                @event.Timestamp,
                utf8Bytes);
        }
    }
}
