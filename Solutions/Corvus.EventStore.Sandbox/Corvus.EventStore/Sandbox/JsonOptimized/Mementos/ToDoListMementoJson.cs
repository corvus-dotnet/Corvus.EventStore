// <copyright file="ToDoListMementoJson.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox.Mementos
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// A memento for the current state of a TodoList object, optimized for Json reading and writing.
    /// </summary>
    public readonly struct ToDoListMementoJson
    {
        /// <summary>
        /// Gets a default, empty to do list memento.
        /// </summary>
        internal static readonly ToDoListMementoJson Empty = new ToDoListMementoJson(ImmutableArray<Guid>.Empty, string.Empty, DateTimeOffset.MinValue);

        private readonly ImmutableArray<Guid> itemIds;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoListMementoJson"/> struct.
        /// </summary>
        /// <param name="itemIds">The <see cref="ItemIds"/>.</param>
        /// <param name="owner">The <see cref="Owner"/>.</param>
        /// <param name="startDate">The <see cref="StartDate"/>.</param>
        internal ToDoListMementoJson(ImmutableArray<Guid> itemIds, string owner, DateTimeOffset startDate)
        {
            this.itemIds = itemIds;
            this.Owner = owner;
            this.StartDate = startDate;
        }

        /// <summary>
        /// Gets the array of IDs of the to-do items currently in the list.
        /// </summary>
        /// <remarks>This illustrates that the memento only needs enough state for the domain logic to do its job.</remarks>
        internal ImmutableArray<Guid> ItemIds => this.GetOrCreateItems();

        /// <summary>
        /// Gets the owner of the list.
        /// </summary>
        internal string Owner { get; }

        /// <summary>
        /// Gets the start date for the list.
        /// </summary>
        /// <remarks>What's this for? Who knows - just an example property, really.</remarks>
        internal DateTimeOffset StartDate { get; }

        /// <summary>
        /// Constructs a memento with the given item added to the list.
        /// </summary>
        /// <param name="toDoItemId">The item that was added.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the item added.</returns>
        internal ToDoListMementoJson WithToDoItemAdded(Guid toDoItemId)
        {
            return new ToDoListMementoJson(this.ItemIds.Add(toDoItemId), this.Owner, this.StartDate);
        }

        /// <summary>
        /// Constructs a memento with the given item removed from the list.
        /// </summary>
        /// <param name="toDoItemId">The item that was removed.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the item removed.</returns>
        internal ToDoListMementoJson WithToDoItemRemoved(Guid toDoItemId)
        {
            return new ToDoListMementoJson(this.ItemIds.Remove(toDoItemId), this.Owner, this.StartDate);
        }

        /// <summary>
        /// Constructs a memento with the owner set.
        /// </summary>
        /// <param name="owner">The owner that was set.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the owner set.</returns>
        internal ToDoListMementoJson WithOwner(string owner)
        {
            return new ToDoListMementoJson(this.ItemIds, owner, this.StartDate);
        }

        /// <summary>
        /// Constructs a memento with the owner set.
        /// </summary>
        /// <param name="startDate">The new start date.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the owner set.</returns>
        internal ToDoListMementoJson WithStartDate(DateTimeOffset startDate)
        {
            return new ToDoListMementoJson(this.ItemIds, this.Owner, startDate);
        }

        private ImmutableArray<Guid> GetOrCreateItems()
        {
            return this.itemIds.IsDefault ? ImmutableArray<Guid>.Empty : this.itemIds;
        }
    }
}
