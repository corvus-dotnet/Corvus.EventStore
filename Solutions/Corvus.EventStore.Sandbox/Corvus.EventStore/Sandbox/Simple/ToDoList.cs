// <copyright file="ToDoList.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.AzureCosmos;
    using Corvus.EventStore.Json;
    using Corvus.EventStore.Sandbox.Handlers;
    using Corvus.EventStore.Sandbox.Mementos;

    /// <summary>
    /// Helper methods to load a todo list from a specific type of aggregate root.
    /// </summary>
    public static class ToDoList
    {
        /// <summary>
        /// Read a to-do list from a cosmos store.
        /// </summary>
        /// <typeparam name="TContainerFactory">The type of the cosmos container factory.</typeparam>
        /// <typeparam name="TSnapshotReader">The type of the snapshot reader.</typeparam>
        /// <param name="cosmosEventStore">The event store from which to load the todo list.</param>
        /// <param name="toDoListId">The ID for the todo list.</param>
        /// <returns>A <see cref="Task{ToDoList}"/> which, when complete, provides the <see cref="ToDoList{TAggregateRoot}"/>.</returns>
        public static async Task<ToDoList<JsonAggregateRoot<ToDoListMemento, CosmosJsonStore>>> ReadOrCreate<TContainerFactory, TSnapshotReader>(CosmosEventStore<TContainerFactory, TSnapshotReader> cosmosEventStore, Guid toDoListId)
            where TContainerFactory : IContainerFactory
            where TSnapshotReader : ISnapshotReader
        {
            JsonAggregateRoot<ToDoListMemento, CosmosJsonStore> aggregateRoot = await cosmosEventStore.Read(toDoListId, toDoListId.ToString(), ToDoListMemento.Empty, ToDoListEventHandler.Instance).ConfigureAwait(false);
            return new ToDoList<JsonAggregateRoot<ToDoListMemento, CosmosJsonStore>>(aggregateRoot);
        }
    }
}
