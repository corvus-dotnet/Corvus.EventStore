// <copyright file="Utf8JsonSnapshotSerializer.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus2.EventStore.Serialization.Json
{
    using System;
    using System.Text.Json;
    using Corvus2.EventStore.Snapshots;

    /// <summary>
    /// An <see cref="ISnapshotSerializer"/> that uses utf8 JSON text.
    /// </summary>
    public readonly struct Utf8JsonSnapshotSerializer : ISnapshotSerializer
    {
        /// <inheritdoc/>
        public TSnapshot Deserialize<TSnapshot, TPayload>(SerializedSnapshot @snapshot, Func<string, long, TPayload, TSnapshot> factory)
            where TSnapshot : ISnapshot
        {
            var reader = new Utf8JsonReader(@snapshot.Utf8TextMemento.Span);
            TPayload payload = JsonSerializer.Deserialize<TPayload>(ref reader);
            return factory(@snapshot.AggregateId, @snapshot.SequenceNumber, payload);
        }

        /// <inheritdoc/>
        public SerializedSnapshot Serialize<TSnapshot, TPayload>(TSnapshot @snapshot)
            where TSnapshot : ISnapshot
        {
            TPayload payload = @snapshot.GetPayload<TPayload>();
            byte[] utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
            return new SerializedSnapshot(
                @snapshot.AggregateId,
                @snapshot.SequenceNumber,
                utf8Bytes);
        }
    }
}
