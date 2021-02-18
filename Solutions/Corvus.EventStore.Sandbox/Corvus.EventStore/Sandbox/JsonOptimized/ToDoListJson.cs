// <copyright file="ToDoListJson.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Json;
    using Corvus.EventStore.Sandbox.Events;
    using Corvus.EventStore.Sandbox.Handlers;
    using Corvus.EventStore.Sandbox.Mementos;

    /// <summary>
    /// A todo list domain object implemented over an aggregate root that supports optimsed JSON reading and writing.
    /// </summary>
    /// <remarks>
    /// This implementation is aware of the target serializatin mechanism and takes advantage of the optimizations
    /// available for a <see cref="JsonAggregateRoot{TMemento}"/>.
    /// </remarks>
    public readonly struct ToDoListJson
    {
        private readonly JsonAggregateRoot<ToDoListMementoJson> aggregateRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoListJson"/> struct.
        /// </summary>
        /// <param name="aggregateRoot">The <see cref="JsonAggregateRoot{ToDoListMementoJson}"/> from which to initialize the todo list.</param>
        internal ToDoListJson(JsonAggregateRoot<ToDoListMementoJson> aggregateRoot)
        {
            this.aggregateRoot = aggregateRoot;
        }

        /// <summary>
        /// Gets the ID for the todo list.
        /// </summary>
        public Guid Id => this.aggregateRoot.Id;

        /// <summary>
        /// Gets the current internal commit sequence number.
        /// </summary>
        internal long CommitSequenceNumber => this.aggregateRoot.CommitSequenceNumber;

        /// <summary>
        /// Gets the current internal commit sequence number.
        /// </summary>
        internal long EventSequenceNumber => this.aggregateRoot.EventSequenceNumber;

        /// <summary>
        /// Read a to-do list from a JSON store.
        /// </summary>
        /// <typeparam name="TEventStore">The type of the event store.</typeparam>
        /// <param name="eventStore">The event store from which to load the todo list.</param>
        /// <param name="toDoListId">The ID for the todo list.</param>
        /// <returns>A <see cref="Task{ToDoList}"/> which, when complete, provides the <see cref="ToDoListJson"/>.</returns>
        public static async Task<ToDoListJson> ReadOrCreate<TEventStore>(TEventStore eventStore, Guid toDoListId)
            where TEventStore : IJsonEventStore
        {
            JsonAggregateRoot<ToDoListMementoJson> aggregateRoot = await eventStore.Read(toDoListId, ToDoListMementoJson.Empty, ToDoListJsonEventHandler.Instance).ConfigureAwait(false);
            return new ToDoListJson(aggregateRoot);
        }

        /// <summary>
        /// Fast path to create a to-do list from an event store.
        /// </summary>
        /// <typeparam name="TEventStore">The type of the event store.</typeparam>
        /// <param name="eventStore">The event store from which to load the todo list.</param>
        /// <param name="toDoListId">The ID for the todo list.</param>
        /// <returns>A <see cref="Task{ToDoList}"/> which, when complete, provides the <see cref="ToDoList"/>.</returns>
        public static ToDoListJson Create<TEventStore>(TEventStore eventStore, Guid toDoListId)
            where TEventStore : IJsonEventStore
        {
            JsonAggregateRoot<ToDoListMementoJson> aggregateRoot = eventStore.Create(toDoListId, ToDoListMementoJson.Empty, ToDoListJsonEventHandler.Instance);
            return new ToDoListJson(aggregateRoot);
        }

        /// <summary>
        /// Initialize the ToDo list with a start date and an owner.
        /// </summary>
        /// <param name="startDate">The starting date for items in the ToDo list.</param>
        /// <param name="owner">The owner of the ToDo list.</param>
        /// <returns>The updated <see cref="ToDoListJson"/>.</returns>
        public ToDoListJson Initialize(DateTimeOffset startDate, string owner)
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
        public ToDoListJson SetOwner(string owner)
        {
            return new ToDoListJson(
                this.aggregateRoot.ApplyEvent(
                        ToDoListOwnerSetEventJsonPayload.EncodedEventType,
                        new ToDoListOwnerSetEventJsonPayload(owner),
                        ToDoListOwnerSetEventJsonPayload.Converter,
                        ToDoListJsonEventHandler.Instance));
        }

        /// <summary>
        /// Sets the start date of the todolist.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <returns>A <see cref="ToDoList"/> with the start date updated.</returns>
        public ToDoListJson SetStartDate(DateTimeOffset startDate)
        {
            return new ToDoListJson(
                this.aggregateRoot.ApplyEvent(
                        ToDoListStartDateSetEventJsonPayload.EncodedEventType,
                        new ToDoListStartDateSetEventJsonPayload(startDate),
                        ToDoListStartDateSetEventJsonPayload.Converter,
                        ToDoListJsonEventHandler.Instance));
        }

        /// <summary>
        /// Adds an item to the todo list.
        /// </summary>
        /// <param name="id">The id of the item.</param>
        /// <param name="title">The title of the item.</param>
        /// <param name="description">The description of the item.</param>
        /// <returns>A <see cref="ToDoList"/> with the start date updated.</returns>
        public ToDoListJson AddToDoItem(Guid id, string title, string description)
        {
            if (this.aggregateRoot.Memento.ItemIds.Contains(id))
            {
                throw new InvalidOperationException($"The item with id {id} has already been added.");
            }

            return new ToDoListJson(
                this.aggregateRoot.ApplyEvent(
                        ToDoItemAddedEventJsonPayload.EncodedEventType,
                        new ToDoItemAddedEventJsonPayload(id, title, description),
                        ToDoItemAddedEventJsonPayload.Converter,
                        ToDoListJsonEventHandler.Instance));
        }

        /// <summary>
        /// Removes an item from the todo list.
        /// </summary>
        /// <param name="id">The id of the item.</param>
        /// <returns>A <see cref="ToDoList"/> with the start date updated.</returns>
        public ToDoListJson RemoveToDoItem(Guid id)
        {
            if (!this.aggregateRoot.Memento.ItemIds.Contains(id))
            {
                throw new InvalidOperationException($"The item with id {id} does not exist.");
            }

            return new ToDoListJson(
                this.aggregateRoot.ApplyEvent(
                        ToDoItemRemovedEventJsonPayload.EncodedEventType,
                        new ToDoItemRemovedEventJsonPayload(id),
                        ToDoItemRemovedEventJsonPayload.Converter,
                        ToDoListJsonEventHandler.Instance));
        }

        /// <summary>
        /// Commit the ToDo list.
        /// </summary>
        /// <returns>The committed <see cref="ToDoList"/>.</returns>
        public async Task<ToDoListJson> Commit()
        {
            // Note that we have a policy here that says "never create snapshots".
            return new ToDoListJson(await this.aggregateRoot.CommitJson().ConfigureAwait(false));
        }
    }
}
