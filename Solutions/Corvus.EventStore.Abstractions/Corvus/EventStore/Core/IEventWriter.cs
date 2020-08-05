// <copyright file="IEventWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;
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
        /// Writes the supplied events to the store as a single transaction.
        /// </summary>
        /// <typeparam name="TEvent1">The type of the first event being written.</typeparam>
        /// <typeparam name="TPayload1">The type of the first payload being written.</typeparam>
        /// <typeparam name="TEvent2">The type of the second event being written.</typeparam>
        /// <typeparam name="TPayload2">The type of the second payload being written.</typeparam>
        /// <param name="event1">The first event to write.</param>
        /// <param name="event2">The second event to write.</param>
        /// <returns>A task that completes when the events have been written to the store.</returns>
        ValueTask WriteAsync<TEvent1, TPayload1, TEvent2, TPayload2>(in TEvent1 @event1, in TEvent2 @event2)
            where TEvent1 : IEvent
            where TEvent2 : IEvent;

        /// <summary>
        /// Writes the supplied events to the store as a single transaction.
        /// </summary>
        /// <typeparam name="TEvent1">The type of the first event being written.</typeparam>
        /// <typeparam name="TPayload1">The type of the first payload being written.</typeparam>
        /// <typeparam name="TEvent2">The type of the second event being written.</typeparam>
        /// <typeparam name="TPayload2">The type of the second payload being written.</typeparam>
        /// <typeparam name="TEvent3">The type of the third event being written.</typeparam>
        /// <typeparam name="TPayload3">The type of the third payload being written.</typeparam>
        /// <param name="event1">The first event to write.</param>
        /// <param name="event2">The second event to write.</param>
        /// <param name="event3">The third event to write.</param>
        /// <returns>A task that completes when the events have been written to the store.</returns>
        ValueTask WriteAsync<TEvent1, TPayload1, TEvent2, TPayload2, TEvent3, TPayload3>(in TEvent1 @event1, in TEvent2 @event2, in TEvent3 @event3)
            where TEvent1 : IEvent
            where TEvent2 : IEvent
            where TEvent3 : IEvent;

        /// <summary>
        /// Writes the supplied events to the store as a single transaction.
        /// </summary>
        /// <typeparam name="TEvent1">The type of the first event being written.</typeparam>
        /// <typeparam name="TPayload1">The type of the first payload being written.</typeparam>
        /// <typeparam name="TEvent2">The type of the second event being written.</typeparam>
        /// <typeparam name="TPayload2">The type of the second payload being written.</typeparam>
        /// <typeparam name="TEvent3">The type of the third event being written.</typeparam>
        /// <typeparam name="TPayload3">The type of the third payload being written.</typeparam>
        /// <typeparam name="TEvent4">The type of the fourth event being written.</typeparam>
        /// <typeparam name="TPayload4">The type of the fourth payload being written.</typeparam>
        /// <param name="event1">The first event to write.</param>
        /// <param name="event2">The second event to write.</param>
        /// <param name="event3">The third event to write.</param>
        /// <param name="event4">The fourth event to write.</param>
        /// <returns>A task that completes when the events have been written to the store.</returns>
        ValueTask WriteAsync<TEvent1, TPayload1, TEvent2, TPayload2, TEvent3, TPayload3, TEvent4, TPayload4>(in TEvent1 @event1, in TEvent2 @event2, in TEvent3 @event3, in TEvent4 @event4)
            where TEvent1 : IEvent
            where TEvent2 : IEvent
            where TEvent3 : IEvent
            where TEvent4 : IEvent;

        /// <summary>
        /// Writes the supplied events to the store as a single transaction.
        /// </summary>
        /// <typeparam name="TEvent1">The type of the first event being written.</typeparam>
        /// <typeparam name="TPayload1">The type of the first payload being written.</typeparam>
        /// <typeparam name="TEvent2">The type of the second event being written.</typeparam>
        /// <typeparam name="TPayload2">The type of the second payload being written.</typeparam>
        /// <typeparam name="TEvent3">The type of the third event being written.</typeparam>
        /// <typeparam name="TPayload3">The type of the third payload being written.</typeparam>
        /// <typeparam name="TEvent4">The type of the fourth event being written.</typeparam>
        /// <typeparam name="TPayload4">The type of the fourth payload being written.</typeparam>
        /// <typeparam name="TEvent5">The type of the fifth event being written.</typeparam>
        /// <typeparam name="TPayload5">The type of the fifth payload being written.</typeparam>
        /// <param name="event1">The first event to write.</param>
        /// <param name="event2">The second event to write.</param>
        /// <param name="event3">The third event to write.</param>
        /// <param name="event4">The fourth event to write.</param>
        /// <param name="event5">The fifth event to write.</param>
        /// <returns>A task that completes when the events have been written to the store.</returns>
        ValueTask WriteAsync<TEvent1, TPayload1, TEvent2, TPayload2, TEvent3, TPayload3, TEvent4, TPayload4, TEvent5, TPayload5>(in TEvent1 @event1, in TEvent2 @event2, in TEvent3 @event3, in TEvent4 @event4, in TEvent5 @event5)
            where TEvent1 : IEvent
            where TEvent2 : IEvent
            where TEvent3 : IEvent
            where TEvent4 : IEvent
            where TEvent5 : IEvent;

        /// <summary>
        /// Performs the supplied list of writes to the store as a single transaction.
        /// </summary>
        /// <param name="eventWrites">The set of writes to add to the batch.</param>
        /// <returns>A task that completes when the events have been written to the store.</returns>
        ValueTask WriteBatchAsync(params Action<IEventBatchWriter>[] eventWrites);
    }
}
