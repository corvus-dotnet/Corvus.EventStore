// <copyright file="ToDoListJson{TAggregateRoot}.cs" company="Endjin Limited">
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
    /// <typeparam name="TAggregateRoot">The type of the aggregate root for this to do list.</typeparam>
    /// <remarks>
    /// This implementation is aware of the target serializatin mechanism and takes advantage of the optimizations
    /// available for a <see cref="IJsonAggregateRoot{TMemento, T}"/>.
    /// </remarks>
    public readonly struct ToDoListJson<TAggregateRoot>
        where TAggregateRoot : IJsonAggregateRoot<ToDoListMementoJson, TAggregateRoot>
    {
        private readonly TAggregateRoot aggregateRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoList{TAggregateRoot}"/> struct.
        /// </summary>
        /// <param name="aggregateRoot">The <see cref="IAggregateRoot{TMemento, T}"/> root from which to initialize the todo list.</param>
        internal ToDoListJson(TAggregateRoot aggregateRoot)
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
        public ToDoListJson<TAggregateRoot> Initialize(DateTimeOffset startDate, string owner)
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
        public ToDoListJson<TAggregateRoot> SetOwner(string owner)
        {
            return new ToDoListJson<TAggregateRoot>(
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
        public ToDoListJson<TAggregateRoot> SetStartDate(DateTimeOffset startDate)
        {
            return new ToDoListJson<TAggregateRoot>(
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
        public ToDoListJson<TAggregateRoot> AddToDoItem(Guid id, string title, string description)
        {
            if (this.aggregateRoot.Memento.ItemIds.Contains(id))
            {
                throw new InvalidOperationException($"The item with id {id} has already been added.");
            }

            return new ToDoListJson<TAggregateRoot>(
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
        public ToDoListJson<TAggregateRoot> RemoveToDoItem(Guid id)
        {
            if (!this.aggregateRoot.Memento.ItemIds.Contains(id))
            {
                throw new InvalidOperationException($"The item with id {id} does not exist.");
            }

            return new ToDoListJson<TAggregateRoot>(
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
        public async Task<ToDoListJson<TAggregateRoot>> Commit()
        {
            // Note that we have a policy here that says "never create snapshots".
            return new ToDoListJson<TAggregateRoot>(await this.aggregateRoot.Commit().ConfigureAwait(false));
        }
    }
}
