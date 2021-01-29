// <copyright file="ToDoItemRemovedEventPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox.Events
{
    using System;

    /// <summary>
    /// An event payload for when a to do item is removed from a todolist.
    /// </summary>
    /// <remarks>Note that this is not immutable to support serialization.</remarks>
    internal class ToDoItemRemovedEventPayload
    {
        /// <summary>
        /// The unique event type.
        /// </summary>
        public const string EventType = "corvus.event-store-example.to-do-item-removed";

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemRemovedEventPayload"/> class.
        /// </summary>
        public ToDoItemRemovedEventPayload()
            : this(Guid.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemRemovedEventPayload"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="Id"/> of the item that was removed.</param>
        public ToDoItemRemovedEventPayload(Guid id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Gets or sets the id of the item that was removed.
        /// </summary>
        public Guid Id { get; set; }
    }
}
