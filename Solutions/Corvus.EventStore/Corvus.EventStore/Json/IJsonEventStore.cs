// <copyright file="IJsonEventStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Json;

    /// <summary>
    /// An interface implemented by an event store that provides a means of reading
    /// the aggregate for a given ID.
    /// </summary>
    public interface IJsonEventStore : IEventStore
    {
        /// <summary>
        /// Reads an aggregate root.
        /// </summary>
        /// <typeparam name="TMemento">The type of the memento for the aggregate root.</typeparam>
        /// <typeparam name="TEventHandler">The type of the <see cref="IJsonEventHandler{TMemento}"/>.</typeparam>
        /// <param name="id">The ID of the aggregate root.</param>
        /// <param name="emptyMemento">The initial empty memento.</param>
        /// <param name="eventHandler">The event reader capable of decoding and applying the event payloads for this aggregate root.</param>
        /// <returns>A <see cref="Task{TResult}"/> which provide the <see cref="IAggregateRoot{TMemento}"/> loaded from the store.</returns>
        Task<JsonAggregateRoot<TMemento>> Read<TMemento, TEventHandler>(Guid id, TMemento emptyMemento, TEventHandler eventHandler)
            where TEventHandler : IJsonEventHandler<TMemento>;

        /// <summary>
        /// Reads an aggregate root.
        /// </summary>
        /// <typeparam name="TMemento">The type of the memento for the aggregate root.</typeparam>
        /// <typeparam name="TEventHandler">The type of the <see cref="IJsonEventHandler{TMemento}"/>.</typeparam>
        /// <param name="id">The ID of the aggregate root.</param>
        /// <param name="partitionKey">The partition key for this aggregate root.</param>
        /// <param name="emptyMemento">The initial empty memento.</param>
        /// <param name="eventHandler">The event reader capable of decoding and applying the event payloads for this aggregate root.</param>
        /// <returns>A <see cref="Task{TResult}"/> which provide the <see cref="IAggregateRoot{TMemento}"/> loaded from the store.</returns>
        Task<JsonAggregateRoot<TMemento>> Read<TMemento, TEventHandler>(Guid id, string partitionKey, TMemento emptyMemento, TEventHandler eventHandler)
            where TEventHandler : IJsonEventHandler<TMemento>;

        /// <summary>
        /// Create a new aggregate root.
        /// </summary>
        /// <typeparam name="TMemento">The type of the memento for the aggregate root.</typeparam>
        /// <typeparam name="TEventHandler">The type of the <see cref="IJsonEventHandler{TMemento}"/>.</typeparam>
        /// <param name="id">The ID of the aggregate root.</param>
        /// <param name="emptyMemento">The initial empty memento.</param>
        /// <param name="eventHandler">The event reader capable of decoding and applying the event payloads for this aggregate root.</param>
        /// <returns>The new <see cref="IAggregateRoot{TMemento}"/>.</returns>
        JsonAggregateRoot<TMemento> Create<TMemento, TEventHandler>(Guid id, TMemento emptyMemento, TEventHandler eventHandler)
            where TEventHandler : IJsonEventHandler<TMemento>;

        /// <summary>
        /// Create a new aggregate root.
        /// </summary>
        /// <typeparam name="TMemento">The type of the memento for the aggregate root.</typeparam>
        /// <typeparam name="TEventHandler">The type of the <see cref="IJsonEventHandler{TMemento}"/>.</typeparam>
        /// <param name="id">The ID of the aggregate root.</param>
        /// <param name="partitionKey">The partition key of the aggregate root.</param>
        /// <param name="emptyMemento">The initial empty memento.</param>
        /// <param name="eventHandler">The event reader capable of decoding and applying the event payloads for this aggregate root.</param>
        /// <returns>The new <see cref="IAggregateRoot{TMemento}"/>.</returns>
        JsonAggregateRoot<TMemento> Create<TMemento, TEventHandler>(Guid id, string partitionKey, TMemento emptyMemento, TEventHandler eventHandler)
            where TEventHandler : IJsonEventHandler<TMemento>;
    }
}
