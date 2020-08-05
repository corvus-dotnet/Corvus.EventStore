// <copyright file="ToDoItemRemovedEventPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example
{
    using System;

    /// <summary>
    /// An event payload for when a to do item is removed from a todolist.
    /// </summary>
    internal readonly struct ToDoItemRemovedEventPayload
    {
        /// <summary>
        /// The unique event type.
        /// </summary>
        public const string EventType = "corvus.event-store-example.to-do-item-removed";

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemRemovedEventPayload"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="ToDoItemId"/> of the item that was removed.</param>
        public ToDoItemRemovedEventPayload(Guid id)
        {
            this.ToDoItemId = id;
        }

        /// <summary>
        /// Gets the id of the item that was removed.
        /// </summary>
        public Guid ToDoItemId { get; }
    }
}
