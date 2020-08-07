// <copyright file="ToDoListStartDateSetEventPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example.Internal
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Corvus.EventStore.Serialization.Json.Converters;

    /// <summary>
    /// An event payload for when the start date of the todo list is set.
    /// </summary>
    [JsonConverter(typeof(Converter))]
    internal readonly struct ToDoListStartDateSetEventPayload
    {
        /// <summary>
        /// The unique event type of this event.
        /// </summary>
        public const string EventType = "corvus.event-store-example.to-do-list-start-date-set";

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemAddedEventPayload"/> struct.
        /// </summary>
        /// <param name="startDate">The <see cref="StartDate"/>.</param>
        public ToDoListStartDateSetEventPayload(DateTimeOffset startDate)
        {
            this.StartDate = startDate;
        }

        /// <summary>
        /// Gets the owner.
        /// </summary>
        public DateTimeOffset StartDate { get; }

        /// <summary>
        /// The type converter.
        /// </summary>
        public class Converter : JsonConverter<ToDoListStartDateSetEventPayload>
        {
            private readonly JsonEncodedText startDateName = JsonEncodedText.Encode("StartDate");

            /// <inheritdoc/>
            public override ToDoListStartDateSetEventPayload Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                DateTimeOffset startDate = this.ReadProperty(ref reader, options);

                reader.Read();

                if (reader.TokenType != JsonTokenType.EndObject)
                {
                    throw new JsonException();
                }

                return new ToDoListStartDateSetEventPayload(startDate);
            }

            /// <inheritdoc/>
            public override void Write(
                Utf8JsonWriter writer,
                ToDoListStartDateSetEventPayload payload,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                ConverterHelpers.WriteProperty(writer, this.startDateName, payload.StartDate, options);
                writer.WriteEndObject();
            }

            private DateTimeOffset ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                if (reader.ValueTextEquals(this.startDateName.EncodedUtf8Bytes))
                {
                    return ConverterHelpers.ReadProperty<DateTimeOffset>(ref reader, options);
                }
                else
                {
                    throw new JsonException();
                }
            }
        }
    }
}
