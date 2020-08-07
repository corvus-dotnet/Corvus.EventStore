// <copyright file="ToDoItemMemento.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example.Internal
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Corvus.EventStore.Serialization.Json.Converters;

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

                (Guid id, string title, string description) = (default, string.Empty, string.Empty);

                // Read each of the three properties.
                (id, title, description) = this.ReadProperty(ref reader, options, (id, title, description));
                (id, title, description) = this.ReadProperty(ref reader, options, (id, title, description));
                (id, title, description) = this.ReadProperty(ref reader, options, (id, title, description));

                reader.Read();

                if (reader.TokenType != JsonTokenType.EndObject)
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
                ConverterHelpers.WriteProperty(writer, this.idName, payload.Id, options);
                ConverterHelpers.WriteProperty(writer, this.titleName, payload.Title, options);
                ConverterHelpers.WriteProperty(writer, this.descriptionName, payload.Description, options);
                writer.WriteEndObject();
            }

            private (Guid id, string title, string description) ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options, (Guid id, string title, string description) result)
            {
                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                if (reader.ValueTextEquals(this.idName.EncodedUtf8Bytes))
                {
                    return (ConverterHelpers.ReadProperty<Guid>(ref reader, options), result.title, result.description);
                }
                else if (reader.ValueTextEquals(this.titleName.EncodedUtf8Bytes))
                {
                    return (result.id, ConverterHelpers.ReadProperty<string>(ref reader, options), result.description);
                }
                else if (reader.ValueTextEquals(this.descriptionName.EncodedUtf8Bytes))
                {
                    return (result.id, result.title, ConverterHelpers.ReadProperty<string>(ref reader, options));
                }
                else
                {
                    throw new JsonException();
                }
            }
        }
    }
}
