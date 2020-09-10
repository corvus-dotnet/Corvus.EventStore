// <copyright file="ImmutableArrayTValueConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.Serialization
{
    using System;
    using System.Collections.Immutable;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A converter for an <see cref="ImmutableDictionary{TKey,TValue}"/>.
    /// </summary>
    internal class ImmutableArrayTValueConverter : JsonConverterFactory
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }

            if (typeToConvert.GetGenericTypeDefinition() != typeof(ImmutableArray<>))
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override JsonConverter CreateConverter(
            Type type,
            JsonSerializerOptions options)
        {
            Type valueType = type.GetGenericArguments()[0];

            var converter = (JsonConverter)Activator.CreateInstance(
                typeof(ImmutableArrayConverterInner<>).MakeGenericType(
                    new Type[] { valueType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null);

            return converter;
        }

        private class ImmutableArrayConverterInner<TValue> :
            JsonConverter<ImmutableArray<TValue>>
        {
            private readonly JsonConverter<TValue> valueConverter;
            private readonly Type valueType;

            public ImmutableArrayConverterInner(JsonSerializerOptions options)
            {
                // For performance, use the existing converter if available.
                this.valueConverter = (JsonConverter<TValue>)options
                    .GetConverter(typeof(TValue));

                // Cache the value types.
                this.valueType = typeof(TValue);
            }

            public override ImmutableArray<TValue> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException();
                }

                ImmutableArray<TValue>.Builder builder = ImmutableArray.CreateBuilder<TValue>();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        return builder.ToImmutable();
                    }

                    // Get the value.
                    TValue v;
                    if (this.valueConverter != null)
                    {
                        v = this.valueConverter.Read(ref reader, this.valueType, options);
                    }
                    else
                    {
                        v = JsonSerializer.Deserialize<TValue>(ref reader, options);
                    }

                    // Add to dictionary.
                    builder.Add(v);
                }

                throw new JsonException();
            }

            public override void Write(
                Utf8JsonWriter writer,
                ImmutableArray<TValue> array,
                JsonSerializerOptions options)
            {
                writer.WriteStartArray();

                foreach (TValue v in array)
                {
                    if (this.valueConverter != null)
                    {
                        this.valueConverter.Write(writer, v, options);
                    }
                    else
                    {
                        JsonSerializer.Serialize(writer, v, options);
                    }
                }

                writer.WriteEndArray();
            }
        }
    }
}
