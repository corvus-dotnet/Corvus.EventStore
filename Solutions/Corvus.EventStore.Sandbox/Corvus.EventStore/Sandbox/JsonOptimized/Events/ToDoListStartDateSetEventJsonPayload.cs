// <copyright file="ToDoListStartDateSetEventJsonPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox.Events
{
    using System;
    using System.Text.Json;
    using Corvus.EventStore.Json;

    /// <summary>
    /// An event payload for when the start date of the todo list is set.
    /// </summary>
    /// <remarks>
    /// Note that our payloads implement <see cref="IJsonEventPayloadWriter{TPayload}"/> directly.
    /// You can if you wish move these out into a separate class and use <see cref="IJsonAggregateRoot{TMemento, T}.ApplyEvent{TPayload, TPayloadWriter, TEventHandler}(System.Text.Json.JsonEncodedText, TPayload, TPayloadWriter, TEventHandler)"/>
    /// in your aggregate implementation (and similarly for your internal read methods).
    /// </remarks>
    internal readonly struct ToDoListStartDateSetEventJsonPayload
    {
        /// <summary>
        /// The unique event type of this event.
        /// </summary>
        public const string EventType = "corvus.event-store-example.to-do-list-start-date-set";

        /// <summary>
        /// The unique event type of this event, encoded as a json string.
        /// </summary>
        public static readonly JsonEncodedText EncodedEventType = JsonEncodedText.Encode(EventType);

        /// <summary>
        /// The reader/writer for this event type.
        /// </summary>
        public static readonly ReaderWriter Converter = default;

        private const string StartDatePropertyString = "startDate";

        private static readonly JsonEncodedText StartDateProperty = JsonEncodedText.Encode(StartDatePropertyString);

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemAddedEventPayload"/> struct.
        /// </summary>
        /// <param name="startDate">The <see cref="StartDate"/>.</param>
        public ToDoListStartDateSetEventJsonPayload(DateTimeOffset startDate)
        {
            this.StartDate = startDate;
        }

        /// <summary>
        /// Gets the start date.
        /// </summary>
        public DateTimeOffset StartDate { get; }

        /// <summary>
        /// The internal reader/writer for the payload.
        /// </summary>
        internal readonly struct ReaderWriter : IJsonEventPayloadWriter<ToDoListStartDateSetEventJsonPayload>
        {
            /// <inheritdoc/>
            public void Write(ToDoListStartDateSetEventJsonPayload payload, Utf8JsonWriter writer)
            {
                writer.WriteStartObject();
                writer.WriteString(StartDateProperty, payload.StartDate);
                writer.WriteEndObject();
            }

            /// <summary>
            /// Read the start date from the stream and consume to the end of the payload.
            /// </summary>
            /// <param name="streamReader">The reader from which to read the start date.</param>
            /// <returns>The start date read from the payload.</returns>
            public DateTimeOffset ReadStartDate(ref Utf8JsonStreamReader streamReader)
            {
                if (streamReader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected to be at the start of the event payload.");
                }

                // Read the ID
                streamReader.Read();
                if (streamReader.TokenType != JsonTokenType.PropertyName || !streamReader.Match(StartDatePropertyString))
                {
                    throw new JsonException($"Expected to find the {StartDatePropertyString}.");
                }

                streamReader.Read();
                if (streamReader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException($"Expected to find a string-encoded DateTimeOffset property.");
                }

                DateTimeOffset startDate = streamReader.GetDateTimeOffset();

                // Read to the end of the object
                streamReader.Read();

                if (streamReader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Expected to be at the end of the event payload.");
                }

                return startDate;
            }
        }
    }
}
