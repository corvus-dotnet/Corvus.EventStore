// <copyright file="IEventWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for classes that can write events for an aggregate to the store.
    /// </summary>
    public interface IEventWriter
    {
        /// <summary>
        /// Writes the supplied events to the store as a single transaction.
        /// </summary>
        /// <typeparam name="TEvent">The type of event being written.</typeparam>
        /// <param name="events">The events to write.</param>
        /// <returns>A task that completes when the events have been written to the store.</returns>
        ValueTask WriteAsync<TEvent>(in IEnumerable<TEvent> events)
            where TEvent : IEvent;

        /// <summary>
        /// Writes the supplied event to the store as a single transaction.
        /// </summary>
        /// <typeparam name="TEvent">The type of event being written.</typeparam>
        /// <param name="event">The event to write.</param>
        /// <returns>A task that completes when the events have been written to the store.</returns>
        ValueTask WriteAsync<TEvent>(in TEvent @event)
            where TEvent : IEvent;
    }
}
