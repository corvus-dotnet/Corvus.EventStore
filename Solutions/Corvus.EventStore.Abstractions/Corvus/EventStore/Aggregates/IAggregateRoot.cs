// <copyright file="IAggregateRoot.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Aggregates
{
    using System.Threading.Tasks;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// An interface for aggregate roots, with standard properties and methods for applying and storing events.
    /// </summary>
    public interface IAggregateRoot
    {
        /// <summary>
        /// Gets the unique Id for the aggregate.
        /// </summary>
        string AggregateId { get; }

        /// <summary>
        /// Gets the current sequence number for the aggregate.
        /// </summary>
        long SequenceNumber { get; }

        /// <summary>
        /// Applies the given event to the aggregate.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to apply.</typeparam>
        /// <typeparam name="TAggregate">The type of the aggregate to which the event is being applied.</typeparam>
        /// <param name="event">The event to apply.</param>
        /// <remarks>
        /// This will be called in two scenarios:
        /// - A new event has been created.
        /// - The aggregate is being rehydrated from stored events.
        /// </remarks>
        /// <returns>The aggregate with the event applied.</returns>
        TAggregate ApplyEvent<TEvent, TAggregate>(TEvent @event)
            where TEvent : IEvent
            where TAggregate : IAggregateRoot;

        /// <summary>
        /// Stores uncommitted events using the specified event writer.
        /// </summary>
        /// <typeparam name="TEventWriter">The type of event writer to use.</typeparam>
        /// <typeparam name="TAggregate">The type of the aggregate root being stored.</typeparam>
        /// <param name="writer">The event writer to use to store new events.</param>
        /// <returns>The aggregate with all new events committed.</returns>
        ValueTask<TAggregate> StoreAsync<TEventWriter, TAggregate>(in TEventWriter writer)
            where TEventWriter : IEventWriter
            where TAggregate : IAggregateRoot;

        /// <summary>
        /// Stores a snapshot for the aggregate using the specified snapshot writer.
        /// </summary>
        /// <typeparam name="TSnapshotWriter">The type of snapshot writer to use.</typeparam>
        /// <typeparam name="TAggregate">The type of the aggregate root being stored.</typeparam>
        /// <param name="writer">The snapshot writer to use to store the new snapshot.</param>
        /// <returns>The aggregate once the new snapshot has been stored.</returns>
        ValueTask<TAggregate> StoreSnapshotAsync<TSnapshotWriter, TAggregate>(in TSnapshotWriter writer)
            where TSnapshotWriter : ISnapshotWriter
            where TAggregate : IAggregateRoot;
    }
}
