// <copyright file="ToDoItem.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example
{
    using System;

    /// <summary>
    /// An item in the list maintained by the <see cref="ToDoListMemento"/>.
    /// </summary>
    internal readonly struct ToDoItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItem"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="Id"/>.</param>
        /// <param name="title">The <see cref="Title"/>.</param>
        public ToDoItem(Guid id, string title)
        {
            this.Id = id;
            this.Title = title;
        }

        /// <summary>
        /// Gets the ID of the to do item.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the title of the to do item.
        /// </summary>
        public string Title { get; }
    }
}
