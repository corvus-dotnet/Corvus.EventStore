// <copyright file="ToDoList{TAggregateRoot}.cs" company="Endjin Limited">
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
    /// <typeparam name="TAggregateRoot">The type of the aggregate root for this to do list.</typeparam>
    /// <remarks>
    /// This uses the generic <see cref="IAggregateRoot{TMemento, T}"/> and has no special optimizations
    /// for the target serialization mechanism.
    /// </remarks>
    public class ToDoList<TAggregateRoot>
        where TAggregateRoot : IAggregateRoot<ToDoListMemento, TAggregateRoot>
    {
        private readonly TAggregateRoot aggregateRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoList{TAggregateRoot}"/> struct.
        /// </summary>
        /// <param name="aggregateRoot">The <see cref="IAggregateRoot{TMemento, T}"/> root from which to initialize the todo list.</param>
        internal ToDoList(TAggregateRoot aggregateRoot)
        {
            this.aggregateRoot = aggregateRoot;
        }

        /// <summary>
        /// Gets the ID for the todo list.
        /// </summary>
        public Guid Id => this.aggregateRoot.Id;

        /// <summary>
        /// Initialize the ToDo list with a start date and an owner.
        /// </summary>
        /// <param name="startDate">The starting date for items in the ToDo list.</param>
        /// <param name="owner">The owner of the ToDo list.</param>
        /// <returns>The updated <see cref="ToDoList{TAggregateRoot}"/>.</returns>
        public ToDoList<TAggregateRoot> Initialize(DateTimeOffset startDate, string owner)
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
        public ToDoList<TAggregateRoot> SetOwner(string owner)
        {
            // Then apply an event to set the owner
            return new ToDoList<TAggregateRoot>(
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
        public ToDoList<TAggregateRoot> SetStartDate(DateTimeOffset startDate)
        {
            // Apply an event to set the start date
            // Then apply an event to set the owner
            return new ToDoList<TAggregateRoot>(
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
        public ToDoList<TAggregateRoot> AddToDoItem(Guid id, string title, string description)
        {
            if (this.aggregateRoot.Memento.ItemIds.Contains(id))
            {
                throw new InvalidOperationException($"The item with id {id} has already been added.");
            }

            // Apply an event to add an item
            // Then apply an event to set the owner
            return new ToDoList<TAggregateRoot>(
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
        public ToDoList<TAggregateRoot> RemoveToDoItem(Guid id)
        {
            if (!this.aggregateRoot.Memento.ItemIds.Contains(id))
            {
                throw new InvalidOperationException($"The item with id {id} does not exist.");
            }

            // Apply an event to add an item
            // Then apply an event to set the owner
            return new ToDoList<TAggregateRoot>(
                this.aggregateRoot.ApplyEvent(
                        ToDoItemRemovedEventPayload.EventType,
                        new ToDoItemRemovedEventPayload(id),
                        ToDoListEventHandler.Instance));
        }

        /// <summary>
        /// Commit the ToDo list.
        /// </summary>
        /// <returns>The committed <see cref="ToDoList"/>.</returns>
        public async Task<ToDoList<TAggregateRoot>> Commit()
        {
            // Note that we have a policy here that says "never create snapshots".
            return new ToDoList<TAggregateRoot>(await this.aggregateRoot.Commit().ConfigureAwait(false));
        }
    }
}
