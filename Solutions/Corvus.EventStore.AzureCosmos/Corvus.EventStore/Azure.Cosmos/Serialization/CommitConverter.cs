// <copyright file="CommitConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.Serialization
{
    using System;
    using System.Collections.Immutable;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Corvus.EventStore.Core;

    /// <summary>
    /// A type converter for <see cref="Commit"/> objects.
    /// </summary>
    internal class CommitConverter : JsonConverter<Commit>
    {
        private readonly JsonEncodedText aggregateIdName = JsonEncodedText.Encode("AggregateId");
        private readonly JsonEncodedText partitionKeyName = JsonEncodedText.Encode("PartitionKey");
        private readonly JsonEncodedText sequenceNumberName = JsonEncodedText.Encode("SequenceNumber");
        private readonly JsonEncodedText timestampName = JsonEncodedText.Encode("Timestamp");
        private readonly JsonEncodedText eventsName = JsonEncodedText.Encode("Events");

        /// <inheritdoc/>
        public override Commit Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            (Guid aggregateId, string partitionKey, long sequenceNumber, long timestamp, ImmutableArray<SerializedEvent> events) = (Guid.Empty, string.Empty, -1, -1, ImmutableArray<SerializedEvent>.Empty);

            // Read each of the 5 properties.
            (aggregateId, partitionKey, sequenceNumber, timestamp, events) = this.ReadProperty(ref reader, options, (aggregateId, partitionKey, sequenceNumber, timestamp, events));
            (aggregateId, partitionKey, sequenceNumber, timestamp, events) = this.ReadProperty(ref reader, options, (aggregateId, partitionKey, sequenceNumber, timestamp, events));
            (aggregateId, partitionKey, sequenceNumber, timestamp, events) = this.ReadProperty(ref reader, options, (aggregateId, partitionKey, sequenceNumber, timestamp, events));
            (aggregateId, partitionKey, sequenceNumber, timestamp, events) = this.ReadProperty(ref reader, options, (aggregateId, partitionKey, sequenceNumber, timestamp, events));
            (aggregateId, partitionKey, sequenceNumber, timestamp, events) = this.ReadProperty(ref reader, options, (aggregateId, partitionKey, sequenceNumber, timestamp, events));

            reader.Read();

            if (reader.TokenType != JsonTokenType.EndObject)
            {
                throw new JsonException();
            }

            return new Commit(aggregateId, partitionKey, sequenceNumber, timestamp, events);
        }

        /// <inheritdoc/>
        public override void Write(
            Utf8JsonWriter writer,
            Commit serializedEvent,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            ConverterHelpers.WriteProperty(writer, this.aggregateIdName, serializedEvent.AggregateId, options);
            ConverterHelpers.WriteProperty(writer, this.partitionKeyName, serializedEvent.PartitionKey, options);
            ConverterHelpers.WriteProperty(writer, this.sequenceNumberName, serializedEvent.SequenceNumber, options);
            ConverterHelpers.WriteProperty(writer, this.timestampName, serializedEvent.Timestamp, options);
            ConverterHelpers.WriteProperty(writer, this.eventsName, serializedEvent.Events, options);
            writer.WriteEndObject();
        }

        private (Guid aggregateId, string partitionKey, long sequenceNumber, long timestamp, ImmutableArray<SerializedEvent> events) ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options, (Guid aggregateId, string partitionKey, long sequenceNumber, long timestamp, ImmutableArray<SerializedEvent> events) result)
        {
            reader.Read();

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            if (reader.ValueTextEquals(this.aggregateIdName.EncodedUtf8Bytes))
            {
                return (ConverterHelpers.ReadProperty<Guid>(ref reader, options), result.partitionKey, result.sequenceNumber, result.timestamp, result.events);
            }
            else if (reader.ValueTextEquals(this.partitionKeyName.EncodedUtf8Bytes))
            {
                return (result.aggregateId, ConverterHelpers.ReadProperty<string>(ref reader, options), result.sequenceNumber, result.timestamp, result.events);
            }
            else if (reader.ValueTextEquals(this.sequenceNumberName.EncodedUtf8Bytes))
            {
                return (result.aggregateId, result.partitionKey, ConverterHelpers.ReadProperty<long>(ref reader, options), result.timestamp, result.events);
            }
            else if (reader.ValueTextEquals(this.timestampName.EncodedUtf8Bytes))
            {
                return (result.aggregateId, result.partitionKey, result.sequenceNumber, ConverterHelpers.ReadProperty<long>(ref reader, options), result.events);
            }
            else if (reader.ValueTextEquals(this.eventsName.EncodedUtf8Bytes))
            {
                return (result.aggregateId, result.partitionKey, result.sequenceNumber, result.timestamp, ConverterHelpers.ReadProperty<ImmutableArray<SerializedEvent>>(ref reader, options));
            }
            else
            {
                throw new JsonException();
            }
        }
    }
}
