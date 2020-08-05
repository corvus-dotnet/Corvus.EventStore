// <copyright file="ToDoListMemento.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example
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
        public ToDoListMemento(ImmutableDictionary<Guid, ToDoItem> items)
        {
            this.Items = items;
        }

        /// <summary>
        /// Gets the array of to-do items currently in the list.
        /// </summary>
        public ImmutableDictionary<Guid, ToDoItem> Items { get; }

        /// <summary>
        /// Constructs a memento with the given item added to the list.
        /// </summary>
        /// <param name="payload">The event payload describing the item that was added.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the item added.</returns>
        public ToDoListMemento With(ToDoItemAddedEventPayload payload)
        {
            return new ToDoListMemento(this.GetOrCreateItems().Add(payload.ToDoItemId, new ToDoItem(payload.ToDoItemId, payload.Title)));
        }

        /// <summary>
        /// Constructs a memento with the given item revmoed from the list.
        /// </summary>
        /// <param name="payload">The event payload describing the item that was removed.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the item removed.</returns>
        public ToDoListMemento With(ToDoItemRemovedEventPayload payload)
        {
            return new ToDoListMemento(this.GetOrCreateItems().Remove(payload.ToDoItemId));
        }

        private ImmutableDictionary<Guid, ToDoItem> GetOrCreateItems()
        {
            return this.Items ?? ImmutableDictionary<Guid, ToDoItem>.Empty;
        }
    }
}
