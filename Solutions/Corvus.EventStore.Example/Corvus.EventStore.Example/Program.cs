// <copyright file="Program.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example
{
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
            var inMemoryEventStore = new InMemoryEventStore();
            var inMemorySnapshotStore = new InMemorySnapshotStore();

            AggregateReader<InMemoryEventReader, InMemorySnapshotReader> reader =
                InMemoryAggregateReader.GetInstance(inMemoryEventStore, inMemorySnapshotStore);

            Aggregate<ToDoList> aggregate = await ToDoList.Read(reader, "someid").ConfigureAwait(false);

            // Example 2: Retrieve an instance of an aggregate from the store. Do more things to it and save it again.
        }
    }
}
