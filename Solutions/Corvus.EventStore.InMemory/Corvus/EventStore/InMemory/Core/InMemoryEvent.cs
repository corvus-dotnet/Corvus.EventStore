// <copyright file="InMemoryEvent.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Core
{
    using System;
    using Corvus.EventStore.Core;

    /// <summary>
    /// In-memory event reader implementation over System.Text.Json.
    /// </summary>
    public readonly struct InMemoryEvent : IEvent
    {
        private readonly byte[] source;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEvent"/> struct.
        /// </summary>
        /// <param name="aggregateId">The Id of the aggregate to which this event is applied.</param>
        /// <param name="eventType">The <see cref="EventType"/>.</param>
        /// <param name="sequenceNumber">The <see cref="SequenceNumber"/>.</param>
        /// <param name="timestamp">The <see cref="Timestamp"/>.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="source">The serialized payload of the event.</param>
        public InMemoryEvent(
            string aggregateId,
            string eventType,
            long sequenceNumber,
            long timestamp,
            string partitionKey,
            ReadOnlySpan<byte> source)
        {
            this.AggregateId = aggregateId;
            this.EventType = eventType;
            this.SequenceNumber = sequenceNumber;
            this.Timestamp = timestamp;
            this.PartitionKey = partitionKey;
            this.source = source.ToArray();
        }

        /// <inheritdoc/>
        public string AggregateId { get; }

        /// <inheritdoc/>
        public string EventType { get; }

        /// <inheritdoc/>
        public string PartitionKey { get; }

        /// <inheritdoc/>
        public long Timestamp { get; }

        /// <inheritdoc/>
        public long SequenceNumber { get; }

        /// <summary>
        /// Creates a new InMemoryEvent from a source Event{TPayload}.
        /// </summary>
        /// <typeparam name="TEvent">the type of the event.</typeparam>
        /// <typeparam name="TPayload">the type of the payload.</typeparam>
        /// <param name="event">The source event.</param>
        /// <returns>A new InMemoryEvent.</returns>
        public static InMemoryEvent CreateFrom<TEvent, TPayload>(TEvent @event)
            where TEvent : IEvent
        {
            return new InMemoryEvent(
                @event.AggregateId,
                @event.EventType,
                @event.SequenceNumber,
                @event.Timestamp,
                @event.PartitionKey,
                JsonExtensions.FromObject(@event.GetPayload<TPayload>()));
        }

        /// <inheritdoc/>
        public T GetPayload<T>()
        {
            return JsonExtensions.ToObject<T>(this.source);
        }
    }
}
