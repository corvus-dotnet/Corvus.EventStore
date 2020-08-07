// <copyright file="ToDoListMemento.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example.Internal.Mementos
{
    using System;
    using System.Collections.Immutable;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Corvus.EventStore.Example.Internal.Events;
    using Corvus.EventStore.Serialization.Json.Converters;

    /// <summary>
    /// A memento for the current state of a TodoList object.
    /// </summary>
    [JsonConverter(typeof(Converter))]
    internal readonly struct ToDoListMemento
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoListMemento"/> struct.
        /// </summary>
        /// <param name="itemIds">The <see cref="ItemIds"/>.</param>
        /// <param name="owner">The <see cref="Owner"/>.</param>
        /// <param name="startDate">The <see cref="StartDate"/>.</param>
        public ToDoListMemento(ImmutableArray<Guid> itemIds, string owner, DateTimeOffset startDate)
        {
            this.ItemIds = itemIds;
            this.Owner = owner;
            this.StartDate = startDate;
        }

        /// <summary>
        /// Gets the array of IDs of the to-do items currently in the list.
        /// </summary>
        /// <remarks>This illustrates that the memento only needs enough state for the domain logic to do its job.</remarks>
        public ImmutableArray<Guid> ItemIds { get; }

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
            return new ToDoListMemento(this.GetOrCreateItems().Add(payload.ToDoItemId), this.Owner, this.StartDate);
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

        private ImmutableArray<Guid> GetOrCreateItems()
        {
            return this.ItemIds.IsDefault ? ImmutableArray<Guid>.Empty : this.ItemIds;
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

                (ImmutableArray<Guid> items, string owner, DateTimeOffset startDate) =
                    (ImmutableArray<Guid>.Empty, string.Empty, default);

                (items, owner, startDate) = this.ReadProperty(ref reader, options, (items, owner, startDate));
                (items, owner, startDate) = this.ReadProperty(ref reader, options, (items, owner, startDate));
                (items, owner, startDate) = this.ReadProperty(ref reader, options, (items, owner, startDate));

                reader.Read();

                if (reader.TokenType != JsonTokenType.EndObject)
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
                ConverterHelpers.WriteProperty(writer, this.startDateName, memento.StartDate, options);
                ConverterHelpers.WriteProperty(writer, this.ownerName, memento.Owner, options);
                ConverterHelpers.WriteProperty(writer, this.itemsName, memento.ItemIds, options);
                writer.WriteEndObject();
            }

            private (ImmutableArray<Guid> items, string owner, DateTimeOffset startDate) ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions options, (ImmutableArray<Guid> items, string owner, DateTimeOffset startDate) result)
            {
                reader.Read();
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                if (reader.ValueTextEquals(this.itemsName.EncodedUtf8Bytes))
                {
                    return (ConverterHelpers.ReadProperty<ImmutableArray<Guid>>(ref reader, options), result.owner, result.startDate);
                }
                else if (reader.ValueTextEquals(this.ownerName.EncodedUtf8Bytes))
                {
                    return (result.items, ConverterHelpers.ReadProperty<string>(ref reader, options), result.startDate);
                }
                else if (reader.ValueTextEquals(this.startDateName.EncodedUtf8Bytes))
                {
                    return (result.items, result.owner, ConverterHelpers.ReadProperty<DateTimeOffset>(ref reader, options));
                }
                else
                {
                    throw new JsonException();
                }
            }
        }
    }
}
