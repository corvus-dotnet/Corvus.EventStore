// <copyright file="ToDoItemAddedEventPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example.Internal
{
    using System;

    /// <summary>
    /// An event payload for when a to do item is added to a todolist.
    /// </summary>
    internal readonly struct ToDoItemAddedEventPayload
    {
        /// <summary>
        /// The unique event type of this event.
        /// </summary>
        public const string EventType = "corvus.event-store-example.to-do-item-added";

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemAddedEventPayload"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ToDoItemId"/>.</param>
        /// <param name="title">The <see cref="Title"/>.</param>
        /// <param name="description">The <see cref="Description"/>.</param>
        public ToDoItemAddedEventPayload(Guid id, string title, string description)
        {
            this.ToDoItemId = id;
            this.Title = title;
            this.Description = description;
        }

        /// <summary>
        /// Gets the to do item ID.
        /// </summary>
        public Guid ToDoItemId { get; }

        /// <summary>
        /// Gets the title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; }
    }
}
