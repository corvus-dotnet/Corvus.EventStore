// <copyright file="ToDoListMemento.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox.Mementos
{
    using System;
    using System.Collections.Generic;
    using Corvus.EventStore.Sandbox.Events;

    /// <summary>
    /// A memento for the current state of a TodoList object.
    /// </summary>
    /// <remarks>Note that this isn't immutable so we can support out-of-the-box System.Text.Json serialization. It illustrates operation on a POCO. Could be a class or a struct.</remarks>
    public class ToDoListMemento
    {
        /// <summary>
        /// Gets a default, empty to do list memento.
        /// </summary>
        internal static readonly ToDoListMemento Empty = new ToDoListMemento(new List<Guid>(), string.Empty, DateTimeOffset.MinValue);

        private List<Guid> itemIds;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoListMemento"/> class.
        /// </summary>
        public ToDoListMemento()
            : this(new List<Guid>(), string.Empty, DateTimeOffset.MinValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoListMemento"/> struct.
        /// </summary>
        /// <param name="itemIds">The <see cref="ItemIds"/>.</param>
        /// <param name="owner">The <see cref="Owner"/>.</param>
        /// <param name="startDate">The <see cref="StartDate"/>.</param>
        internal ToDoListMemento(List<Guid> itemIds, string owner, DateTimeOffset startDate)
        {
            this.itemIds = itemIds;
            this.Owner = owner;
            this.StartDate = startDate;
        }

        /// <summary>
        /// Gets or sets the array of IDs of the to-do items currently in the list.
        /// </summary>
        /// <remarks>This illustrates that the memento only needs enough state for the domain logic to do its job.</remarks>
        internal List<Guid> ItemIds
        {
            get => this.GetOrCreateItems();
            set => this.itemIds = value;
        }

        /// <summary>
        /// Gets or sets the owner of the list.
        /// </summary>
        internal string Owner { get; set;  }

        /// <summary>
        /// Gets or sets the start date for the list.
        /// </summary>
        /// <remarks>What's this for? Who knows - just an example property, really.</remarks>
        internal DateTimeOffset StartDate { get; set;  }

        /// <summary>
        /// Constructs a memento with the given item added to the list.
        /// </summary>
        /// <param name="payload">The event payload describing the item that was added.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the item added.</returns>
        internal ToDoListMemento With(ToDoItemAddedEventPayload payload)
        {
            var list = new List<Guid>(this.ItemIds)
            {
                payload.Id,
            };
            return new ToDoListMemento(list, this.Owner, this.StartDate);
        }

        /// <summary>
        /// Constructs a memento with the given item removed from the list.
        /// </summary>
        /// <param name="payload">The event payload describing the item that was removed.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the item removed.</returns>
        internal ToDoListMemento With(ToDoItemRemovedEventPayload payload)
        {
            var list = new List<Guid>(this.ItemIds);
            list.Remove(payload.Id);
            return new ToDoListMemento(list, this.Owner, this.StartDate);
        }

        /// <summary>
        /// Constructs a memento with the owner set.
        /// </summary>
        /// <param name="payload">The event payload describing owner that was set.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the owner set.</returns>
        internal ToDoListMemento With(ToDoListOwnerSetEventPayload payload)
        {
            return new ToDoListMemento(this.ItemIds, payload.Owner, this.StartDate);
        }

        /// <summary>
        /// Constructs a memento with the owner set.
        /// </summary>
        /// <param name="payload">The event payload describing owner that was set.</param>
        /// <returns>A <see cref="ToDoListMemento"/> with the owner set.</returns>
        internal ToDoListMemento With(ToDoListStartDateSetEventPayload payload)
        {
            return new ToDoListMemento(this.ItemIds, this.Owner, payload.StartDate);
        }

        private List<Guid> GetOrCreateItems()
        {
            return this.itemIds ??= new List<Guid>();
        }
    }
}
