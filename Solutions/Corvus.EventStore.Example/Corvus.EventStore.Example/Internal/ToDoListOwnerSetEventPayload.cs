// <copyright file="ToDoListOwnerSetEventPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example.Internal
{
    using System;
    using System.Diagnostics;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// An event payload for when the owner of the todo list is set.
    /// </summary>
    [JsonConverter(typeof(Converter))]
    internal readonly struct ToDoListOwnerSetEventPayload
    {
        /// <summary>
        /// The unique event type of this event.
        /// </summary>
        public const string EventType = "corvus.event-store-example.to-do-list-owner-set";

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemAddedEventPayload"/> struct.
        /// </summary>
        /// <param name="owner">The <see cref="Owner"/>.</param>
        public ToDoListOwnerSetEventPayload(string owner)
        {
            this.Owner = owner;
        }

        /// <summary>
        /// Gets the owner.
        /// </summary>
        public string Owner { get; }

        private class Converter : JsonConverter<ToDoListOwnerSetEventPayload>
        {
            private readonly JsonEncodedText ownerName = JsonEncodedText.Encode("Owner");

            /// <inheritdoc/>
            public override ToDoListOwnerSetEventPayload Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                // Get the first property.
                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                string owner;
                bool ownerSet;

                if (reader.ValueTextEquals(this.ownerName.EncodedUtf8Bytes))
                {
                    owner = this.ReadStringProperty(ref reader, options);
                    ownerSet = true;
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

                if (!ownerSet)
                {
                    throw new JsonException();
                }

                return new ToDoListOwnerSetEventPayload(owner);
            }

            /// <inheritdoc/>
            public override void Write(
                Utf8JsonWriter writer,
                ToDoListOwnerSetEventPayload payload,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                this.WriteStringProperty(writer, this.ownerName, payload.Owner, options);
                writer.WriteEndObject();
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
