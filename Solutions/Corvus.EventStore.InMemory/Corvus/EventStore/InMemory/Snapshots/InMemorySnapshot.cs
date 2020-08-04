// <copyright file="InMemorySnapshot.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Snapshots
{
    using System;
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// In-memory specific implementation of <see cref="ISnapshot"/>.
    /// </summary>
    public readonly struct InMemorySnapshot : ISnapshot
    {
        private readonly byte[] source;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemorySnapshot"/> struct.
        /// </summary>
        /// <param name="aggregateId">The Id of the aggregate to which this event is applied.</param>
        /// <param name="sequenceNumber">The <see cref="SequenceNumber"/>.</param>
        /// <param name="source">The serialized payload of the event.</param>
        public InMemorySnapshot(
            string aggregateId,
            long sequenceNumber,
            ReadOnlySpan<byte> source)
        {
            this.AggregateId = aggregateId;
            this.SequenceNumber = sequenceNumber;
            this.source = source.ToArray();
        }

        /// <inheritdoc/>
        public string AggregateId { get; }

        /// <inheritdoc/>
        public long SequenceNumber { get; }

        /// <summary>
        /// Creates a new InMemoryEvent from a source ISnapshot.
        /// </summary>
        /// <typeparam name="TSnapshot">The type of the snapshot.</typeparam>
        /// <typeparam name="TMemento">The type of the memento.</typeparam>
        /// <param name="event">The source snapshot.</param>
        /// <returns>A new InMemorySnapshot.</returns>
        public static InMemorySnapshot CreateFrom<TSnapshot, TMemento>(TSnapshot @event)
            where TSnapshot : ISnapshot
        {
            return new InMemorySnapshot(
                @event.AggregateId,
                @event.SequenceNumber,
                JsonExtensions.FromObject(@event.GetPayload<TMemento>()));
        }

        /// <inheritdoc/>
        public TMemento GetPayload<TMemento>()
        {
            return JsonExtensions.ToObject<TMemento>(this.source);
        }
    }
}
