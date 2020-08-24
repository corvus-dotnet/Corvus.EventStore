// <copyright file="Utf8JsonEventListSerializer.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core.Internal
{
    using System;
    using System.Collections.Immutable;
    using System.Text.Json;
    using Corvus.EventStore.Core;

    /// <summary>
    /// Serialization helpers for an event list.
    /// </summary>
    internal static class Utf8JsonEventListSerializer
    {
        /// <summary>
        /// Gets or sets the <see cref="JsonSerializerOptions"/>.
        /// </summary>
        public static JsonSerializerOptions Options { get; set; } = GetDefaultOptions();

        /// <summary>
        /// Deserialize an event list.
        /// </summary>
        /// <param name="events">The Utf8 encoded binary JSON representation of the list.</param>
        /// <returns>An <see cref="ImmutableArray{T}"/> of <see cref="SerializedEvent"/>.</returns>
        public static ImmutableArray<SerializedEvent> DeserializeEventList(ReadOnlySpan<byte> events)
        {
            return JsonSerializer.Deserialize<ImmutableArray<SerializedEvent>>(events, Options);
        }

        /// <summary>
        /// Deserialize an event list.
        /// </summary>
        /// <param name="events">The Utf8 encoded binary JSON representation of the list.</param>
        /// <returns>An <see cref="ImmutableArray{T}"/> of <see cref="SerializedEvent"/>.</returns>
        public static ImmutableArray<SerializedEvent> DeserializeEventList(string events)
        {
            return JsonSerializer.Deserialize<ImmutableArray<SerializedEvent>>(events, Options);
        }

        /// <summary>
        /// Serializes the event list.
        /// </summary>
        /// <param name="events">The events to serialize.</param>
        /// <returns>The Utf8 binary encoded Json representation of the list.</returns>
        public static byte[] SerializeEventList(ImmutableArray<SerializedEvent> events)
        {
            return JsonSerializer.SerializeToUtf8Bytes(events, Options);
        }

        /// <summary>
        /// Serializes the event list.
        /// </summary>
        /// <param name="events">The events to serialize.</param>
        /// <returns>The Utf8 binary encoded Json representation of the list.</returns>
        public static string SerializeEventListToString(ImmutableArray<SerializedEvent> events)
        {
            return JsonSerializer.Serialize(events, Options);
        }

        private static JsonSerializerOptions GetDefaultOptions()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new ImmutableArrayTValueConverter());
            options.Converters.Add(new ReadOnlyMemoryByteConverter());
            options.Converters.Add(new SerializedEventConverter());
            return options;
        }
    }
}
