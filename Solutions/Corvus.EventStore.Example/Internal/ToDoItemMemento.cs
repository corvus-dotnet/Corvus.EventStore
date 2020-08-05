// <copyright file="ToDoItemMemento.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example
{
    using System;

    /// <summary>
    /// An item in the list maintained by the <see cref="ToDoListMemento"/>.
    /// </summary>
    internal readonly struct ToDoItemMemento
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToDoItemMemento"/> struct.
        /// </summary>
        /// <param name="id">The <see cref="Id"/>.</param>
        /// <param name="title">The <see cref="Title"/>.</param>
        public ToDoItemMemento(Guid id, string title)
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
