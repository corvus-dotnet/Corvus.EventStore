// <copyright file="IEvent.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;

    /// <summary>
    /// Represents an event created by an Aggregate Root that can be stored by an <see cref="IEventWriter"/>.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Gets the Id of the aggregate to which this event is applied.
        /// </summary>
        string AggregateId { get; }

        /// <summary>
        /// Gets the unique name for the type of event that this data represents.
        /// </summary>
        /// <remarks>
        /// It is recommended that some type of namespaced name is used, ideally with a scheme that provides for a
        /// version number.
        /// </remarks>
        string EventType { get; }

        /// <summary>
        /// Gets the partition key for the event.
        /// </summary>
        string PartitionKey { get; }

        /// <summary>
        /// Gets the nominal wall clock timestamp for the event as determined by the creator of the event.
        /// </summary>
        long Timestamp { get; }

        /// <summary>
        /// Gets the sequence number for the event.
        /// </summary>
        /// <remarks>
        /// This is a monotonically incrementing value for the aggregate to which the event belongs.</remarks>
        long SequenceNumber { get; }

        /// <summary>
        /// Retrieves the payload coerced to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to coerce to.</typeparam>
        /// <returns>The payload.</returns>
        /// <exception cref="InvalidOperationException">The payload could not be converted to the specified type.</exception>
        T GetPayload<T>();
    }
}