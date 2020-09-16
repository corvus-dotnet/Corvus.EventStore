// <copyright file="JsonAggregateRoot{TMemento,TJsonStore}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Json
{
    using System;
    using System.Buffers;
    using System.Buffers.Text;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Corvus.EventStore;
    using Corvus.Extensions;

    /// <summary>
    /// An aggregate root implemented over an <see cref="IJsonStore"/>.
    /// </summary>
    /// <typeparam name="TMemento">The type of the internal memento for the aggregate root.</typeparam>
    /// <typeparam name="TJsonStore">The type of the Json Store to be used by this aggregate root.</typeparam>
    public readonly struct JsonAggregateRoot<TMemento, TJsonStore> : IJsonAggregateRoot<TMemento, JsonAggregateRoot<TMemento, TJsonStore>>
        where TJsonStore : class, IJsonStore
    {
        private readonly TJsonStore jsonStore;
        private readonly ArrayBufferWriter<byte> bufferWriter;
        private readonly Utf8JsonWriter utf8JsonWriter;
        private readonly JsonSerializerOptions options;
        private readonly JsonEncodedText encodedPartitionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonAggregateRoot{TMemento,TJsonStore}"/> struct.
        /// </summary>
        /// <param name="id">The unique ID of the aggregate root.</param>
        /// <param name="memento">The current memento.</param>
        /// <param name="jsonStore">The json store from which to read and write the json stream.</param>
        /// <param name="bufferWriter">The buffer writer into which we are writing.</param>
        /// <param name="utf8JsonWriter">The Utf8 json writer over which this aggregate root is implemented.</param>
        /// <param name="encodedPartitionKey">Json encoded text for the partition key.</param>
        /// <param name="eventSequenceNumber">The <see cref="EventSequenceNumber"/>.</param>
        /// <param name="commitSequenceNumber">The <see cref="CommitSequenceNumber"/>.</param>
        /// <param name="hasUncommittedEvents">A valut that indicates whether the aggregate has uncommitted events.</param>
        /// <param name="storeMetadata">Metadata applied to the root by the store.</param>
        /// <param name="options">The JSON serializer options for the aggregate root.</param>
        public JsonAggregateRoot(Guid id, TMemento memento, TJsonStore jsonStore, ArrayBufferWriter<byte> bufferWriter, Utf8JsonWriter utf8JsonWriter, JsonEncodedText encodedPartitionKey, long eventSequenceNumber, long commitSequenceNumber, bool hasUncommittedEvents, ReadOnlyMemory<byte> storeMetadata, JsonSerializerOptions options)
        {
            this.Id = id;
            this.Memento = memento;
            this.jsonStore = jsonStore;
            this.bufferWriter = bufferWriter;
            this.utf8JsonWriter = utf8JsonWriter;
            this.encodedPartitionKey = encodedPartitionKey;
            this.EventSequenceNumber = eventSequenceNumber;
            this.CommitSequenceNumber = commitSequenceNumber;
            this.HasUncommittedEvents = hasUncommittedEvents;
            this.StoreMetadata = storeMetadata;
            this.options = options;
        }

        /// <inheritdoc/>
        public Guid Id { get; }

        /// <inheritdoc/>
        public long EventSequenceNumber { get; }

        /// <inheritdoc/>
        public long CommitSequenceNumber { get; }

        /// <inheritdoc/>
        public bool HasUncommittedEvents { get; }

        /// <inheritdoc/>
        public TMemento Memento { get; }

        /// <inheritdoc/>
        public ReadOnlyMemory<byte> StoreMetadata { get; }

        /// <summary>
        /// Process an array of commits.
        /// </summary>
        /// <typeparam name="TEventHandler">The type of the event handler.</typeparam>
        /// <param name="aggregateId">The aggregate ID whose commits are being processed.</param>
        /// <param name="commitSequenceNumber">The starting commit sequence number.</param>
        /// <param name="eventSequenceNumber">The starting event sequence number.</param>
        /// <param name="memento">The starting memento.</param>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="streamReader">The stream reader.</param>
        /// <returns>The updated memento, commit sequence number and event sequence number once the events have been processed.</returns>
        public static (TMemento, long, long) ProcessCommits<TEventHandler>(Guid aggregateId, long commitSequenceNumber, long eventSequenceNumber, TMemento memento, TEventHandler eventHandler, ref Utf8JsonStreamReader streamReader)
            where TEventHandler : IJsonEventHandler<TMemento>
        {
            // Advance from the start array to the start of the object
            streamReader.Read();

            while (streamReader.TokenType != JsonTokenType.EndArray)
            {
                (memento, eventSequenceNumber) = ProcessCommit(aggregateId, commitSequenceNumber, eventSequenceNumber, memento, eventHandler, ref streamReader);
                commitSequenceNumber += 1;

                FindNextCommit(ref streamReader);
            }

            return (memento, commitSequenceNumber, eventSequenceNumber);
        }

        /// <summary>
        /// Process a single commit.
        /// </summary>
        /// <typeparam name="TEventHandler">The type of the event handler.</typeparam>
        /// <param name="aggregateId">The aggregate ID whose commits are being processed.</param>
        /// <param name="commitSequenceNumber">The starting commit sequence number.</param>
        /// <param name="eventSequenceNumber">The starting event sequence number.</param>
        /// <param name="memento">The starting memento.</param>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="streamReader">The stream reader.</param>
        /// <returns>The updated memento, commit sequence number and event sequence number once the events have been processed.</returns>
        public static (TMemento, long) ProcessCommit<TEventHandler>(Guid aggregateId, long commitSequenceNumber, long eventSequenceNumber, in TMemento memento, in TEventHandler eventHandler, ref Utf8JsonStreamReader streamReader)
            where TEventHandler : IJsonEventHandler<TMemento>
        {
            ValidateCommitAndFindEvents(aggregateId, commitSequenceNumber, ref streamReader);

            return ProcessEvents(aggregateId, commitSequenceNumber, eventSequenceNumber, memento, eventHandler, ref streamReader);
        }

        /// <inheritdoc/>
        public JsonAggregateRoot<TMemento, TJsonStore> ApplyEvent<TPayload>(string eventType, in TPayload payload, IEventHandler<TMemento> eventHandler)
        {
            return this.ApplyEvent(JsonEncodedText.Encode(eventType), payload, new JsonSerializerPayloadWriter<TPayload>(this.options), new JsonEventHandlerOverJsonSerializer(eventHandler, this.options));
        }

        /// <inheritdoc/>
        public JsonAggregateRoot<TMemento, TJsonStore> ApplyEvent<TPayload, TEventHandler>(JsonEncodedText eventType, in TPayload payload, in TEventHandler eventHandler)
            where TPayload : IJsonEventPayloadWriter<TPayload>
            where TEventHandler : IJsonEventHandler<TMemento>
        {
            return this.ApplyEvent(eventType, payload, payload, eventHandler);
        }

        /// <inheritdoc/>
        public JsonAggregateRoot<TMemento, TJsonStore> ApplyEvent<TPayload, TPayloadWriter, TEventHandler>(JsonEncodedText eventType, in TPayload payload, in TPayloadWriter payloadWriter, in TEventHandler eventHandler)
            where TPayloadWriter : IJsonEventPayloadWriter<TPayload>
            where TEventHandler : IJsonEventHandler<TMemento>
        {
            if (!this.HasUncommittedEvents)
            {
                WriteStartCommit(this.utf8JsonWriter, this.Id, this.CommitSequenceNumber + 1, this.encodedPartitionKey);
            }

            WriteEvent(this.utf8JsonWriter, eventType, this.EventSequenceNumber + 1, payload, payloadWriter);

            TMemento memento = eventHandler.HandleEvent(this.Id, this.CommitSequenceNumber, eventType, this.EventSequenceNumber + 1, this.Memento, payload);

            return new JsonAggregateRoot<TMemento, TJsonStore>(this.Id, memento, this.jsonStore, this.bufferWriter, this.utf8JsonWriter, this.encodedPartitionKey, this.EventSequenceNumber + 1, this.CommitSequenceNumber, true, this.StoreMetadata, this.options);
        }

        /// <inheritdoc/>
        public async Task<JsonAggregateRoot<TMemento, TJsonStore>> Commit()
        {
            WriteEndCommit(this.utf8JsonWriter);

            using var stream = new ReadOnlyMemoryStream(this.bufferWriter.WrittenMemory);

            await this.jsonStore.Write(stream, this.Id, this.CommitSequenceNumber + 1, this.encodedPartitionKey).ConfigureAwait(false);

            this.bufferWriter.Clear();
            this.utf8JsonWriter.Reset();

            return new JsonAggregateRoot<TMemento, TJsonStore>(this.Id, this.Memento, this.jsonStore, this.bufferWriter, this.utf8JsonWriter, this.encodedPartitionKey, this.EventSequenceNumber, this.CommitSequenceNumber + 1, false, this.StoreMetadata, this.options);
        }

        /// <summary>
        /// Writes the preamble for the commit up to and including the start of the Event array.
        /// </summary>
        /// <param name="utf8JsonWriter">The writer to which to write the commit.</param>
        /// <param name="aggregateId">The aggregate ID for the commit.</param>
        /// <param name="commitSequenceNumber">The sequence number for the commit.</param>
        /// <param name="encodedPartitionKey">The encoded partition key for the commit.</param>
        private static void WriteStartCommit(Utf8JsonWriter utf8JsonWriter, Guid aggregateId, long commitSequenceNumber, JsonEncodedText encodedPartitionKey)
        {
            // Rent a small buffer - sufficient for a 32-character Guid + separators, plus a long
            byte[] buffer = ArrayPool<byte>.Shared.Rent(120);

            try
            {
                Utf8Formatter.TryFormat(aggregateId, buffer.AsSpan(), out int bytesWrittenForId);
                Utf8Formatter.TryFormat(commitSequenceNumber, buffer.AsSpan().Slice(bytesWrittenForId), out int bytesWrittenForSequenceNumber);

                // Changing property ordering is considered a breaking change in the schema, and the reading half should
                // also be modified to deal with either old or new versioning.
                utf8JsonWriter.WriteStartObject();
                utf8JsonWriter.WriteString(JsonCommit.IdPropertyName, buffer.AsSpan().Slice(0, bytesWrittenForId + bytesWrittenForSequenceNumber));
                utf8JsonWriter.WriteString(JsonCommit.PartitionKeyPropertyName, encodedPartitionKey);
                utf8JsonWriter.WriteString(JsonCommit.AggregateIdPropertyName, buffer.AsSpan().Slice(0, bytesWrittenForId));
                utf8JsonWriter.WriteNumber(JsonCommit.CommitSequenceNumberPropertyName, commitSequenceNumber);
                utf8JsonWriter.WritePropertyName(JsonCommit.EventsPropertyName);
                utf8JsonWriter.WriteStartArray();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Write an event to a <see cref="Utf8JsonWriter"/>.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload.</typeparam>
        /// <typeparam name="TPayloadWriter">The type of the writer for the payload.</typeparam>
        /// <param name="utf8JsonWriter">The <see cref="Utf8JsonWriter"/> to which to write the event.</param>
        /// <param name="eventType">The type of the event to write.</param>
        /// <param name="eventSequenceNumber">The event sequence number.</param>
        /// <param name="payload">The payload to write.</param>
        /// <param name="payloadWriter">A writer for the payload.</param>
        private static void WriteEvent<TPayload, TPayloadWriter>(Utf8JsonWriter utf8JsonWriter, JsonEncodedText eventType, long eventSequenceNumber, in TPayload payload, in TPayloadWriter payloadWriter)
            where TPayloadWriter : IJsonEventPayloadWriter<TPayload>
        {
            utf8JsonWriter.WriteStartObject();

            utf8JsonWriter.WriteNumber(JsonCommit.EventSequenceNumberPropertyName, eventSequenceNumber);
            utf8JsonWriter.WriteString(JsonCommit.EventTypePropertyName, eventType);

            utf8JsonWriter.WritePropertyName(JsonCommit.EventPayloadPropertyName);

            payloadWriter.Write(payload, utf8JsonWriter);

            utf8JsonWriter.WriteEndObject();
        }

        /// <summary>
        /// This writes the end of a commit to the stream.
        /// </summary>
        /// <param name="utf8JsonWriter">The writer to which to write the end of the commit.</param>
        private static void WriteEndCommit(Utf8JsonWriter utf8JsonWriter)
        {
            utf8JsonWriter.WriteEndArray();
            utf8JsonWriter.WriteEndObject();

            // We flush the commit to the writer at this point.
            utf8JsonWriter.Flush();
        }

        /// <summary>
        /// Validate a commit and find the array of Events.
        /// </summary>
        /// <param name="aggregateId">The expected aggregate ID for the commit.</param>
        /// <param name="commitSequenceNumber">The expected commit sequence number.</param>
        /// <param name="streamReader">The stream reader in which to find the commit events.</param>
        /// <remarks>
        /// This leaves the reader at the start of the events array.
        /// </remarks>
        private static void ValidateCommitAndFindEvents(Guid aggregateId, long commitSequenceNumber, ref Utf8JsonStreamReader streamReader)
        {
            if (streamReader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected to find the start of a Commit object");
            }

            // Because we have tight control of reader and writer, we expect to find exactly the property
            // ordering we created; this helps us be efficient in our read.
            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.PropertyName || !streamReader.Match(JsonCommit.IdPropertyNameString))
            {
                throw new JsonException($"Expected to find the {JsonCommit.IdPropertyNameString} property.");
            }

            // Read past the ID value
            streamReader.Read();

            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.PropertyName || !streamReader.Match(JsonCommit.PartitionKeyPropertyNameString))
            {
                throw new JsonException($"Expected to find the {JsonCommit.PartitionKeyPropertyNameString} property.");
            }

            // Read past the partition key value
            streamReader.Read();

            // Now, read the aggregate ID
            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.PropertyName || !streamReader.Match(JsonCommit.AggregateIdPropertyNameString))
            {
                throw new JsonException($"Expected to find the {JsonCommit.AggregateIdPropertyNameString} property.");
            }

            // Now read the value of the aggregate ID and validate it
            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.String || streamReader.GetGuid() != aggregateId)
            {
                throw new JsonException("Unexpected Aggregate ID matched.");
            }

            // Now, read the commit sequence number
            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.PropertyName || !streamReader.Match(JsonCommit.CommitSequenceNumberPropertyNameString))
            {
                throw new JsonException($"Expected to find the {JsonCommit.CommitSequenceNumberPropertyNameString} property.");
            }

            // Now read the value of the aggregate ID and validate it
            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.Number || streamReader.GetInt64() != commitSequenceNumber + 1)
            {
                throw new JsonException($"Unexpected commit sequence number matched. Expected {commitSequenceNumber + 1}, was {streamReader.GetInt64()}");
            }

            // Then find the events and iterate on those.
            streamReader.Read();

            if (streamReader.TokenType != JsonTokenType.PropertyName || !streamReader.Match(JsonCommit.EventsPropertyNameString))
            {
                throw new JsonException($"Expected to find the {JsonCommit.EventsPropertyNameString} property.");
            }

            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException($"Expected the {JsonCommit.EventsPropertyNameString} property to be an array of event objects.");
            }
        }

        private static void FindNextCommit(ref Utf8JsonStreamReader streamReader)
        {
            do
            {
                if (!streamReader.Read())
                {
                    throw new JsonException("Expected to find the end of the Commit object.");
                }
            }
            while (streamReader.TokenType != JsonTokenType.EndObject);

            if (!streamReader.Read())
            {
                throw new JsonException("Expected to find the next Commit object or the end of the array of Commit objects.");
            }
        }

        // We expect memento to be copied in here so that we can update the value of the variable before we return the result
        private static (TMemento, long) ProcessEvents<TEventHandler>(Guid aggregateId, long commitSequenceNumber, long eventSequenceNumber, TMemento memento, in TEventHandler eventHandler, ref Utf8JsonStreamReader streamReader)
            where TEventHandler : IJsonEventHandler<TMemento>
        {
            // Advance from the start array to the start of the event
            streamReader.Read();

            while (streamReader.TokenType != JsonTokenType.EndArray)
            {
                if (streamReader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Your handler must read to the end of the event object.");
                }

                eventSequenceNumber += 1;
                memento = eventHandler.HandleSerializedEvent(ref streamReader, aggregateId, commitSequenceNumber, eventSequenceNumber, memento);

                if (streamReader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Your handler must read to the end of the event object.");
                }

                // Advance to the next event in the commit
                streamReader.Read();
            }

            return (memento, eventSequenceNumber);
        }

        /// <summary>
        /// This provides a simple serializer-based writer for your JSON payloads
        /// so generic, non-performance-sensitive clients can use the general purpose IEventStore/IAggregateRoot interfaces.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload.</typeparam>
        public readonly struct JsonSerializerPayloadWriter<TPayload> : IJsonEventPayloadWriter<TPayload>
        {
            private readonly JsonSerializerOptions options;

            /// <summary>
            /// Initializes a new instance of the <see cref="JsonSerializerPayloadWriter{TPayload}"/> struct.
            /// </summary>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> for the serialization.</param>
            public JsonSerializerPayloadWriter(JsonSerializerOptions options)
            {
                this.options = options;
            }

            /// <inheritdoc/>
            public void Write(in TPayload payload, Utf8JsonWriter writer)
            {
                JsonSerializer.Serialize(writer, payload, typeof(TPayload), this.options);
            }
        }

        /// <summary>
        /// This wraps a regular event handler with a JsonPayloadSerializer
        /// It allows for the simpler but less efficient model offered by
        /// IEventStore/IAggregateRoot, rather than the model used by this IJsonAggregateRoot/IJsonEventStore
        /// It allocates extra strings for e.g. event types and RAW JsonElement data,
        /// and parses entire events into memory when that may not be
        /// necessary.
        /// </summary>
        public readonly struct JsonEventHandlerOverJsonSerializer
            : IJsonEventHandler<TMemento>
        {
            private readonly IEventHandler<TMemento> eventHandler;
            private readonly JsonSerializerOptions options;

            /// <summary>
            /// Initializes a new instance of the <see cref="JsonEventHandlerOverJsonSerializer"/> struct.
            /// </summary>
            /// <param name="eventHandler">The event handler.</param>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> for serialization.</param>
            public JsonEventHandlerOverJsonSerializer(IEventHandler<TMemento> eventHandler, JsonSerializerOptions options)
            {
                this.eventHandler = eventHandler;
                this.options = options;
            }

            /// <inheritdoc/>
            public TMemento HandleSerializedEvent(ref Utf8JsonStreamReader streamReader, Guid aggregateId, long commitSequenceNumber, long expectedEventSequenceNumber, in TMemento memento)
            {
                long sequenceNumber = JsonEventHandler.ReadEventSequenceNumber(ref streamReader);

                if (expectedEventSequenceNumber != sequenceNumber)
                {
                    throw new JsonException($"Unexpected event sequence number for aggregate {aggregateId} in commit {commitSequenceNumber}. Expected {expectedEventSequenceNumber} but was {sequenceNumber}.");
                }

                string eventType = ReadEventType(ref streamReader);

                JsonEventHandler.FindPayload(ref streamReader);

                var payloadReader = new JsonElementPayloadReader(streamReader.Deserialize<JsonElement>(this.options), this.options);

                // Read to the end object.
                streamReader.Read();

                if (streamReader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Unexpected token: Expected to find the end of the Event object.");
                }

                return this.eventHandler.HandleSerializedEvent(aggregateId, commitSequenceNumber, eventType, sequenceNumber, memento, payloadReader);
            }

            /// <inheritdoc/>
            public TMemento HandleEvent<TPayload>(Guid aggregateId, long commitSequenceNumber, JsonEncodedText eventType, long eventSequenceNumber, in TMemento memento, in TPayload payload)
            {
                // We pay for a conversion back to a string when going down this path
                return this.eventHandler.HandleEvent(aggregateId, commitSequenceNumber, eventType.ToString(), eventSequenceNumber, memento, payload);
            }

            /// <summary>
            /// Read the event type from the stream reader.
            /// </summary>
            /// <param name="streamReader">The stream reader from which to read the event type.</param>
            /// <returns>The event type as a string.</returns>
            private static string ReadEventType(ref Utf8JsonStreamReader streamReader)
            {
                JsonEventHandler.FindEventType(ref streamReader);

                return streamReader.GetString();
            }
        }

        private class JsonElementPayloadReader : IPayloadReader
        {
            private readonly JsonElement jsonElement;
            private readonly JsonSerializerOptions options;

            public JsonElementPayloadReader(JsonElement payload, JsonSerializerOptions options)
            {
                this.jsonElement = payload;
                this.options = options;
            }

            public TPayload Read<TPayload>()
            {
                if (typeof(TPayload) == typeof(JsonElement))
                {
                    return CastTo<TPayload>.From(this.jsonElement);
                }

                return JsonSerializer.Deserialize<TPayload>(this.jsonElement.GetRawText(), this.options);
            }
        }
    }
}
