// <copyright file="ToDoList.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.Example.Internal.EventHandlers;
    using Corvus.EventStore.Example.Internal.Events;
    using Corvus.EventStore.Example.Internal.Mementos;

    /// <summary>
    /// A to do list backed by an aggregate root.
    /// </summary>
    public readonly struct ToDoList
    {
        // We tune this based on our item event size
        private const int MaxItemsPerBatch = 100;

        private readonly AggregateWithMemento<ToDoListEventHandler, ToDoListMemento> aggregate;

        private ToDoList(AggregateWithMemento<ToDoListEventHandler, ToDoListMemento> aggregate)
        {
            this.aggregate = aggregate;
        }

        /// <summary>
        /// Reads an instance of a to-do list, optionally to the specified commit sequence number.
        /// </summary>
        /// <typeparam name="TReader">The type of the <see cref="IAggregateReader"/>.</typeparam>
        /// <param name="reader">The reader from which to read the aggregate.</param>
        /// <param name="aggregateId">The id of the aggregate to read.</param>
        /// <param name="partitionKey">The partition key of the aggregate to read.</param>
        /// <param name="commitSequenceNumber">The (optional) commit sequence number at which to read the aggregate.</param>
        /// <returns>A <see cref="ValueTask"/> which completes with the to do list.</returns>
        public static async ValueTask<ToDoList> ReadAsync<TReader>(TReader reader, Guid aggregateId, string partitionKey, long commitSequenceNumber = long.MaxValue)
            where TReader : IAggregateReader
        {
            AggregateWithMemento<ToDoListEventHandler, ToDoListMemento> aggregate = await AggregateWithMemento<ToDoListEventHandler, ToDoListMemento>.ReadAsync(reader, aggregateId, partitionKey, MaxItemsPerBatch, commitSequenceNumber).ConfigureAwait(false);
            return new ToDoList(aggregate);
        }

        /// <summary>
        /// Initialize the ToDo list with a start date and an owner.
        /// </summary>
        /// <param name="startDate">The starting date for items in the ToDo list.</param>
        /// <param name="owner">The owner of the ToDo list.</param>
        /// <returns>The updated <see cref="ToDoList"/>.</returns>
        public ToDoList Initialize(DateTimeOffset startDate, string owner)
        {
            // Apply an event to set the start date
            // Then apply an event to set the owner
            return this.SetStartDate(startDate)
                       .SetOwner(owner);
        }

        /// <summary>
        /// Sets the owner of the to do list.
        /// </summary>
        /// <param name="owner">The name of the owner.</param>
        /// <returns>A <see cref="ToDoList"/> with the name updated.</returns>
        public ToDoList SetOwner(string owner)
        {
            // Then apply an event to set the owner
            return new ToDoList(
                this.aggregate.ApplyEvent(
                    new Event<ToDoListOwnerSetEventPayload>(
                        ToDoListOwnerSetEventPayload.EventType,
                        this.aggregate.EventSequenceNumber + 1,
                        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        new ToDoListOwnerSetEventPayload(owner))));
        }

        /// <summary>
        /// Sets the start date of the todolist.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <returns>A <see cref="ToDoList"/> with the start date updated.</returns>
        public ToDoList SetStartDate(DateTimeOffset startDate)
        {
            // Apply an event to set the start date
            // Then apply an event to set the owner
            return new ToDoList(
                this.aggregate.ApplyEvent(
                    new Event<ToDoListStartDateSetEventPayload>(
                        ToDoListStartDateSetEventPayload.EventType,
                        this.aggregate.EventSequenceNumber + 1,
                        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        new ToDoListStartDateSetEventPayload(startDate))));
        }

        /// <summary>
        /// Adds an item to the todo list.
        /// </summary>
        /// <param name="id">The id of the item.</param>
        /// <param name="title">The title of the item.</param>
        /// <param name="description">The description of the item.</param>
        /// <returns>A <see cref="ToDoList"/> with the start date updated.</returns>
        public ToDoList AddToDoItem(Guid id, string title, string description)
        {
            if (this.aggregate.Memento.Items.ContainsKey(id))
            {
                throw new InvalidOperationException($"The item with id {id} has already been added.");
            }

            // Apply an event to add an item
            // Then apply an event to set the owner
            return new ToDoList(
                this.aggregate.ApplyEvent(
                    new Event<ToDoItemAddedEventPayload>(
                        ToDoItemAddedEventPayload.EventType,
                        this.aggregate.EventSequenceNumber + 1,
                        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        new ToDoItemAddedEventPayload(id, title, description))));
        }

        /// <summary>
        /// Removes an item from the todo list.
        /// </summary>
        /// <param name="id">The id of the item.</param>
        /// <returns>A <see cref="ToDoList"/> with the start date updated.</returns>
        public ToDoList RemoveToDoItem(Guid id)
        {
            if (!this.aggregate.Memento.Items.ContainsKey(id))
            {
                throw new InvalidOperationException($"The item with id {id} does not exist.");
            }

            // Apply an event to add an item
            // Then apply an event to set the owner
            return new ToDoList(
                this.aggregate.ApplyEvent(
                    new Event<ToDoItemRemovedEventPayload>(
                        ToDoItemRemovedEventPayload.EventType,
                        this.aggregate.EventSequenceNumber + 1,
                        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        new ToDoItemRemovedEventPayload(id))));
        }

        /// <summary>
        /// Commit the ToDo list.
        /// </summary>
        /// <typeparam name="TWriter">The type of the writer to which to commit the todo list.</typeparam>
        /// <param name="writer">The writer to which to commit the todo list.</param>
        /// <returns>The committed <see cref="ToDoList"/>.</returns>
        public async ValueTask<ToDoList> CommitAsync<TWriter>(TWriter writer)
            where TWriter : IAggregateWriter
        {
            // Note that we have a policy here that says "never create snapshots".
            return new ToDoList(
                await writer.CommitAsync(
                    this.aggregate,
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    NeverSnapshotPolicy<AggregateWithMemento<ToDoListEventHandler, ToDoListMemento>>.Instance).ConfigureAwait(false));
        }
    }
}
