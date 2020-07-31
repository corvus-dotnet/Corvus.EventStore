// <copyright file="Event.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;

    /// <summary>
    /// Represents an event created by an Aggregate Root that can be stored by an <see cref="IEventWriter"/>.
    /// </summary>
    /// <typeparam name="TPayload">The type of the payload.</typeparam>
    public readonly struct Event<TPayload> : IEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Event{TPayload}"/> struct.
        /// </summary>
        /// <param name="aggregateId">The Id of the aggregate to which this event is applied.</param>
        /// <param name="eventType">The <see cref="EventType"/>.</param>
        /// <param name="sequenceNumber">The <see cref="SequenceNumber"/>.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="payload">The <see cref="Payload"/>.</param>
        public Event(string aggregateId, string eventType, long sequenceNumber, string partitionKey, TPayload payload)
        {
            this.AggregateId = aggregateId;
            this.EventType = eventType;
            this.SequenceNumber = sequenceNumber;
            this.PartitionKey = partitionKey;
            this.Payload = payload;
        }

        /// <inheritdoc/>
        public string AggregateId { get; }

        /// <inheritdoc/>
        public long SequenceNumber { get; }

        /// <inheritdoc/>
        public string PartitionKey { get; }

        /// <inheritdoc/>
        public string EventType { get; }

        /// <summary>
        /// Gets the payload data for the event.
        /// </summary>
        public TPayload Payload { get; }

        /// <inheritdoc/>
        public TTarget GetPayload<TTarget>()
        {
            if (this.Payload is TTarget result)
            {
                return result;
            }

            throw new InvalidOperationException();
        }
    }
}
