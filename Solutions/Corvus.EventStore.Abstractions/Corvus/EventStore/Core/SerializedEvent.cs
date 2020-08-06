// <copyright file="SerializedEvent.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;

    /// <summary>
    /// Represents an event serialized to a byte array.
    /// </summary>
    public readonly struct SerializedEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedEvent"/> struct.
        /// </summary>
        /// <param name="eventType">The <see cref="EventType"/>.</param>
        /// <param name="sequenceNumber">The <see cref="SequenceNumber"/>.</param>
        /// <param name="timestamp">The <see cref="Timestamp"/>.</param>
        /// <param name="payload">The <see cref="Payload"/>.</param>
        public SerializedEvent(
            string eventType,
            long sequenceNumber,
            long timestamp,
            in ReadOnlyMemory<byte> payload)
        {
            this.EventType = eventType;
            this.SequenceNumber = sequenceNumber;
            this.Timestamp = timestamp;
            this.Payload = payload;
        }

        /// <summary>
        /// Gets the unique name for the type of event that this data represents.
        /// </summary>
        /// <remarks>
        /// It is recommended that some type of namespaced name is used, ideally with a scheme that provides for a
        /// version number.
        /// </remarks>
        public string EventType { get; }

        /// <summary>
        /// Gets the nominal wall clock timestamp for the event as determined by the creator of the event.
        /// </summary>
        /// <remarks>While the timestamp of any event in a single commit will normally be >= that of the preceding event, because any single commit
        /// occurrs on a single instance, clocks could be reset while the operation is underway, and so you cannot depend on this.
        /// Sequence numbers determine order, but the timestamp may be useful for diagnostics or reporting.</remarks>
        public long Timestamp { get; }

        /// <summary>
        /// Gets the sequence number for the event.
        /// </summary>
        /// <remarks>
        /// This is a monotonically incrementing value for the aggregate to which the event belongs.</remarks>
        public long SequenceNumber { get; }

        /// <summary>
        /// Gets the payload data for the event.
        /// </summary>
        public ReadOnlyMemory<byte> Payload { get; }
    }
}
