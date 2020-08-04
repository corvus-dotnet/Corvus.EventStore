// <copyright file="JsonExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory
{
    using System;
    using System.Buffers;
    using System.Text.Json;

    /// <summary>
    /// Json seriliazation/deserialization extensions from https://github.com/dotnet/runtime/issues/31274.
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        /// Deserializes a JsonElement to the specified type.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="source">The source data.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>The deserialized object.</returns>
        public static T ToObject<T>(Span<byte> source, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Deserialize<T>(source, options);
        }

        /// <summary>
        /// Serializes an object to UTF8 JSON returned as a <c>Span{byte}</c>.
        /// </summary>
        /// <typeparam name="T">The type of the source object.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>A UTF8 JSON representation of the source object.</returns>
        public static Span<byte> FromObject<T>(T source, JsonSerializerOptions? options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();

            using (var writer = new Utf8JsonWriter(bufferWriter))
            {
                JsonSerializer.Serialize(writer, source, options);
            }

            return bufferWriter.GetSpan();
        }
    }
}
