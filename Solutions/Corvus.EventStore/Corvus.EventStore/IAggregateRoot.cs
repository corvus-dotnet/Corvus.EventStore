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
    public interface IAggregateRoot<TMemento>
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
        /// Gets a value containing the store metadata for the aggregate root.
        /// </summary>
        ReadOnlyMemory<byte> StoreMetadata { get; }

        /// <summary>
        /// Gets a value containing the partition key value for the aggregate root (if available).
        /// </summary>
        string PartitionKey { get; }

        /// <summary>
        /// Add an event to the aggregate root.
        /// </summary>
        /// <typeparam name="TPayload">The payload of the event.</typeparam>
        /// <param name="eventType">The type of the event.</param>
        /// <param name="payload">The event payload.</param>
        /// <param name="eventHandler">The event handler that can apply the event to the memento.</param>
        /// <returns>The aggregate root with the uncommitted event added.</returns>
        IAggregateRoot<TMemento> ApplyEvent<TPayload>(string eventType, in TPayload payload, IEventHandler<TMemento> eventHandler);

        /// <summary>
        /// Commits the events that have been added to the aggregate root.
        /// </summary>
        /// <returns>The aggregate root with the events committed.</returns>
        Task<IAggregateRoot<TMemento>> Commit();
    }
}
