// <copyright file="SerializedEventConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.Serialization
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Corvus.EventStore.Core;

    /// <summary>
    /// A type converter for <see cref="SerializedEvent"/> objects.
    /// </summary>
    internal class SerializedEventConverter : JsonConverter<SerializedEvent>
    {
        private readonly JsonEncodedText eventTypeName = JsonEncodedText.Encode("EventType");
        private readonly JsonEncodedText sequenceNumberName = JsonEncodedText.Encode("SequenceNumber");
        private readonly JsonEncodedText timestampName = JsonEncodedText.Encode("Timestamp");
        private readonly JsonEncodedText payloadName = JsonEncodedText.Encode("Payload");

        /// <inheritdoc/>
        public override SerializedEvent Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            (string eventType, long sequenceNumber, long timestamp, ReadOnlyMemory<byte> payload) = (string.Empty, -1, -1, ReadOnlyMemory<byte>.Empty);

            // Read each of the four properties.
            (eventType, sequenceNumber, timestamp, payload) = this.ReadProperty(ref reader, options, (eventType, sequenceNumber, timestamp, payload));
            (eventType, sequenceNumber, timestamp, payload) = this.ReadProperty(ref reader, options, (eventType, sequenceNumber, timestamp, payload));
            (eventType, sequenceNumber, timestamp, payload) = this.ReadProperty(ref reader, options, (eventType, sequenceNumber, timestamp, payload));
            (eventType, sequenceNumber, timestamp, payload) = this.ReadProperty(ref reader, options, (eventType, sequenceNumber, timestamp, payload));

            reader.Read();

            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            return new SerializedEvent(eventType, sequenceNumber, timestamp, payload);
        }

        /// <inheritdoc/>
        public override void Write(
            Utf8JsonWriter writer,
            SerializedEvent serializedEvent,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            ConverterHelpers.WriteProperty(writer, this.eventTypeName, serializedEvent.EventType, options);
            ConverterHelpers.WriteProperty(writer, this.sequenceNumberName, serializedEvent.SequenceNumber, options);
            ConverterHelpers.WriteProperty(writer, this.timestampName, serializedEvent.Timestamp, options);
            ConverterHelpers.WriteProperty(writer, this.payloadName, serializedEvent.Payload, options);
            writer.WriteEndObject();
        }

        private (string eventType, long sequenceNumber, long timestamp, ReadOnlyMemory<byte> payload) ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options, (string eventType, long sequenceNumber, long timestamp, ReadOnlyMemory<byte> payload) result)
        {
            reader.Read();

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            if (reader.ValueTextEquals(this.eventTypeName.EncodedUtf8Bytes))
            {
                return (ConverterHelpers.ReadProperty<string>(ref reader, options), result.sequenceNumber, result.timestamp, result.payload);
            }
            else if (reader.ValueTextEquals(this.sequenceNumberName.EncodedUtf8Bytes))
            {
                return (result.eventType, ConverterHelpers.ReadProperty<long>(ref reader, options), result.timestamp, result.payload);
            }
            else if (reader.ValueTextEquals(this.timestampName.EncodedUtf8Bytes))
            {
                return (result.eventType, result.sequenceNumber, ConverterHelpers.ReadProperty<long>(ref reader, options), result.payload);
            }
            else if (reader.ValueTextEquals(this.payloadName.EncodedUtf8Bytes))
            {
                return (result.eventType, result.sequenceNumber, result.timestamp, ConverterHelpers.ReadProperty<ReadOnlyMemory<byte>>(ref reader, options));
            }
            else
            {
                throw new JsonException();
            }
        }
    }
}
