// <copyright file="IEventStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface implemented by an event store that provides a means of reading
    /// the aggregate for a given ID.
    /// </summary>
    public interface IEventStore
    {
        /// <summary>
        /// Reads an aggregate root.
        /// </summary>
        /// <typeparam name="TMemento">The type of the memento for the aggregate root.</typeparam>
        /// <param name="id">The ID of the aggregate root.</param>
        /// <param name="partitionKey">The partition key of the aggregate root.</param>
        /// <param name="emptyMemento">The initial empty memento.</param>
        /// <param name="eventHandler">The event reader capable of decoding and applying the event payloads for this aggregate root.</param>
        /// <returns>A <see cref="Task{TResult}"/> which provide the <see cref="IAggregateRoot{TMemento}"/> loaded from the store.</returns>
        Task<IAggregateRoot<TMemento>> Read<TMemento>(Guid id, string partitionKey, TMemento emptyMemento, IEventHandler<TMemento> eventHandler);

        /// <summary>
        /// Reads an aggregate root.
        /// </summary>
        /// <typeparam name="TMemento">The type of the memento for the aggregate root.</typeparam>
        /// <param name="id">The ID of the aggregate root.</param>
        /// <param name="emptyMemento">The initial empty memento.</param>
        /// <param name="eventHandler">The event reader capable of decoding and applying the event payloads for this aggregate root.</param>
        /// <returns>A <see cref="Task{TResult}"/> which provide the <see cref="IAggregateRoot{TMemento}"/> loaded from the store.</returns>
        Task<IAggregateRoot<TMemento>> Read<TMemento>(Guid id, TMemento emptyMemento, IEventHandler<TMemento> eventHandler);

        /// <summary>
        /// Create a new aggregate root.
        /// </summary>
        /// <typeparam name="TMemento">The type of the memento for the aggregate root.</typeparam>
        /// <param name="id">The ID of the aggregate root.</param>
        /// <param name="emptyMemento">The initial empty memento.</param>
        /// <param name="eventHandler">The event reader capable of decoding and applying the event payloads for this aggregate root.</param>
        /// <returns>The new <see cref="IAggregateRoot{TMemento}"/>.</returns>
        IAggregateRoot<TMemento> Create<TMemento>(Guid id, TMemento emptyMemento, IEventHandler<TMemento> eventHandler);

        /// <summary>
        /// Create a new aggregate root.
        /// </summary>
        /// <typeparam name="TMemento">The type of the memento for the aggregate root.</typeparam>
        /// <param name="id">The ID of the aggregate root.</param>
        /// <param name="partitionKey">The partition key of the aggregate root.</param>
        /// <param name="emptyMemento">The initial empty memento.</param>
        /// <param name="eventHandler">The event reader capable of decoding and applying the event payloads for this aggregate root.</param>
        /// <returns>The new <see cref="IAggregateRoot{TMemento}"/>.</returns>
        IAggregateRoot<TMemento> Create<TMemento>(Guid id, string partitionKey, TMemento emptyMemento, IEventHandler<TMemento> eventHandler);
    }
}
