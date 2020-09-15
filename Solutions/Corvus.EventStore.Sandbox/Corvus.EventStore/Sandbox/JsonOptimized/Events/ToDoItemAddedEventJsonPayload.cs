// <copyright file="ToDoItemAddedEventJsonPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox.Events
{
    using System;
    using System.Text.Json;
    using Corvus.EventStore.Json;

    /// <summary>
    /// An event payload for when a to do item is added to a todolist.
    /// </summary>
    internal readonly struct ToDoItemAddedEventJsonPayload
    {
        /// <summary>
        /// The unique event type of this event.
        /// </summary>
        public const string EventType = "corvus.event-store-example.to-do-item-added";

        /// <summary>
        /// The unique event type of this event, encoded as a json string.
        /// </summary>
        public static readonly JsonEncodedText EncodedEventType = JsonEncodedText.Encode(EventType);

        /// <summary>
        /// The reader/writer for this event type.
        /// </summary>
        public static readonly ReaderWriter Converter = default;

        private const string IdPropertyString = "id";
        private const string TitlePropertyString = "title";
        private const string DescriptionPropertyString = "description";

        private static readonly JsonEncodedText IdProperty = JsonEncodedText.Encode(IdPropertyString);
        private static readonly JsonEncodedText TitleProperty = JsonEncodedText.Encode(TitlePropertyString);
        private static readonly JsonEncodedText DescriptionProperty = JsonEncodedText.Encode(DescriptionPropertyString);

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemAddedEventPayload"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="Id"/>.</param>
        /// <param name="title">The <see cref="Title"/>.</param>
        /// <param name="description">The <see cref="Description"/>.</param>
        public ToDoItemAddedEventJsonPayload(Guid id, string title, string description)
        {
            this.Id = id;
            this.Title = title;
            this.Description = description;
        }

        /// <summary>
        /// Gets the to do item ID.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The internal reader/writer for the payload.
        /// </summary>
        internal readonly struct ReaderWriter : IJsonEventPayloadWriter<ToDoItemAddedEventJsonPayload>
        {
            /// <inheritdoc/>
            public void Write(ToDoItemAddedEventJsonPayload payload, Utf8JsonWriter writer)
            {
                writer.WriteStartObject();
                writer.WriteString(IdProperty, payload.Id);
                writer.WriteString(TitleProperty, payload.Title);
                writer.WriteString(DescriptionProperty, payload.Description);
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

                // Skip the Title
                streamReader.Read();
                streamReader.Read();

                // Skip the Description
                streamReader.Read();
                streamReader.Read();

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
