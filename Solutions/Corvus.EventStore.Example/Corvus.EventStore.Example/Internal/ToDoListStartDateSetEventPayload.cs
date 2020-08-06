// <copyright file="ToDoListStartDateSetEventPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example.Internal
{
    using System;
    using System.Diagnostics;
    using System.Text.Json;
    using System.Text.Json.Serialization;

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

                // Get the first property.
                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                DateTimeOffset startDate;
                bool startDateSet;

                if (reader.ValueTextEquals(this.startDateName.EncodedUtf8Bytes))
                {
                    startDate = this.ReadDateTimeOffsetProperty(ref reader, options);
                    startDateSet = true;
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

                if (!startDateSet)
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
                this.WriteDateTimeOffsetProperty(writer, this.startDateName, payload.StartDate, options);
                writer.WriteEndObject();
            }

            private DateTimeOffset ReadDateTimeOffsetProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                if (!(options?.GetConverter(typeof(DateTimeOffset)) is JsonConverter<DateTimeOffset> dateTimeOffsetConverter))
                {
                    throw new InvalidOperationException();
                }

                Debug.Assert(reader.TokenType == JsonTokenType.PropertyName, "Unexpected token type while trying to read a string property.");

                reader.Read();
                return dateTimeOffsetConverter.Read(ref reader, typeof(DateTimeOffset), options);
            }

            private void WriteDateTimeOffsetProperty(Utf8JsonWriter writer, JsonEncodedText name, DateTimeOffset dateTimeOffsetValue, JsonSerializerOptions options)
            {
                if (!(options?.GetConverter(typeof(DateTimeOffset)) is JsonConverter<DateTimeOffset> dateTimeOffsetConverter))
                {
                    throw new InvalidOperationException();
                }

                writer.WritePropertyName(name);
                dateTimeOffsetConverter.Write(writer, dateTimeOffsetValue, options);
            }
        }
    }
}
