// <copyright file="IAggregateRoot.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface implemented by aggregate roots.
    /// </summary>
    /// <typeparam name="TMemento">The type of the memento for the aggregate root.</typeparam>
    /// <typeparam name="T">The type of the entity implementing this interface.</typeparam>
    public interface IAggregateRoot<TMemento, T>
        where T : IAggregateRoot<TMemento, T>
    {
        /// <summary>
        /// Gets the unique id of the aggregate root.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the current memento for the aggregate root.
        /// </summary>
        TMemento Memento { get; }

        /// <summary>
        /// Gets the current event sequence number.
        /// </summary>
        /// <remarks>This is the sequence number of the last event applied to the aggregate root, whether commited or uncommitted.</remarks>
        long EventSequenceNumber { get; }

        /// <summary>
        /// Gets the current commit sequence number.
        /// </summary>
        /// <remarks>This is the sequence number of the last commit applied to the aggregate root.</remarks>
        long CommitSequenceNumber { get; }

        /// <summary>
        /// Gets a value indicating whether the aggregate root has uncommitted events.
        /// </summary>
        bool HasUncommittedEvents { get; }

        /// <summary>
        /// Add an event to the aggregate root.
        /// </summary>
        /// <typeparam name="TPayload">The payload of the event.</typeparam>
        /// <typeparam name="TEventHandler">The type of th <see cref="IEventHandler{TMemento}"/>.</typeparam>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="payload">The event payload.</param>
        /// <param name="eventHandler">The event handler that can apply the event to the memento.</param>
        /// <returns>The aggregate root with the uncommitted event added.</returns>
        T ApplyEvent<TPayload, TEventHandler>(string eventType, TPayload payload, TEventHandler eventHandler)
            where TEventHandler : IEventHandler<TMemento>;

        /// <summary>
        /// Commits the events that have been added to the aggregate root.
        /// </summary>
        /// <returns>The aggregate root with the events committed.</returns>
        Task<T> Commit();
    }
}
