// <copyright file="Program.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.InMemory.Aggregates;
    using Corvus.EventStore.InMemory.Core;
    using Corvus.EventStore.InMemory.Core.Internal;
    using Corvus.EventStore.InMemory.Snapshots;
    using Corvus.EventStore.InMemory.Snapshots.Internal;

    /// <summary>
    /// Main program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main()
        {
            // Example 1: Retrieve a new instance of an aggregate from the store. Do things to it and commit it.
            // Note: We never create an instance of an aggregate with 'new AggregateType()'. We always request them
            // from the store.

            // Configure the database (in this case a hand-rolled "in memory event store" database. But could be e.g. SQL, Cosmos, Table Storage
            var inMemoryEventStore = new InMemoryEventStore();
            var inMemorySnapshotStore = new InMemorySnapshotStore();

            // Create an aggregate reader for the configured store. This is cheap and can be done every time. It is stateless.
            AggregateReader<InMemoryEventReader, InMemorySnapshotReader> reader =
                InMemoryAggregateReader.GetInstance(inMemoryEventStore, inMemorySnapshotStore);

            // Read a todolist from the store.
            ToDoList toDoList = await ToDoList.Read(reader, Guid.Parse("2c46ed1c-474e-4c94-ac44-0570a46ceb30")).ConfigureAwait(false);

            string currentUser = "Bill Gates";

            // Create an aggregate writer for the configured store. This is cheap and can be done every time. It is stateless.
            AggregateWriter<InMemoryEventWriter, InMemorySnapshotWriter> writer =
                InMemoryAggregateWriter.GetInstance(inMemoryEventStore, inMemorySnapshotStore);

            // Use one of the todo-list services handy methods to do some work.
            toDoList = toDoList.Initialize(DateTimeOffset.Now, currentUser);

            // Commit whatever batch of events we had for that Initialize operation.
            toDoList = await toDoList.CommitAsync(writer).ConfigureAwait(false);

            toDoList.ToString();

            // Example 2: Retrieve an instance of an aggregate from the store. Do more things to it and save it again.
        }
    }
}
