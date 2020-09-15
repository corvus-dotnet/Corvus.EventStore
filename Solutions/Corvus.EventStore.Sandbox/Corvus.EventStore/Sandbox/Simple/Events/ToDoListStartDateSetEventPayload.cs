// <copyright file="ToDoListStartDateSetEventPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox.Events
{
    using System;

    /// <summary>
    /// An event payload for when the start date of the todo list is set.
    /// </summary>
    /// <remarks>
    /// Note that this is not immutable to support serialization.
    /// </remarks>
    internal class ToDoListStartDateSetEventPayload
    {
        /// <summary>
        /// The unique event type of this event.
        /// </summary>
        public const string EventType = "corvus.event-store-example.to-do-list-start-date-set";

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoListStartDateSetEventPayload"/> class.
        /// </summary>
        public ToDoListStartDateSetEventPayload()
            : this(DateTimeOffset.MinValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemAddedEventPayload"/> struct.
        /// </summary>
        /// <param name="startDate">The <see cref="StartDate"/>.</param>
        public ToDoListStartDateSetEventPayload(DateTimeOffset startDate)
        {
            this.StartDate = startDate;
        }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public DateTimeOffset StartDate { get; set; }
    }
}
