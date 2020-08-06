﻿// <copyright file="AggregateImplementationWithMemento.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Serialization;
    using Corvus.EventStore.Serialization.Json;
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// An implementation helper for aggregate roots that store their state in an internal memento.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the aggregate root that is using this implementation.</typeparam>
    /// <typeparam name="TMemento">The type of the memento.</typeparam>
    public readonly struct AggregateImplementationWithMemento<TAggregate, TMemento>
        where TAggregate : IAggregateRoot<TAggregate>, IAggregateImplementationWithMementoHost<TAggregate, TMemento>
        where TMemento : new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateImplementationWithMemento{Task, TMemento}"/> struct.
        /// </summary>
        /// <param name="aggregateId">The <see cref="AggregateId"/>.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="commitSequenceNumber">The <see cref="CommitSequenceNumber"/>.</param>
        /// <param name="eventSequenceNumber">The <see cref="EventSequenceNumber"/>.</param>
        /// <param name="uncommittedEvents">The <see cref="UncommittedEvents"/>.</param>
        /// <param name="memento">The <see cref="Memento"/>.</param>
        private AggregateImplementationWithMemento(string aggregateId, string partitionKey, long commitSequenceNumber, long eventSequenceNumber, in ImmutableArray<SerializedEvent> uncommittedEvents, in TMemento memento)
        {
            this.AggregateId = aggregateId;
            this.PartitionKey = partitionKey;
            this.CommitSequenceNumber = commitSequenceNumber;
            this.EventSequenceNumber = eventSequenceNumber;
            this.UncommittedEvents = uncommittedEvents;
            this.Memento = memento;
        }

        /// <summary>
        /// Gets or sets the event serializer to use for the aggregate.
        /// </summary>
        public static IEventSerializer EventSerializer { get; set; } = default(Utf8JsonEventSerializer);

        /// <summary>
        /// Gets or sets the snapshot serializer to use for the aggregate.
        /// </summary>
        public static ISnapshotSerializer SnapshotSerializer { get; set; } = default(Utf8JsonSnapshotSerializer);

        /// <summary>
        /// Gets the unique Id for the aggregate.
        /// </summary>
        public string AggregateId { get; }

        /// <summary>
        /// Gets the partition key for the aggregate.
        /// </summary>
        /// <remarks>
        /// For most implementations, the AggregateId makes a good partition key.
        /// </remarks>
        public string PartitionKey { get; }

        /// <summary>
        /// Gets the sequence number of the current commit for the aggregate.
        /// </summary>
        /// <remarks>
        /// This represents the last commit before any currently uncommitted events were added.
        /// </remarks>
        public long CommitSequenceNumber { get; }

        /// <summary>
        /// Gets the sequence number of the latest event in the aggregate, including uncommitted events.
        /// </summary>
        public long EventSequenceNumber { get; }

        /// <summary>
        /// Gets the uncommitted events.
        /// </summary>
        public ImmutableArray<SerializedEvent> UncommittedEvents { get; }

        /// <summary>
        /// Gets the memento.
        /// </summary>
        public TMemento Memento { get; }

        /// <summary>
        /// Deserialize a serialized event.
        /// </summary>
        /// <typeparam name="TPayload">The type of the event payload.</typeparam>
        /// <param name="event">The serialized event.</param>
        /// <returns>An event with the appropriate payload.</returns>
        public Event<TPayload> Deserialize<TPayload>(SerializedEvent @event)
        {
            return EventSerializer.Deserialize<TPayload>(@event);
        }

        /// <summary>
        /// Applies the given event to the aggregate.
        /// </summary>
        /// <typeparam name="TPayload">The payload of the event to apply.</typeparam>
        /// <param name="host">The hosting aggregate.</param>
        /// <param name="event">The event to apply.</param>
        /// <remarks>
        /// This will be called when a new event has been created.
        /// </remarks>
        /// <returns>The aggregate with the event applied.</returns>
        public AggregateImplementationWithMemento<TAggregate, TMemento> ApplyEvent<TPayload>(in TAggregate host, in Event<TPayload> @event)
        {
            this.Validate(@event);

            // Update our memento if we want to.
            TMemento updatedMemento = host.ApplyEventToMemento(this.Memento, @event);

            // Add our uncommitted event
            SerializedEvent serializedEvent = EventSerializer.Serialize(@event);
            return new AggregateImplementationWithMemento<TAggregate, TMemento>(this.AggregateId, this.PartitionKey, this.CommitSequenceNumber, this.EventSequenceNumber + 1, this.UncommittedEvents.Add(serializedEvent), updatedMemento);
        }

        /// <summary>
        /// Creates the updated aggregate state with the new memento.
        /// </summary>
        /// <param name="memento">The updated memento.</param>
        /// <returns>The updated state.</returns>
        public AggregateImplementationWithMemento<TAggregate, TMemento> UpdateAfterApplyingSerializedEvent(TMemento memento)
        {
            return new AggregateImplementationWithMemento<TAggregate, TMemento>(this.AggregateId, this.PartitionKey, this.CommitSequenceNumber, this.EventSequenceNumber + 1, this.UncommittedEvents, memento);
        }

        /// <summary>
        /// Applies the given commits to the aggregate.
        /// </summary>
        /// <param name="host">The hosting aggregate.</param>
        /// <param name="commits">The ordered list of commits to apply to the aggregate.</param>
        /// <returns>The aggreagte with the commits applied.</returns>
        /// <remarks>
        /// This will be called when the aggregate is being rehydrated from committed events.
        /// </remarks>
        public AggregateImplementationWithMemento<TAggregate, TMemento> ApplyCommits(in TAggregate host, in IEnumerable<Commit> commits)
        {
            commits.ValidateCommits(this.AggregateId, this.CommitSequenceNumber, this.EventSequenceNumber);

            AggregateImplementationWithMemento<TAggregate, TMemento> implementation = this;

            foreach (Commit commit in commits)
            {
                foreach (SerializedEvent @event in commit.Events)
                {
                    implementation = host.ApplySerializedEvent(implementation, @event);
                }
            }

            return implementation;
        }

        /// <summary>
        /// Stores uncommitted events using the specified event writer.
        /// </summary>
        /// <typeparam name="TEventWriter">The type of event writer to use.</typeparam>
        /// <param name="writer">The event writer to use to store new events.</param>
        /// <returns>The aggregate with all new events committed.</returns>
        public async ValueTask<AggregateImplementationWithMemento<TAggregate, TMemento>> CommitAsync<TEventWriter>(TEventWriter writer)
            where TEventWriter : IEventWriter
        {
            if (this.UncommittedEvents.Length == 0)
            {
                return this;
            }

            await writer.WriteCommitAsync(new Commit(this.AggregateId, this.PartitionKey, this.CommitSequenceNumber + 1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), this.UncommittedEvents)).ConfigureAwait(false);
            return new AggregateImplementationWithMemento<TAggregate, TMemento>(this.AggregateId, this.PartitionKey, this.CommitSequenceNumber + 1, this.EventSequenceNumber, ImmutableArray<SerializedEvent>.Empty, this.Memento);
        }

        /// <summary>
        /// Stores a snapshot for the aggregate using the specified snapshot writer.
        /// </summary>
        /// <typeparam name="TSnapshotWriter">The type of snapshot writer to use.</typeparam>
        /// <param name="writer">The snapshot writer to use to store the new snapshot.</param>
        /// <returns>The aggregate once the new snapshot has been stored.</returns>
        public Task StoreSnapshotAsync<TSnapshotWriter>(TSnapshotWriter writer)
            where TSnapshotWriter : ISnapshotWriter
        {
            var snapshot = new Snapshot<TMemento>(this.AggregateId, this.CommitSequenceNumber, this.Memento);
            return writer.WriteAsync(SnapshotSerializer.Serialize(snapshot));
        }

        private void Validate<TPayload>(in Event<TPayload> @event)
        {
            if (@event.SequenceNumber != this.EventSequenceNumber + 1)
            {
                throw new ArgumentException($"The event sequence number was incorrect. Expected {this.EventSequenceNumber + 1}, actual {@event.SequenceNumber}");
            }
        }
    }
}
