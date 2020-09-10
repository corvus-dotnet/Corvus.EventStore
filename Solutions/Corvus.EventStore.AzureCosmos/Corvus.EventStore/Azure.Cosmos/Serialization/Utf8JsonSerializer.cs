// <copyright file="Utf8JsonSerializer.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.Serialization
{
    using System.Text.Json;

    /// <summary>
    /// Serialization helpers for Cosmos DB.
    /// </summary>
    internal static class Utf8JsonSerializer
    {
        /// <summary>
        /// Gets or sets the <see cref="JsonSerializerOptions"/>.
        /// </summary>
        public static JsonSerializerOptions Options { get; set; } = GetDefaultOptions();

        /// <summary>
        /// Deserialize an item.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="item">The Utf8 encoded binary JSON representation of the item.</param>
        /// <returns>An instance of the item.</returns>
        public static T Deserialize<T>(string item)
        {
            return JsonSerializer.Deserialize<T>(item, Options);
        }

        /// <summary>
        /// Serializes the event list.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="item">The item to serialize.</param>
        /// <returns>The Utf8 binary encoded Json representation of the list.</returns>
        public static string Serialize<T>(T item)
        {
            return JsonSerializer.Serialize(item, Options);
        }

        private static JsonSerializerOptions GetDefaultOptions()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new ImmutableArrayTValueConverter());
            options.Converters.Add(new ReadOnlyMemoryByteConverter());
            options.Converters.Add(new SerializedEventConverter());
            options.Converters.Add(new CommitConverter());
            return options;
        }
    }
}
