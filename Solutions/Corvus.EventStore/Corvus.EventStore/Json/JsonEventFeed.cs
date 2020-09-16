// <copyright file="JsonEventFeed.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Json
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Corvus.Extensions;

    /// <summary>
    /// A Json Event Feed that processes commits in an event stream.
    /// </summary>
    public static class JsonEventFeed
    {
        /// <summary>
        /// Process an array of commits.
        /// </summary>
        /// <typeparam name="TEventHandler">The type of the event handler.</typeparam>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="streamReader">The stream reader.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static void ProcessCommits<TEventHandler>(in TEventHandler eventHandler, ref Utf8JsonStreamReader streamReader, CancellationToken cancellationToken)
            where TEventHandler : IJsonEventFeedHandler
        {
            // Advance from the start array to the start of the object
            streamReader.Read();

            while (streamReader.TokenType != JsonTokenType.EndArray && !cancellationToken.IsCancellationRequested)
            {
                ProcessCommit(eventHandler, ref streamReader, cancellationToken);
                FindNextCommit(ref streamReader);
            }
        }

        /// <summary>
        /// Process a single commit.
        /// </summary>
        /// <typeparam name="TEventHandler">The type of the event handler.</typeparam>
        /// <param name="eventHandler">The event handler.</param>
        /// <param name="streamReader">The stream reader.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static void ProcessCommit<TEventHandler>(in TEventHandler eventHandler, ref Utf8JsonStreamReader streamReader, CancellationToken cancellationToken)
            where TEventHandler : IJsonEventFeedHandler
        {
            (Guid aggregateId, long commitSequenceNumber) = ValidateCommitAndFindEvents(ref streamReader);
            ProcessEvents(aggregateId, commitSequenceNumber, eventHandler, ref streamReader, cancellationToken);
            eventHandler.HandleCommitComplete(aggregateId, commitSequenceNumber);
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

        /// <summary>
        /// Process the events in the commit.
        /// </summary>
        /// <typeparam name="TEventHandler">The type of the event handler.</typeparam>
        /// <param name="aggregateId">The aggregate ID to which the commit was applied.</param>
        /// <param name="commitSequenceNumber">The commit sequence number.</param>
        /// <param name="eventHandler">The event handler to process the event.</param>
        /// <param name="streamReader">The stream reader from which to read the events.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private static void ProcessEvents<TEventHandler>(Guid aggregateId, long commitSequenceNumber, in TEventHandler eventHandler, ref Utf8JsonStreamReader streamReader, CancellationToken cancellationToken)
            where TEventHandler : IJsonEventFeedHandler
        {
            // Advance from the start array to the start of the event
            streamReader.Read();

            while (streamReader.TokenType != JsonTokenType.EndArray && !cancellationToken.IsCancellationRequested)
            {
                if (streamReader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Your handler must read to the end of the event object.");
                }

                eventHandler.HandleSerializedEvent(ref streamReader, aggregateId, commitSequenceNumber);

                if (streamReader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Your handler must read to the end of the event object.");
                }

                // Advance to the next event in the commit
                streamReader.Read();
            }
        }

        /// <summary>
        /// Validate a commit and find the array of Events.
        /// </summary>
        /// <param name="streamReader">The stream reader in which to find the commit events.</param>
        /// <remarks>
        /// This leaves the reader at the start of the events array.
        /// </remarks>
        private static (Guid aggregateId, long commitSequenceNumber) ValidateCommitAndFindEvents(ref Utf8JsonStreamReader streamReader)
        {
            Guid aggregateId;
            long commitSequenceNumber;

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
            if (streamReader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Expected the aggregate ID to be a string-encoded GUID");
            }

            aggregateId = streamReader.GetGuid();

            // Now, read the commit sequence number
            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.PropertyName || !streamReader.Match(JsonCommit.CommitSequenceNumberPropertyNameString))
            {
                throw new JsonException($"Expected to find the {JsonCommit.CommitSequenceNumberPropertyNameString} property.");
            }

            // Now read the value of commit sequence number and validate it
            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.Number)
            {
                throw new JsonException("Expected to find the commit sequence number as a number.");
            }

            commitSequenceNumber = streamReader.GetInt64();

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

            return (aggregateId, commitSequenceNumber);
        }

        /// <summary>
        /// This wraps a regular event feed handler with a JsonPayloadSerializer
        /// It allows for the simpler but less efficient model offered by
        /// IEventFeedHandler, rather than the model used by this IJsonEventFeedHandler
        /// It allocates extra strings for e.g. event types and RAW JsonElement data,
        /// and parses entire events into memory when that may not be
        /// necessary.
        /// </summary>
        public readonly struct JsonEventFeedHandlerOverJsonSerializer
            : IJsonEventFeedHandler
        {
            private readonly IEventFeedHandler eventHandler;
            private readonly JsonSerializerOptions options;

            /// <summary>
            /// Initializes a new instance of the <see cref="JsonEventFeedHandlerOverJsonSerializer"/> struct.
            /// </summary>
            /// <param name="eventHandler">The event handler.</param>
            /// <param name="options">The <see cref="JsonSerializerOptions"/> for serialization.</param>
            public JsonEventFeedHandlerOverJsonSerializer(IEventFeedHandler eventHandler, JsonSerializerOptions options)
            {
                this.eventHandler = eventHandler;
                this.options = options;
            }

            /// <inheritdoc/>
            public void HandleSerializedEvent(ref Utf8JsonStreamReader streamReader, Guid aggregateId, long commitSequenceNumber)
            {
                long sequenceNumber = JsonEventHandler.ReadEventSequenceNumber(ref streamReader);

                string eventType = ReadEventType(ref streamReader);

                JsonEventHandler.FindPayload(ref streamReader);

                var payloadReader = new JsonElementPayloadReader(streamReader.Deserialize<JsonElement>(this.options), this.options);

                // Read to the end object.
                streamReader.Read();

                if (streamReader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Unexpected token: Expected to find the end of the Event object.");
                }

                this.eventHandler.HandleSerializedEvent(aggregateId, commitSequenceNumber, eventType, sequenceNumber, payloadReader);
            }

            /// <inheritdoc/>
            public Task HandleBatchComplete(string checkpoint)
            {
                return this.eventHandler.HandleBatchComplete(checkpoint);
            }

            /// <inheritdoc/>
            public void HandleCommitComplete(Guid aggregateId, long commitSequenceNumber)
            {
                this.eventHandler.HandleCommitComplete(aggregateId, commitSequenceNumber);
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

        private readonly struct JsonElementPayloadReader : IPayloadReader
        {
            private readonly JsonElement jsonElement;
            private readonly JsonSerializerOptions options;

            public JsonElementPayloadReader(JsonElement payload, JsonSerializerOptions options)
                : this()
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
