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
        private readonly ConcurrentDictionary<string, InMemorySnapshotList> store =
            new ConcurrentDictionary<string, InMemorySnapshotList>();

        /// <summary>
        /// Reads the specified snapshot for the given aggregate.
        /// </summary>
        /// <typeparam name="TMemento">The type of memento that the target snapshot contains.</typeparam>
        /// <param name="defaultPayloadFactory">A factory method that can be used to create an empty snapshot if none have been stored.</param>
        /// <param name="aggregateId">The Id of the aggregate to read the snapshot for.</param>
        /// <param name="atSequenceId">The sequence Id to read the snapshot at. The snapshot returned will be the one with the highest sequence number less than or equal to this value.</param>
        /// <returns>The most recent snapshot for the aggregate. If no snapshot exists, a new snapshot will be returned containing a payload created via the defaultPayloadFactory.</returns>
        public ValueTask<InMemorySnapshot> ReadAsync<TMemento>(
            Func<TMemento> defaultPayloadFactory,
            string aggregateId,
            long atSequenceId = long.MaxValue)
        {
            if (!this.store.TryGetValue(aggregateId, out InMemorySnapshotList list))
            {
                return new ValueTask<InMemorySnapshot>(new InMemorySnapshot(aggregateId, -1, JsonExtensions.FromObject(defaultPayloadFactory())));
            }

            KeyValuePair<long, InMemorySnapshot>? snapshot = list.Snapshots.OrderByDescending(s => s.Key).Where(s => s.Key < atSequenceId).FirstOrDefault();

            if (snapshot is null)
            {
                return new ValueTask<InMemorySnapshot>(new InMemorySnapshot(aggregateId, -1, JsonExtensions.FromObject(defaultPayloadFactory())));
            }

            return new ValueTask<InMemorySnapshot>(snapshot.Value.Value);
        }

        /// <summary>
        /// Writes the given snapshot to the store.
        /// </summary>
        /// <param name="snapshot">The snapshot to store.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <typeparam name="TSnapshot">The type of the snapshot being written.</typeparam>
        public ValueTask WriteAsync<TSnapshot>(in TSnapshot snapshot)
            where TSnapshot : ISnapshot
        {
            var inMemorySnapshot = InMemorySnapshot.CreateFrom(snapshot);

            this.store.AddOrUpdate(
                snapshot.AggregateId,
                seq =>
                {
                    return new InMemorySnapshotList(ImmutableDictionary<long, InMemorySnapshot>.Empty.Add(inMemorySnapshot.SequenceNumber, inMemorySnapshot));
                },
                (aggregateId, list) =>
                {
                    return list.AddSnapshot(inMemorySnapshot);
                });

            return new ValueTask(Task.CompletedTask);
        }

        private readonly struct ContinuationToken
        {
            public ContinuationToken(string aggregateId, long fromSequenceNumber, long toSequenceNumber, int maxItems)
            {
                this.FromSequenceNumber = fromSequenceNumber;
                this.ToSequenceNumber = toSequenceNumber;
                this.MaxItems = maxItems;
                this.AggregateId = aggregateId;
            }

            public long FromSequenceNumber { get; }

            public long ToSequenceNumber { get; }

            public int MaxItems { get; }

            public string AggregateId { get; }
        }

        private readonly struct InMemorySnapshotList
        {
            public InMemorySnapshotList(ImmutableDictionary<long, InMemorySnapshot> snapshots)
            {
                this.Snapshots = snapshots;
            }

            public ImmutableDictionary<long, InMemorySnapshot> Snapshots { get; }

            public InMemorySnapshotList AddSnapshot(InMemorySnapshot snapshot)
            {
                if (this.Snapshots.ContainsKey(snapshot.SequenceNumber))
                {
                    throw new InMemorySnapshotStoreConcurrencyException();
                }

                return new InMemorySnapshotList(
                    this.Snapshots.Add(snapshot.SequenceNumber, snapshot));
            }
        }
    }
}
