﻿// <copyright file="ToDoListOwnerSetEventPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox.Events
{
    /// <summary>
    /// An event payload for when the owner of the todo list is set.
    /// </summary>
    /// <remarks>Note that this is not immutable to support serialization.</remarks>
    internal class ToDoListOwnerSetEventPayload
    {
        /// <summary>
        /// The unique event type of this event.
        /// </summary>
        public const string EventType = "corvus.event-store-example.to-do-list-owner-set";

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoListOwnerSetEventPayload"/> class.
        /// </summary>
        public ToDoListOwnerSetEventPayload()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemAddedEventPayload"/> struct.
        /// </summary>
        /// <param name="owner">The <see cref="Owner"/>.</param>
        public ToDoListOwnerSetEventPayload(string owner)
        {
            this.Owner = owner;
        }

        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        public string Owner { get; set; }
    }
}
