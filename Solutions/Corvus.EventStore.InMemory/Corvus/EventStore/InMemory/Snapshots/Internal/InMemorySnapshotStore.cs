// <copyright file="InMemorySnapshotStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Snapshots.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// Underlying store used by <see cref="InMemorySnapshotReader"/> and <see cref="InMemorySnapshotWriter"/>.
    /// </summary>
    public class InMemorySnapshotStore
    {
        private readonly ConcurrentDictionary<Guid, SnapshotList> store =
            new ConcurrentDictionary<Guid, SnapshotList>();

        /// <summary>
        /// Reads the specified snapshot for the given aggregate.
        /// </summary>
        /// <param name="aggregateId">The Id of the aggregate.</param>
        /// <param name="partitionKey">The partition key of the aggregate.</param>
        /// <param name="atSequenceId">The sequence Id to read the snapshot at. The snapshot returned will be the one with the highest sequence number less than or equal to this value.</param>
        /// <returns>The most recent snapshot for the aggregate. If no snapshot exists, a new snapshot will be returned containing a payload created via the defaultPayloadFactory.</returns>
        public ValueTask<SerializedSnapshot> ReadAsync(
            Guid aggregateId,
            string partitionKey,
            long atSequenceId = long.MaxValue)
        {
            if (!this.store.TryGetValue(aggregateId, out SnapshotList list))
            {
                return new ValueTask<SerializedSnapshot>(SerializedSnapshot.Empty(aggregateId, partitionKey));
            }

            KeyValuePair<long, SerializedSnapshot>? snapshot = list.Snapshots.OrderByDescending(s => s.Key).Where(s => s.Key < atSequenceId).FirstOrDefault();

            if (snapshot is null)
            {
                return new ValueTask<SerializedSnapshot>(SerializedSnapshot.Empty(aggregateId, partitionKey));
            }

            return new ValueTask<SerializedSnapshot>(snapshot.Value.Value);
        }

        /// <summary>
        /// Writes the given snapshot to the store.
        /// </summary>
        /// <param name="snapshot">The snapshot to store.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task WriteAsync(in SerializedSnapshot snapshot)
        {
            SerializedSnapshot localSnapshot = snapshot;
            this.store.AddOrUpdate(
                snapshot.AggregateId,
                seq =>
                {
                    return new SnapshotList(ImmutableDictionary<long, SerializedSnapshot>.Empty.Add(localSnapshot.CommitSequenceNumber, localSnapshot));
                },
                (aggregateId, list) =>
                {
                    return list.AddSnapshot(localSnapshot);
                });

            return Task.CompletedTask;
        }

        private readonly struct SnapshotList
        {
            public SnapshotList(ImmutableDictionary<long, SerializedSnapshot> snapshots)
            {
                this.Snapshots = snapshots;
            }

            public ImmutableDictionary<long, SerializedSnapshot> Snapshots { get; }

            public SnapshotList AddSnapshot(SerializedSnapshot snapshot)
            {
                if (this.Snapshots.ContainsKey(snapshot.CommitSequenceNumber))
                {
                    throw new InMemorySnapshotStoreConcurrencyException();
                }

                return new SnapshotList(
                    this.Snapshots.Add(snapshot.CommitSequenceNumber, snapshot));
            }
        }
    }
}
