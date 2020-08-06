// <copyright file="Utf8JsonSnapshotSerializer.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Serialization.Json
{
    using System.Text.Json;
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// An <see cref="ISnapshotSerializer"/> that uses utf8 JSON text.
    /// </summary>
    public readonly struct Utf8JsonSnapshotSerializer : ISnapshotSerializer
    {
        private readonly JsonSerializerOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="Utf8JsonSnapshotSerializer"/> struct.
        /// </summary>
        /// <param name="options">The <see cref="JsonSerializerOptions"/>.</param>
        public Utf8JsonSnapshotSerializer(JsonSerializerOptions options)
        {
            this.options = options;
        }

        /// <inheritdoc/>
        public Snapshot<TMemento> Deserialize<TMemento>(in SerializedSnapshot snapshot)
            where TMemento : new()
        {
            if (snapshot.IsEmpty)
            {
                return new Snapshot<TMemento>(snapshot.AggregateId, snapshot.SequenceNumber, new TMemento());
            }

            var reader = new Utf8JsonReader(snapshot.Memento.Span);
            TMemento memento = JsonSerializer.Deserialize<TMemento>(ref reader, this.options);
            return new Snapshot<TMemento>(snapshot.AggregateId, snapshot.SequenceNumber, memento);
        }

        /// <inheritdoc/>
        public SerializedSnapshot Serialize<TMemento>(in Snapshot<TMemento> snapshot)
            where TMemento : new()
        {
            byte[] utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(snapshot.Memento, this.options);
            return new SerializedSnapshot(
                snapshot.AggregateId,
                snapshot.CommitSequenceNumber,
                utf8Bytes);
        }
    }
}
