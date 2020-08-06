// <copyright file="ToDoListMemento.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example.Internal
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A memento for the current state of a TodoList object.
    /// </summary>
    [JsonConverter(typeof(Converter))]
    internal readonly struct ToDoListMemento
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoListMemento"/> struct.
        /// </summary>
        /// <param name="items">The <see cref="Items"/>.</param>
        /// <param name="owner">The <see cref="Owner"/>.</param>
        /// <param name="startDate">The <see cref="StartDate"/>.</param>
        public ToDoListMemento(ImmutableDictionary<Guid, ToDoItemMemento> items, string owner, DateTimeOffset startDate)
        {
            this.Items = items;
            this.Owner = owner;
            this.StartDate = startDate;
        }

        /// <summary>
        /// Gets the array of to-do items currently in the list.
        /// </summary>
        public ImmutableDictionary<Guid, ToDoItemMemento> Items { get; }

        /// <summary>
        /// Gets the owner of the list.
        /// </summary>
        public string Owner { get; }

        /// <summary>
        /// Gets the start date for the list.
        /// </summary>
        /// <remarks>What's this for? Who knows - just an example property, really.</remarks>
        public DateTimeOffset StartDate { get; }

        /// <summary>
        /// Constructs a memento with the given item added to the list.
        /// </summary>
        /// <param name="payload">The event payload describing the item that was added.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the item added.</returns>
        public ToDoListMemento With(ToDoItemAddedEventPayload payload)
        {
            return new ToDoListMemento(this.GetOrCreateItems().Add(payload.ToDoItemId, new ToDoItemMemento(payload.ToDoItemId, payload.Title, payload.Description)), this.Owner, this.StartDate);
        }

        /// <summary>
        /// Constructs a memento with the given item removed from the list.
        /// </summary>
        /// <param name="payload">The event payload describing the item that was removed.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the item removed.</returns>
        public ToDoListMemento With(ToDoItemRemovedEventPayload payload)
        {
            return new ToDoListMemento(this.GetOrCreateItems().Remove(payload.ToDoItemId), this.Owner, this.StartDate);
        }

        /// <summary>
        /// Constructs a memento with the owner set.
        /// </summary>
        /// <param name="payload">The event payload describing owner that was set.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the owner set.</returns>
        public ToDoListMemento With(ToDoListOwnerSetEventPayload payload)
        {
            return new ToDoListMemento(this.GetOrCreateItems(), payload.Owner, this.StartDate);
        }

        /// <summary>
        /// Constructs a memento with the owner set.
        /// </summary>
        /// <param name="payload">The event payload describing owner that was set.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the owner set.</returns>
        public ToDoListMemento With(ToDoListStartDateSetEventPayload payload)
        {
            return new ToDoListMemento(this.GetOrCreateItems(), this.Owner, payload.StartDate);
        }

        private ImmutableDictionary<Guid, ToDoItemMemento> GetOrCreateItems()
        {
            return this.Items ?? ImmutableDictionary<Guid, ToDoItemMemento>.Empty;
        }

        private class Converter : JsonConverter<ToDoListMemento>
        {
            private readonly JsonEncodedText itemsName = JsonEncodedText.Encode("Items");
            private readonly JsonEncodedText ownerName = JsonEncodedText.Encode("Owner");
            private readonly JsonEncodedText startDateName = JsonEncodedText.Encode("StartDate");

            /// <inheritdoc/>
            public override ToDoListMemento Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                ImmutableDictionary<Guid, ToDoItemMemento> items = ImmutableDictionary<Guid, ToDoItemMemento>.Empty;
                bool itemsSet = false;

                string owner = string.Empty;
                bool ownerSet = false;

                DateTimeOffset startDate = default;
                bool startDateSet = false;

                // Get the first property.
                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                if (reader.ValueTextEquals(this.itemsName.EncodedUtf8Bytes))
                {
                    items = this.ReadImmutableDictionaryProperty(ref reader, options);
                    itemsSet = true;
                }
                else if (reader.ValueTextEquals(this.ownerName.EncodedUtf8Bytes))
                {
                    owner = this.ReadStringProperty(ref reader, options);
                    ownerSet = true;
                }
                else if (reader.ValueTextEquals(this.startDateName.EncodedUtf8Bytes))
                {
                    startDate = this.ReadDateTimeOffsetProperty(ref reader, options);
                    startDateSet = true;
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

                if (reader.ValueTextEquals(this.itemsName.EncodedUtf8Bytes))
                {
                    items = this.ReadImmutableDictionaryProperty(ref reader, options);
                    itemsSet = true;
                }
                else if (reader.ValueTextEquals(this.ownerName.EncodedUtf8Bytes))
                {
                    owner = this.ReadStringProperty(ref reader, options);
                    ownerSet = true;
                }
                else if (reader.ValueTextEquals(this.startDateName.EncodedUtf8Bytes))
                {
                    startDate = this.ReadDateTimeOffsetProperty(ref reader, options);
                    startDateSet = true;
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

                if (reader.ValueTextEquals(this.itemsName.EncodedUtf8Bytes))
                {
                    items = this.ReadImmutableDictionaryProperty(ref reader, options);
                    itemsSet = true;
                }
                else if (reader.ValueTextEquals(this.ownerName.EncodedUtf8Bytes))
                {
                    owner = this.ReadStringProperty(ref reader, options);
                    ownerSet = true;
                }
                else if (reader.ValueTextEquals(this.startDateName.EncodedUtf8Bytes))
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

                if (!(itemsSet && ownerSet && startDateSet))
                {
                    throw new JsonException();
                }

                return new ToDoListMemento(items, owner, startDate);
            }

            /// <inheritdoc/>
            public override void Write(
                Utf8JsonWriter writer,
                ToDoListMemento memento,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                this.WriteDateTimeOffsetProperty(writer, this.startDateName, memento.StartDate, options);
                this.WriteStringProperty(writer, this.ownerName, memento.Owner, options);
                this.WriteImmutableDictionaryProperty(writer, this.itemsName, memento.Items ?? ImmutableDictionary<Guid, ToDoItemMemento>.Empty, options);
                writer.WriteEndObject();
            }

            private DateTimeOffset ReadDateTimeOffsetProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                Debug.Assert(reader.TokenType == JsonTokenType.PropertyName, "Unexpected token type while trying to read a Guid property.");

                if (!(options?.GetConverter(typeof(DateTimeOffset)) is JsonConverter<DateTimeOffset> dateTimeOffsetConverter))
                {
                    throw new InvalidOperationException();
                }

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

            private ImmutableDictionary<Guid, ToDoItemMemento> ReadImmutableDictionaryProperty(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                Debug.Assert(reader.TokenType == JsonTokenType.PropertyName, "Unexpected token type while trying to read an ImmutableDictionary<Guid, ToDoItemMemento> property.");

                if (!(options?.GetConverter(typeof(ImmutableDictionary<Guid, ToDoItemMemento>)) is JsonConverter<ImmutableDictionary<Guid, ToDoItemMemento>> immutableDictionaryConverter))
                {
                    throw new InvalidOperationException();
                }

                reader.Read();
                return immutableDictionaryConverter.Read(ref reader, typeof(ImmutableDictionary<Guid, ToDoItemMemento>), options);
            }

            private void WriteImmutableDictionaryProperty(Utf8JsonWriter writer, JsonEncodedText name, ImmutableDictionary<Guid, ToDoItemMemento> immutableDictionaryValue, JsonSerializerOptions options)
            {
                if (!(options?.GetConverter(typeof(ImmutableDictionary<Guid, ToDoItemMemento>)) is JsonConverter<ImmutableDictionary<Guid, ToDoItemMemento>> immutableDictionaryConverter))
                {
                    throw new InvalidOperationException();
                }

                writer.WritePropertyName(name);
                immutableDictionaryConverter.Write(writer, immutableDictionaryValue, options);
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
