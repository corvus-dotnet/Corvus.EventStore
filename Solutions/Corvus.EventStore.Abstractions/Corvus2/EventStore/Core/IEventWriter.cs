// <copyright file="IEventWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus2.EventStore.Core
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for classes that can write events for an aggregate to the store.
    /// </summary>
    public interface IEventWriter
    {
        /// <summary>
        /// Writes the supplied event to the store as a single transaction.
        /// </summary>
        /// <param name="event">The event to write.</param>
        /// <returns>A task that completes when the events have been written to the store.</returns>
        Task WriteAsync(in SerializedEvent @event);

        /// <summary>
        /// Performs the supplied list of events to the store as a single transaction.
        /// </summary>
        /// <param name="events">batch set of events write.</param>
        /// <returns>A task that completes when the events have been written to the store.</returns>
        Task WriteBatchAsync(in IEnumerable<SerializedEvent> events);
    }
}
