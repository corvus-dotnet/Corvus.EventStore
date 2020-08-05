// <copyright file="IAggregateRoot.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Aggregates
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// An interface for aggregate roots, with standard properties and methods for applying and storing events.
    /// </summary>
    /// <typeparam name="TAggregate">The implementation type.</typeparam>
    public interface IAggregateRoot<TAggregate>
        where TAggregate : IAggregateRoot<TAggregate>
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
        /// <typeparam name="TPayload">The payload of the event to apply.</typeparam>
        /// <param name="event">The event to apply.</param>
        /// <remarks>
        /// This will be called when a new event has been created.
        /// </remarks>
        /// <returns>The aggregate with the event applied.</returns>
        TAggregate ApplyEvent<TPayload>(in Event<TPayload> @event);

        /// <summary>
        /// Applies the given events to the aggregate.
        /// </summary>
        /// <param name="events">The ordered list of events to apply to the aggregate.</param>
        /// <returns>The aggreagte with the events applied.</returns>
        /// <remarks>
        /// This will typically be called when the aggregate is being rehydrated from stored events.
        /// </remarks>
        TAggregate ApplySerializedEvents(in IEnumerable<SerializedEvent> events);

        /// <summary>
        /// Stores uncommitted events using the specified event writer.
        /// </summary>
        /// <typeparam name="TEventWriter">The type of event writer to use.</typeparam>
        /// <param name="writer">The event writer to use to store new events.</param>
        /// <returns>The aggregate with all new events committed.</returns>
        ValueTask<TAggregate> StoreAsync<TEventWriter>(TEventWriter writer)
            where TEventWriter : IEventWriter;

        /// <summary>
        /// Stores a snapshot for the aggregate using the specified snapshot writer.
        /// </summary>
        /// <typeparam name="TSnapshotWriter">The type of snapshot writer to use.</typeparam>
        /// <param name="writer">The snapshot writer to use to store the new snapshot.</param>
        /// <returns>The aggregate once the new snapshot has been stored.</returns>
        Task StoreSnapshotAsync<TSnapshotWriter>(TSnapshotWriter writer)
            where TSnapshotWriter : ISnapshotWriter;
    }
}
