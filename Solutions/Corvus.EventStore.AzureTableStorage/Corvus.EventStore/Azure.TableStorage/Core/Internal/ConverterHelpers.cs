// <copyright file="ConverterHelpers.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core.Internal
{
    using System;
    using System.Diagnostics;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Helpers for writing custom <see cref="JsonConverter"/> implementations.
    /// </summary>
    internal static class ConverterHelpers
    {
        /// <summary>
        /// Reads a property of a given type using the converter supplied in the options.
        /// </summary>
        /// <typeparam name="T">The type of the property to read.</typeparam>
        /// <param name="reader">The reader from which to read the property.</param>
        /// <param name="options">The options class from which to create the converter.</param>
        /// <returns>An instance of the property value as the given type.</returns>
        public static T ReadProperty<T>(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            Debug.Assert(reader.TokenType == JsonTokenType.PropertyName, $"Unexpected token type while trying to read a {typeof(T)} property.");

            if (!(options?.GetConverter(typeof(T)) is JsonConverter<T> converter))
            {
                throw new InvalidOperationException();
            }

            reader.Read();
            return converter.Read(ref reader, typeof(T), options);
        }

        /// <summary>
        /// Writes a property of a given type using the converter supplied in the options.
        /// </summary>
        /// <typeparam name="T">The type of the property to write.</typeparam>
        /// <param name="writer">The writer to which to write the property.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="propertyValue">The value of the property.</param>
        /// <param name="options">The options class from which to create the converter.</param>
        public static void WriteProperty<T>(Utf8JsonWriter writer, JsonEncodedText name, T propertyValue, JsonSerializerOptions options)
        {
            if (!(options?.GetConverter(typeof(T)) is JsonConverter<T> converter))
            {
                throw new InvalidOperationException();
            }

            writer.WritePropertyName(name);
            converter.Write(writer, propertyValue, options);
        }
    }
}
