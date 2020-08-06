// <copyright file="ToDoListStartDateSetEventPayload.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example.Internal
{
    using System;

    /// <summary>
    /// An event payload for when the start date of the todo list is set.
    /// </summary>
    internal readonly struct ToDoListStartDateSetEventPayload
    {
        /// <summary>
        /// The unique event type of this event.
        /// </summary>
        public const string EventType = "corvus.event-store-example.to-do-list-start-date-set";

        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemAddedEventPayload"/> struct.
        /// </summary>
        /// <param name="startDate">The <see cref="StartDate"/>.</param>
        public ToDoListStartDateSetEventPayload(DateTimeOffset startDate)
        {
            this.StartDate = startDate;
        }

        /// <summary>
        /// Gets the owner.
        /// </summary>
        public DateTimeOffset StartDate { get; }
    }
}
