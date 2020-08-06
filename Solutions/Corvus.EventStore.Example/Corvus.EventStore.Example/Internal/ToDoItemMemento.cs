// <copyright file="ToDoItemMemento.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example.Internal
{
    using System;
    using System.Diagnostics;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// An item in the list maintained by the <see cref="ToDoListMemento"/>.
    /// </summary>
    [JsonConverter(typeof(Converter))]
    internal readonly struct ToDoItemMemento
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemMemento"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="Id"/>.</param>
        /// <param name="title">The <see cref="Title"/>.</param>
        /// <param name="description">The <see cref="Description"/>.</param>
        public ToDoItemMemento(Guid id, string title, string description)
        {
            this.Id = id;
            this.Title = title;
            this.Description = description;
        }

        /// <summary>
        /// Gets the ID of the to do item.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the title of the to do item.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the description of the to do item.
        /// </summary>
        public string Description { get; }

        private class Converter : JsonConverter<ToDoItemMemento>
        {
            private readonly JsonEncodedText idName = JsonEncodedText.Encode("Id");
            private readonly JsonEncodedText titleName = JsonEncodedText.Encode("Title");
            private readonly JsonEncodedText descriptionName = JsonEncodedText.Encode("Description");

            /// <inheritdoc/>
            public override ToDoItemMemento Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                Guid id = default;
                bool idSet = false;

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

                if (reader.ValueTextEquals(this.idName.EncodedUtf8Bytes))
                {
                    id = this.ReadGuidProperty(ref reader, options);
                    idSet = true;
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

                if (reader.ValueTextEquals(this.idName.EncodedUtf8Bytes))
                {
                    id = this.ReadGuidProperty(ref reader, options);
                    idSet = true;
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

                if (reader.ValueTextEquals(this.idName.EncodedUtf8Bytes))
                {
                    id = this.ReadGuidProperty(ref reader, options);
                    idSet = true;
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

                if (!(idSet && titleSet && descriptionSet))
                {
                    throw new JsonException();
                }

                return new ToDoItemMemento(id, title, description);
            }

            /// <inheritdoc/>
            public override void Write(
                Utf8JsonWriter writer,
                ToDoItemMemento payload,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                this.WriteGuidProperty(writer, this.idName, payload.Id, options);
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
