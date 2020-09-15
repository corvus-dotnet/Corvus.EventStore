// <copyright file="IJsonEventPayloadWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Json
{
    using System.Text.Json;

    /// <summary>
    /// Implements an optimized json writer for a given payload.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload to write.</typeparam>
    public interface IJsonEventPayloadWriter<TPayload>
    {
        /// <summary>
        /// Write the payload to the writer.
        /// </summary>
        /// <param name="payload">The payload to write.</param>
        /// <param name="writer">The writer to which to write the payload.</param>
        void Write(TPayload payload, Utf8JsonWriter writer);
    }
}
