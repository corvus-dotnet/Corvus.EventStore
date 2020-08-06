// <copyright file="ToDoListMemento.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example.Internal
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// A memento for the current state of a TodoList object.
    /// </summary>
    internal readonly struct ToDoListMemento
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoListMemento"/> struct.
        /// </summary>
        /// <param name="items">The <see cref="Items"/>.</param>
        /// <param name="owner">The <see cref="Owner"/>.</param>
        /// <param name="startDate">The <see cref="StartDate"/>.</param>
        public ToDoListMemento(ImmutableDictionary<Guid, ToDoItemMemento> items, string owner, DateTimeOffset startDate)
        {
            this.Items = items;
            this.Owner = owner;
            this.StartDate = startDate;
        }

        /// <summary>
        /// Gets the array of to-do items currently in the list.
        /// </summary>
        public ImmutableDictionary<Guid, ToDoItemMemento> Items { get; }

        /// <summary>
        /// Gets the owner of the list.
        /// </summary>
        public string Owner { get; }

        /// <summary>
        /// Gets the start date for the list.
        /// </summary>
        /// <remarks>What's this for? Who knows - just an example property, really.</remarks>
        public DateTimeOffset StartDate { get; }

        /// <summary>
        /// Constructs a memento with the given item added to the list.
        /// </summary>
        /// <param name="payload">The event payload describing the item that was added.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the item added.</returns>
        public ToDoListMemento With(ToDoItemAddedEventPayload payload)
        {
            return new ToDoListMemento(this.GetOrCreateItems().Add(payload.ToDoItemId, new ToDoItemMemento(payload.ToDoItemId, payload.Title)), this.Owner, this.StartDate);
        }

        /// <summary>
        /// Constructs a memento with the given item removed from the list.
        /// </summary>
        /// <param name="payload">The event payload describing the item that was removed.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the item removed.</returns>
        public ToDoListMemento With(ToDoItemRemovedEventPayload payload)
        {
            return new ToDoListMemento(this.GetOrCreateItems().Remove(payload.ToDoItemId), this.Owner, this.StartDate);
        }

        /// <summary>
        /// Constructs a memento with the owner set.
        /// </summary>
        /// <param name="payload">The event payload describing owner that was set.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the owner set.</returns>
        public ToDoListMemento With(ToDoListOwnerSetEventPayload payload)
        {
            return new ToDoListMemento(this.GetOrCreateItems(), payload.Owner, this.StartDate);
        }

        /// <summary>
        /// Constructs a memento with the owner set.
        /// </summary>
        /// <param name="payload">The event payload describing owner that was set.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the owner set.</returns>
        public ToDoListMemento With(ToDoListStartDateSetEventPayload payload)
        {
            return new ToDoListMemento(this.GetOrCreateItems(), this.Owner, payload.StartDate);
        }

        private ImmutableDictionary<Guid, ToDoItemMemento> GetOrCreateItems()
        {
            return this.Items ?? ImmutableDictionary<Guid, ToDoItemMemento>.Empty;
        }
    }
}
