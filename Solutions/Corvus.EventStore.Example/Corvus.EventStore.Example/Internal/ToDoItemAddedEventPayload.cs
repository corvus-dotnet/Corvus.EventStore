// <copyright file="ToDoItemAddedEventPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example.Internal
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Corvus.EventStore.Serialization.Json.Converters;

    /// <summary>
    /// An event payload for when a to do item is added to a todolist.
    /// </summary>
    [JsonConverter(typeof(Converter))]
    internal readonly struct ToDoItemAddedEventPayload
    {
        /// <summary>
        /// The unique event type of this event.
        /// </summary>
        public const string EventType = "corvus.event-store-example.to-do-item-added";

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemAddedEventPayload"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ToDoItemId"/>.</param>
        /// <param name="title">The <see cref="Title"/>.</param>
        /// <param name="description">The <see cref="Description"/>.</param>
        public ToDoItemAddedEventPayload(Guid id, string title, string description)
        {
            this.ToDoItemId = id;
            this.Title = title;
            this.Description = description;
        }

        /// <summary>
        /// Gets the to do item ID.
        /// </summary>
        public Guid ToDoItemId { get; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; }

        private class Converter : JsonConverter<ToDoItemAddedEventPayload>
        {
            private readonly JsonEncodedText toDoItemIdName = JsonEncodedText.Encode("ToDoItemId");
            private readonly JsonEncodedText titleName = JsonEncodedText.Encode("Title");
            private readonly JsonEncodedText descriptionName = JsonEncodedText.Encode("Description");

            /// <inheritdoc/>
            public override ToDoItemAddedEventPayload Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                (Guid toDoItemId, string title, string description) = (default, string.Empty, string.Empty);

                // Read each of the three properties.
                (toDoItemId, title, description) = this.ReadProperty(ref reader, options, (toDoItemId, title, description));
                (toDoItemId, title, description) = this.ReadProperty(ref reader, options, (toDoItemId, title, description));
                (toDoItemId, title, description) = this.ReadProperty(ref reader, options, (toDoItemId, title, description));

                reader.Read();

                if (reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException();
                }

                return new ToDoItemAddedEventPayload(toDoItemId, title, description);
            }

            /// <inheritdoc/>
            public override void Write(
                Utf8JsonWriter writer,
                ToDoItemAddedEventPayload payload,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                ConverterHelpers.WriteProperty(writer, this.toDoItemIdName, payload.ToDoItemId, options);
                ConverterHelpers.WriteProperty(writer, this.titleName, payload.Title, options);
                ConverterHelpers.WriteProperty(writer, this.descriptionName, payload.Description, options);
                writer.WriteEndObject();
            }

            private (Guid toDoItemId, string title, string description) ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options, (Guid toDoItemId, string title, string description) result)
            {
                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                if (reader.ValueTextEquals(this.toDoItemIdName.EncodedUtf8Bytes))
                {
                    return (ConverterHelpers.ReadProperty<Guid>(ref reader, options), result.title, result.description);
                }
                else if (reader.ValueTextEquals(this.titleName.EncodedUtf8Bytes))
                {
                    return (result.toDoItemId, ConverterHelpers.ReadProperty<string>(ref reader, options), result.description);
                }
                else if (reader.ValueTextEquals(this.descriptionName.EncodedUtf8Bytes))
                {
                    return (result.toDoItemId, result.title, ConverterHelpers.ReadProperty<string>(ref reader, options));
                }
                else
                {
                    throw new JsonException();
                }
            }
        }
    }
}
