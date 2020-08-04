// <copyright file="IEventWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;
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
        /// <typeparam name="TEvent">The type of event being written.</typeparam>
        /// <typeparam name="TPayload">The type of the payload being written.</typeparam>
        /// <param name="event">The event to write.</param>
        /// <returns>A task that completes when the events have been written to the store.</returns>
        ValueTask WriteAsync<TEvent, TPayload>(in TEvent @event)
            where TEvent : IEvent;

        /// <summary>
        /// Performs the supplied list of writes to the store as a single transaction.
        /// </summary>
        /// <param name="eventWrites">The set of writes to add to the batch.</param>
        /// <returns>A task that completes when the events have been written to the store.</returns>
        ValueTask WriteBatchAsync(in IEnumerable<Action<IEventBatchWriter>> eventWrites);
    }
}
