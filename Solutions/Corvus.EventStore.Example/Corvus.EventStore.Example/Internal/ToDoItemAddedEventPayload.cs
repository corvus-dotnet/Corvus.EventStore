// <copyright file="ToDoItemAddedEventPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example.Internal
{
    using System;
    using System.Diagnostics;
    using System.Text.Json;
    using System.Text.Json.Serialization;

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

                Guid toDoItemId = default;
                bool toDoItemIdSet = false;

                string title = string.Empty;
                bool titleSet = false;

                string description = string.Empty;
                bool descriptionSet = false;

                // Get the first property.
                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                if (reader.ValueTextEquals(this.toDoItemIdName.EncodedUtf8Bytes))
                {
                    toDoItemId = this.ReadGuidProperty(ref reader, options);
                    toDoItemIdSet = true;
                }
                else if (reader.ValueTextEquals(this.titleName.EncodedUtf8Bytes))
                {
                    title = this.ReadStringProperty(ref reader, options);
                    titleSet = true;
                }
                else if (reader.ValueTextEquals(this.descriptionName.EncodedUtf8Bytes))
                {
                    description = this.ReadStringProperty(ref reader, options);
                    descriptionSet = true;
                }
                else
                {
                    throw new JsonException();
                }

                // Get the second property.
                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                if (reader.ValueTextEquals(this.toDoItemIdName.EncodedUtf8Bytes))
                {
                    toDoItemId = this.ReadGuidProperty(ref reader, options);
                    toDoItemIdSet = true;
                }
                else if (reader.ValueTextEquals(this.titleName.EncodedUtf8Bytes))
                {
                    title = this.ReadStringProperty(ref reader, options);
                    titleSet = true;
                }
                else if (reader.ValueTextEquals(this.descriptionName.EncodedUtf8Bytes))
                {
                    description = this.ReadStringProperty(ref reader, options);
                    descriptionSet = true;
                }
                else
                {
                    throw new JsonException();
                }

                // Get the third property.
                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                if (reader.ValueTextEquals(this.toDoItemIdName.EncodedUtf8Bytes))
                {
                    toDoItemId = this.ReadGuidProperty(ref reader, options);
                    toDoItemIdSet = true;
                }
                else if (reader.ValueTextEquals(this.titleName.EncodedUtf8Bytes))
                {
                    title = this.ReadStringProperty(ref reader, options);
                    titleSet = true;
                }
                else if (reader.ValueTextEquals(this.descriptionName.EncodedUtf8Bytes))
                {
                    description = this.ReadStringProperty(ref reader, options);
                    descriptionSet = true;
                }
                else
                {
                    throw new JsonException();
                }

                reader.Read();

                if (reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException();
                }

                if (!(toDoItemIdSet && titleSet && descriptionSet))
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
                this.WriteGuidProperty(writer, this.toDoItemIdName, payload.ToDoItemId, options);
                this.WriteStringProperty(writer, this.titleName, payload.Title, options);
                this.WriteStringProperty(writer, this.descriptionName, payload.Description, options);
                writer.WriteEndObject();
            }

            private Guid ReadGuidProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                Debug.Assert(reader.TokenType == JsonTokenType.PropertyName, "Unexpected token type while trying to read a Guid property.");

                if (!(options?.GetConverter(typeof(Guid)) is JsonConverter<Guid> guidConverter))
                {
                    throw new InvalidOperationException();
                }

                reader.Read();
                return guidConverter.Read(ref reader, typeof(Guid), options);
            }

            private void WriteGuidProperty(Utf8JsonWriter writer, JsonEncodedText name, Guid guidValue, JsonSerializerOptions options)
            {
                if (!(options?.GetConverter(typeof(Guid)) is JsonConverter<Guid> guidConverter))
                {
                    throw new InvalidOperationException();
                }

                writer.WritePropertyName(name);
                guidConverter.Write(writer, guidValue, options);
            }

            private string ReadStringProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                Debug.Assert(reader.TokenType == JsonTokenType.PropertyName, "Unexpected token type while trying to read a string property.");

                if (!(options?.GetConverter(typeof(string)) is JsonConverter<string> stringConverter))
                {
                    throw new InvalidOperationException();
                }

                reader.Read();
                return stringConverter.Read(ref reader, typeof(string), options);
            }

            private void WriteStringProperty(Utf8JsonWriter writer, JsonEncodedText name, string stringValue, JsonSerializerOptions options)
            {
                if (!(options?.GetConverter(typeof(string)) is JsonConverter<string> stringConverter))
                {
                    throw new InvalidOperationException();
                }

                writer.WritePropertyName(name);
                stringConverter.Write(writer, stringValue, options);
            }
        }
    }
}
