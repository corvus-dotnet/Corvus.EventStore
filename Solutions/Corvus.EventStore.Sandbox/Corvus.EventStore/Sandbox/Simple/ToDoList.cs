// <copyright file="ToDoList.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Sandbox.Events;
    using Corvus.EventStore.Sandbox.Handlers;
    using Corvus.EventStore.Sandbox.Mementos;

    /// <summary>
    /// A todo list domain object implemented over an aggregate root.
    /// </summary>
    public class ToDoList
    {
        private readonly IAggregateRoot<ToDoListMemento> aggregateRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoList"/> class.
        /// </summary>
        /// <param name="aggregateRoot">The <see cref="IAggregateRoot{TMemento}"/> root from which to initialize the todo list.</param>
        internal ToDoList(IAggregateRoot<ToDoListMemento> aggregateRoot)
        {
            this.aggregateRoot = aggregateRoot;
        }

        /// <summary>
        /// Gets the ID for the todo list.
        /// </summary>
        public Guid Id => this.aggregateRoot.Id;

        /// <summary>
        /// Read a to-do list from a cosmos store.
        /// </summary>
        /// <param name="eventStore">The event store from which to load the todo list.</param>
        /// <param name="toDoListId">The ID for the todo list.</param>
        /// <returns>A <see cref="Task{ToDoList}"/> which, when complete, provides the <see cref="ToDoList"/>.</returns>
        public static async Task<ToDoList> ReadOrCreate(IEventStore eventStore, Guid toDoListId)
        {
            IAggregateRoot<ToDoListMemento> aggregateRoot = await eventStore.Read(toDoListId, ToDoListMemento.Empty, ToDoListEventHandler.Instance).ConfigureAwait(false);
            return new ToDoList(aggregateRoot);
        }

        /// <summary>
        /// Fast path to create a to-do list from an event store.
        /// </summary>
        /// <param name="eventStore">The event store from which to load the todo list.</param>
        /// <param name="toDoListId">The ID for the todo list.</param>
        /// <returns>A <see cref="Task{ToDoList}"/> which, when complete, provides the <see cref="ToDoList"/>.</returns>
        public static ToDoList Create(IEventStore eventStore, Guid toDoListId)
        {
            IAggregateRoot<ToDoListMemento> aggregateRoot = eventStore.Create(toDoListId, ToDoListMemento.Empty, ToDoListEventHandler.Instance);
            return new ToDoList(aggregateRoot);
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
                this.aggregateRoot.ApplyEvent(
                        ToDoListOwnerSetEventPayload.EventType,
                        new ToDoListOwnerSetEventPayload(owner),
                        ToDoListEventHandler.Instance));
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
                this.aggregateRoot.ApplyEvent(
                        ToDoListStartDateSetEventPayload.EventType,
                        new ToDoListStartDateSetEventPayload(startDate),
                        ToDoListEventHandler.Instance));
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
            if (this.aggregateRoot.Memento.ItemIds.Contains(id))
            {
                throw new InvalidOperationException($"The item with id {id} has already been added.");
            }

            // Apply an event to add an item
            // Then apply an event to set the owner
            return new ToDoList(
                this.aggregateRoot.ApplyEvent(
                        ToDoItemAddedEventPayload.EventType,
                        new ToDoItemAddedEventPayload(id, title, description),
                        ToDoListEventHandler.Instance));
        }

        /// <summary>
        /// Removes an item from the todo list.
        /// </summary>
        /// <param name="id">The id of the item.</param>
        /// <returns>A <see cref="ToDoList"/> with the start date updated.</returns>
        public ToDoList RemoveToDoItem(Guid id)
        {
            if (!this.aggregateRoot.Memento.ItemIds.Contains(id))
            {
                throw new InvalidOperationException($"The item with id {id} does not exist.");
            }

            // Apply an event to add an item
            // Then apply an event to set the owner
            return new ToDoList(
                this.aggregateRoot.ApplyEvent(
                        ToDoItemRemovedEventPayload.EventType,
                        new ToDoItemRemovedEventPayload(id),
                        ToDoListEventHandler.Instance));
        }

        /// <summary>
        /// Commit the ToDo list.
        /// </summary>
        /// <returns>The committed <see cref="ToDoList"/>.</returns>
        public async Task<ToDoList> Commit()
        {
            // Note that we have a policy here that says "never create snapshots".
            return new ToDoList(await this.aggregateRoot.Commit().ConfigureAwait(false));
        }
    }
}
