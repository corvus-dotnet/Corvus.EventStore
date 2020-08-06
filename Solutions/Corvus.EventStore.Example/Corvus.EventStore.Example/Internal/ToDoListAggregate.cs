// <copyright file="ToDoListAggregate.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Example.Internal;
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// The aggregate root for a ToDo list containing ToDo items.
    /// </summary>
    internal readonly struct ToDoListAggregate : IAggregateImplementationWithMementoHost<ToDoListAggregate, ToDoListMemento>
    {
        private readonly AggregateImplementationWithMemento<ToDoListAggregate, ToDoListMemento> state;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoListAggregate"/> struct.
        /// </summary>
        /// <param name="state">The current state of the aggregate.</param>
        private ToDoListAggregate(in AggregateImplementationWithMemento<ToDoListAggregate, ToDoListMemento> state)
        {
            this.state = state;
        }

        /// <inheritdoc/>
        public string AggregateId => this.state.AggregateId;

        /// <inheritdoc/>
        public string PartitionKey => this.state.PartitionKey;

        /// <inheritdoc/>
        public long CommitSequenceNumber => this.state.CommitSequenceNumber;

        /// <inheritdoc/>
        public long EventSequenceNumber => this.state.EventSequenceNumber;

        /// <summary>
        /// Reads an instance of an aggregate, optionally to the specified commit sequence number.
        /// </summary>
        /// <typeparam name="TReader">The type of the <see cref="IAggregateReader"/>.</typeparam>
        /// <param name="reader">The reader from which to read the aggregate.</param>
        /// <param name="aggregateId">The id of the aggregate to read.</param>
        /// <param name="commitSequenceNumber">The (optional) commit sequence number at which to read the aggregate.</param>
        /// <returns>A <see cref="ValueTask"/> which completes with the aggregate.</returns>
        public static ValueTask<ToDoListAggregate> Read<TReader>(TReader reader, string aggregateId, long commitSequenceNumber = long.MaxValue)
            where TReader : IAggregateReader
        {
            return reader.ReadAsync(s => CreateFrom(s), aggregateId, commitSequenceNumber);
        }

        /// <inheritdoc/>
        public ToDoListAggregate ApplyEvent<TPayload>(in Event<TPayload> @event)
        {
            return new ToDoListAggregate(this.state.ApplyEvent(this, @event));
        }

        /// <inheritdoc/>
        public ToDoListAggregate ApplyCommits(in IEnumerable<Commit> commits)
        {
            return new ToDoListAggregate(this.state.ApplyCommits(this, commits));
        }

        /// <inheritdoc/>
        public async ValueTask<ToDoListAggregate> CommitAsync<TEventWriter>(TEventWriter writer)
            where TEventWriter : IEventWriter
        {
            return new ToDoListAggregate(await this.state.CommitAsync(writer).ConfigureAwait(false));
        }

        /// <inheritdoc/>
        public Task StoreSnapshotAsync<TSnapshotWriter>(TSnapshotWriter writer)
            where TSnapshotWriter : ISnapshotWriter
        {
            return this.state.StoreSnapshotAsync(writer);
        }

        /// <inheritdoc/>
        public ToDoListMemento ApplyEventToMemento<TPayload>(in ToDoListMemento memento, in Event<TPayload> @event)
        {
            return @event switch
            {
                Event<ToDoItemAddedEventPayload> tdia => memento.With(tdia.Payload),
                Event<ToDoItemRemovedEventPayload> tdir => memento.With(tdir.Payload),
                _ => throw new InvalidOperationException($"The event type of {@event.EventType} for the event for the aggregate with ID {this.AggregateId} with event sequence number {@event.SequenceNumber} was not recognized."),
            };
        }

        /// <inheritdoc/>
        public AggregateImplementationWithMemento<ToDoListAggregate, ToDoListMemento> ApplySerializedEvent(in AggregateImplementationWithMemento<ToDoListAggregate, ToDoListMemento> implementation, in SerializedEvent @event)
        {
            return @event.EventType switch
            {
                ToDoItemAddedEventPayload.EventType => this.HandleToDoItemAdded(implementation, @event),
                ToDoItemRemovedEventPayload.EventType => this.HandleToDoItemRemoved(implementation, @event),
                _ => throw new InvalidOperationException($"The event with sequence number {@event.SequenceNumber} had event type {@event.EventType} which was not recognized as a valid event type for the ToDoListAggregate."),
            };
        }

        private static ToDoListAggregate CreateFrom(SerializedSnapshot s)
        {
            return new ToDoListAggregate(AggregateImplementationWithMemento<ToDoListAggregate, ToDoListMemento>.CreateFrom(s));
        }

        private AggregateImplementationWithMemento<ToDoListAggregate, ToDoListMemento> HandleToDoItemAdded(in AggregateImplementationWithMemento<ToDoListAggregate, ToDoListMemento> state, in SerializedEvent @event)
        {
            return state.UpdateAfterApplyingSerializedEvent(state.Memento.With(state.Deserialize<ToDoItemAddedEventPayload>(@event).Payload));
        }

        private AggregateImplementationWithMemento<ToDoListAggregate, ToDoListMemento> HandleToDoItemRemoved(in AggregateImplementationWithMemento<ToDoListAggregate, ToDoListMemento> state, in SerializedEvent @event)
        {
            return state.UpdateAfterApplyingSerializedEvent(state.Memento.With(state.Deserialize<ToDoItemRemovedEventPayload>(@event).Payload));
        }
    }
}
