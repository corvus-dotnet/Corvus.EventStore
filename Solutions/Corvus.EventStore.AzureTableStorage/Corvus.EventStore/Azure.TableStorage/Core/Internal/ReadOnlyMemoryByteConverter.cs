// <copyright file="ReadOnlyMemoryByteConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core.Internal
{
    using System;
    using System.Collections.Immutable;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A converter for an <see cref="ImmutableDictionary{TKey,TValue}"/>.
    /// </summary>
    internal class ReadOnlyMemoryByteConverter : JsonConverter<ReadOnlyMemory<byte>>
    {
        /// <inheritdoc/>
        public override ReadOnlyMemory<byte> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            return reader.GetBytesFromBase64().AsMemory();
        }

        /// <inheritdoc/>
        public override void Write(
            Utf8JsonWriter writer,
            ReadOnlyMemory<byte> array,
            JsonSerializerOptions options)
        {
            writer.WriteBase64StringValue(array.Span);
        }
    }
}
