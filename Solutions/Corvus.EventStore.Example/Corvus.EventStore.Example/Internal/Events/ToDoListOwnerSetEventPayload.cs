// <copyright file="ToDoListOwnerSetEventPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example.Internal.Events
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Corvus.EventStore.Serialization.Json.Converters;

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

                string owner = this.ReadProperty(ref reader, options);

                reader.Read();

                if (reader.TokenType != JsonTokenType.EndObject)
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
                ConverterHelpers.WriteProperty(writer, this.ownerName, payload.Owner, options);
                writer.WriteEndObject();
            }

            private string ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                if (reader.ValueTextEquals(this.ownerName.EncodedUtf8Bytes))
                {
                    return ConverterHelpers.ReadProperty<string>(ref reader, options);
                }
                else
                {
                    throw new JsonException();
                }
            }
        }
    }
}
