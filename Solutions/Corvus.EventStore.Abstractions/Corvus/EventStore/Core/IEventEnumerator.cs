// <copyright file="IEventEnumerator.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Provides an interface for enumerating a set of events.
    /// </summary>
    public interface IEventEnumerator : IEnumerable<IEvent>, IEnumerable, IEnumerator<IEvent>, IEnumerator
    {
        /// <summary>
        /// Gets the Id of the aggregate to which this event is applied.
        /// </summary>
        string CurrentAggregateId { get; }

        /// <summary>
        /// Gets the unique name for the type of event that this data represents.
        /// </summary>
        /// <remarks>
        /// It is recommended that some type of namespaced name is used, ideally with a scheme that provides for a
        /// version number.
        /// </remarks>
        string CurrentEventType { get; }

        /// <summary>
        /// Gets the partition key for the event.
        /// </summary>
        string CurrentPartitionKey { get; }

        /// <summary>
        /// Gets the nominal wall clock timestamp for the event as determined by the creator of the event.
        /// </summary>
        long CurrentTimestamp { get; }

        /// <summary>
        /// Gets the sequence number for the event.
        /// </summary>
        /// <remarks>
        /// This is a monotonically incrementing value for the aggregate to which the event belongs.</remarks>
        long CurrentSequenceNumber { get; }

        /// <summary>
        /// Retrieves the payload coerced to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to coerce to.</typeparam>
        /// <returns>The payload.</returns>
        /// <exception cref="InvalidOperationException">The payload could not be converted to the specified type.</exception>
        T GetCurrentPayload<T>();
    }
}
