// <copyright file="ToDoListOwnerSetEventJsonPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox.Events
{
    using System.Text.Json;
    using Corvus.EventStore.Json;

    /// <summary>
    /// An event payload for when the owner of the todo list is set.
    /// </summary>
    internal readonly struct ToDoListOwnerSetEventJsonPayload
    {
        /// <summary>
        /// The unique event type of this event.
        /// </summary>
        public const string EventType = "corvus.event-store-example.to-do-list-owner-set";

        /// <summary>
        /// The unique event type of this event, encoded as a json string.
        /// </summary>
        public static readonly JsonEncodedText EncodedEventType = JsonEncodedText.Encode(EventType);

        /// <summary>
        /// The reader/writer for this event type.
        /// </summary>
        public static readonly ReaderWriter Converter = default;

        private const string OwnerPropertyString = "owner";

        private static readonly JsonEncodedText OwnerProperty = JsonEncodedText.Encode(OwnerPropertyString);

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemAddedEventPayload"/> struct.
        /// </summary>
        /// <param name="owner">The <see cref="Owner"/>.</param>
        public ToDoListOwnerSetEventJsonPayload(string owner)
        {
            this.Owner = owner;
        }

        /// <summary>
        /// Gets the owner.
        /// </summary>
        public string Owner { get; }

        /// <summary>
        /// The internal reader/writer for the payload.
        /// </summary>
        internal readonly struct ReaderWriter : IJsonEventPayloadWriter<ToDoListOwnerSetEventJsonPayload>
        {
            /// <inheritdoc/>
            public void Write(in ToDoListOwnerSetEventJsonPayload payload, Utf8JsonWriter writer)
            {
                writer.WriteStartObject();
                writer.WriteString(OwnerProperty, payload.Owner);
                writer.WriteEndObject();
            }

            /// <summary>
            /// Read the owner from the stream and consume to the end of the payload.
            /// </summary>
            /// <param name="streamReader">The reader from which to read the id.</param>
            /// <returns>The ID read from the payload.</returns>
            public string ReadOwner(ref Utf8JsonStreamReader streamReader)
            {
                if (streamReader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected to be at the start of the event payload.");
                }

                // Read the ID
                streamReader.Read();
                if (streamReader.TokenType != JsonTokenType.PropertyName || !streamReader.Match(OwnerPropertyString))
                {
                    throw new JsonException($"Expected to find the {OwnerPropertyString}.");
                }

                streamReader.Read();
                if (streamReader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException($"Expected to find a string property.");
                }

                string owner = streamReader.GetString();

                // Read to the end of the object
                streamReader.Read();

                if (streamReader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException("Expected to be at the end of the event payload.");
                }

                return owner;
            }
        }
    }
}
