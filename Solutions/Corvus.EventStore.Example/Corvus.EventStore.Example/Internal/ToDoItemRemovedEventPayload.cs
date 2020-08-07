// <copyright file="ToDoItemRemovedEventPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example.Internal
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Corvus.EventStore.Serialization.Json.Converters;

    /// <summary>
    /// An event payload for when a to do item is removed from a todolist.
    /// </summary>
    [JsonConverter(typeof(Converter))]
    internal readonly struct ToDoItemRemovedEventPayload
    {
        /// <summary>
        /// The unique event type.
        /// </summary>
        public const string EventType = "corvus.event-store-example.to-do-item-removed";

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemRemovedEventPayload"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ToDoItemId"/> of the item that was removed.</param>
        public ToDoItemRemovedEventPayload(Guid id)
        {
            this.ToDoItemId = id;
        }

        /// <summary>
        /// Gets the id of the item that was removed.
        /// </summary>
        public Guid ToDoItemId { get; }

        private class Converter : JsonConverter<ToDoItemRemovedEventPayload>
        {
            private readonly JsonEncodedText toDoItemIdName = JsonEncodedText.Encode("ToDoItemId");

            /// <inheritdoc/>
            public override ToDoItemRemovedEventPayload Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                Guid toDoItemId = this.ReadProperty(ref reader, options);

                reader.Read();

                if (reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException();
                }

                return new ToDoItemRemovedEventPayload(toDoItemId);
            }

            /// <inheritdoc/>
            public override void Write(
                Utf8JsonWriter writer,
                ToDoItemRemovedEventPayload payload,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                ConverterHelpers.WriteProperty(writer, this.toDoItemIdName, payload.ToDoItemId, options);
                writer.WriteEndObject();
            }

            private Guid ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                reader.Read();

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                if (reader.ValueTextEquals(this.toDoItemIdName.EncodedUtf8Bytes))
                {
                    return ConverterHelpers.ReadProperty<Guid>(ref reader, options);
                }
                else
                {
                    throw new JsonException();
                }
            }
        }
    }
}
