// <copyright file="Utf8JsonSnapshotSerializer.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Serialization.Json
{
    using System.Text.Json;
    using System.Threading.Tasks;
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
        public ValueTask<Snapshot<TMemento>> Deserialize<TMemento>(SerializedSnapshot snapshot)
            where TMemento : new()
        {
            if (snapshot.IsEmpty)
            {
                return new ValueTask<Snapshot<TMemento>>(new Snapshot<TMemento>(snapshot.AggregateId, snapshot.SequenceNumber, new TMemento()));
            }

            var reader = new Utf8JsonReader(snapshot.Memento.Span);
            TMemento memento = JsonSerializer.Deserialize<TMemento>(ref reader, this.options);
            return new ValueTask<Snapshot<TMemento>>(new Snapshot<TMemento>(snapshot.AggregateId, snapshot.SequenceNumber, memento));
        }

        /// <inheritdoc/>
        public ValueTask<SerializedSnapshot> Serialize<TMemento>(Snapshot<TMemento> snapshot)
            where TMemento : new()
        {
            byte[] utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(snapshot.Memento, this.options);
            return new ValueTask<SerializedSnapshot>(
                new SerializedSnapshot(
                    snapshot.AggregateId,
                    snapshot.SequenceNumber,
                    utf8Bytes));
        }
    }
}
