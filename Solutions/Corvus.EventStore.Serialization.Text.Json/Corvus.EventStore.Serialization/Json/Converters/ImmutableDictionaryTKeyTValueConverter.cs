// <copyright file="ImmutableDictionaryTKeyTValueConverter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Serialization.Json.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using Corvus.Extensions;

    /// <summary>
    /// A converter for an <see cref="ImmutableDictionary{TKey,TValue}"/>.
    /// </summary>
    internal class ImmutableDictionaryTKeyTValueConverter : JsonConverterFactory
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }

            if (typeToConvert.GetGenericTypeDefinition() != typeof(ImmutableDictionary<,>))
            {
                return false;
            }

            return typeToConvert.GetGenericArguments()[0] == typeof(string) || typeToConvert.GetGenericArguments()[0] == typeof(Guid);
        }

        /// <inheritdoc/>
        public override JsonConverter CreateConverter(
            Type type,
            JsonSerializerOptions options)
        {
            Type keyType = type.GetGenericArguments()[0];
            Type valueType = type.GetGenericArguments()[1];

            var converter = (JsonConverter)Activator.CreateInstance(
                typeof(ImmutableDictionaryConverterInner<,>).MakeGenericType(
                    new Type[] { keyType, valueType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null);

            return converter;
        }

        private class ImmutableDictionaryConverterInner<TKey, TValue> :
            JsonConverter<ImmutableDictionary<TKey, TValue>>
        {
            private readonly JsonConverter<TValue> valueConverter;
            private readonly Type keyType;
            private readonly Type valueType;

            public ImmutableDictionaryConverterInner(JsonSerializerOptions options)
            {
                // For performance, use the existing converter if available.
                this.valueConverter = (JsonConverter<TValue>)options
                    .GetConverter(typeof(TValue));

                // Cache the key and value types.
                this.keyType = typeof(TKey);
                this.valueType = typeof(TValue);
            }

            public override ImmutableDictionary<TKey, TValue> Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                var builder = ImmutableDictionary<TKey, TValue>.Empty.ToBuilder();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return builder.ToImmutable();
                    }

                    // Get the key.
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    TKey key;

                    if (this.keyType == typeof(Guid))
                    {
                        key = CastTo<TKey>.From(Guid.Parse(reader.GetString()));
                    }
                    else
                    {
                        key = CastTo<TKey>.From(reader.GetString());
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
                    builder.Add(key, v);
                }

                throw new JsonException();
            }

            public override void Write(
                Utf8JsonWriter writer,
                ImmutableDictionary<TKey, TValue> dictionary,
                JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
                {
                    if (kvp.Key is null)
                    {
                        throw new JsonException("You are not permitted to use null keys in the ImmutableCollection{TKey, TValue}.");
                    }

                    writer.WritePropertyName(kvp.Key.ToString());

                    if (this.valueConverter != null)
                    {
                        this.valueConverter.Write(writer, kvp.Value, options);
                    }
                    else
                    {
                        JsonSerializer.Serialize(writer, kvp.Value, options);
                    }
                }

                writer.WriteEndObject();
            }
        }
    }
}
