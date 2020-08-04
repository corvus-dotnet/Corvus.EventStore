// <copyright file="IEventBatchWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    /// <summary>
    /// Interface for types that can write events as part of a batch to the store.
    /// </summary>
    public interface IEventBatchWriter
    {
        /// <summary>
        /// Writes the supplied event to the store as part of a current batch.
        /// </summary>
        /// <typeparam name="TEvent">The type of event being written.</typeparam>
        /// <typeparam name="TPayload">The type of the payload being written.</typeparam>
        /// <param name="event">The event to write.</param>
        void WriteBatchItemAsync<TEvent, TPayload>(in TEvent @event)
            where TEvent : IEvent;
    }
}
