// <copyright file="ToDoItemRemovedEventJsonPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox.Events
{
    using System;
    using System.Text.Json;
    using Corvus.EventStore.Json;

    /// <summary>
    /// An event payload for when a to do item is removed from a todolist.
    /// </summary>
    internal readonly struct ToDoItemRemovedEventJsonPayload
    {
        /// <summary>
        /// The unique event type.
        /// </summary>
        public const string EventType = "corvus.event-store-example.to-do-item-removed";

        /// <summary>
        /// The unique event type of this event, encoded as a json string.
        /// </summary>
        public static readonly JsonEncodedText EncodedEventType = JsonEncodedText.Encode(EventType);

        /// <summary>
        /// The reader/writer for this event type.
        /// </summary>
        public static readonly ReaderWriter Converter = default;

        private const string IdPropertyString = "id";

        private static readonly JsonEncodedText IdProperty = JsonEncodedText.Encode(IdPropertyString);

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemRemovedEventPayload"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ToDoItemId"/> of the item that was removed.</param>
        public ToDoItemRemovedEventJsonPayload(Guid id)
        {
            this.ToDoItemId = id;
        }

        /// <summary>
        /// Gets the id of the item that was removed.
        /// </summary>
        public Guid ToDoItemId { get; }

        /// <summary>
        /// The internal reader/writer for the payload.
        /// </summary>
        internal readonly struct ReaderWriter : IJsonEventPayloadWriter<ToDoItemRemovedEventJsonPayload>
        {
            /// <inheritdoc/>
            public void Write(in ToDoItemRemovedEventJsonPayload payload, Utf8JsonWriter writer)
            {
                writer.WriteStartObject();
                writer.WriteString(IdProperty, payload.ToDoItemId);
                writer.WriteEndObject();
            }

            /// <summary>
            /// Read the ToDoItemId from the stream and consume to the end of the payload.
            /// </summary>
            /// <param name="streamReader">The reader from which to read the id.</param>
            /// <returns>The ID read from the payload.</returns>
            public Guid ReadToDoItemId(ref Utf8JsonStreamReader streamReader)
            {
                if (streamReader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected to be at the start of the event payload.");
                }

                // Read the ID
                streamReader.Read();
                if (streamReader.TokenType != JsonTokenType.PropertyName || !streamReader.Match(IdPropertyString))
                {
                    throw new JsonException($"Expected to find the {IdPropertyString}.");
                }

                streamReader.Read();
                if (streamReader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException($"Expected to find a string-encoded GUID property.");
                }

                Guid id = streamReader.GetGuid();

                // Read to the end of the object
                streamReader.Read();

                if (streamReader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Expected to be at the end of the event payload.");
                }

                return id;
            }
        }
    }
}
