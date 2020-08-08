// <copyright file="Program.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Aggregates;
    using Corvus.EventStore.Azure.TableStorage.Aggregates;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Corvus.EventStore.Azure.TableStorage.Core;
    using Corvus.EventStore.Azure.TableStorage.Snapshots;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.InMemory.Aggregates;
    using Corvus.EventStore.InMemory.Core;
    using Corvus.EventStore.InMemory.Core.Internal;
    using Corvus.EventStore.InMemory.Snapshots;
    using Corvus.EventStore.InMemory.Snapshots.Internal;
    using Corvus.SnapshotStore.Azure.TableStorage.ContainerFactories;

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
            Console.WriteLine("Running in-memory.");
            await RunInMemoryAsync().ConfigureAwait(false);

            Console.WriteLine("Running with table storage.");
            await RunInMemoryAsync().ConfigureAwait(false);
        }

        private static async Task RunInMemoryAsync()
        {
            // Configure the database (in this case a hand-rolled "in memory event store" database. But could be e.g. SQL, Cosmos, Table Storage
            // This would typically be done while you are setting up the container
            var inMemoryEventStore = new InMemoryEventStore();
            var inMemorySnapshotStore = new InMemorySnapshotStore();

            // Example 1: Retrieve a new instance of an aggregate from the store. This type of aggregate is implemented over its own in-memory memento.
            // We Do things to it and commit it.

            // This is the ID of our aggregate - imagine this came in from the request, for example.
            string aggregateIdAsString = "2c46ed1c-474e-4c94-ac44-0570a46ceb30";
            var aggregateId = Guid.Parse(aggregateIdAsString);

            // Using the Id as the partition key.
            string partitionKey = aggregateIdAsString;

            // Create an aggregate reader for the configured store. This is cheap and can be done every time. It is stateless.
            // You would typically get this as a transient from the container. But as you can see you can just new everything up, too.
            // This happens to use an in memory reader for both the events and the snapshots, but you could (and frequently would) configure the AggregateReader with
            // different readers for events and snapshots so that they can go to different stores.
            AggregateReader<InMemoryEventReader, InMemorySnapshotReader> reader =
                InMemoryAggregateReader.GetInstance(inMemoryEventStore, inMemorySnapshotStore);

            // Read a todolist from the store. This type uses an aggregate in its implementation, and provides
            // a nice domain-specific facade over it.
            // Note: We never create an instance of an aggregate with 'new AggregateType()'. We always request them
            // from the store. Aggregates will typically have private constructors.
            // While you can hand-roll your own aggregates from scratch, it is usually better to use one of the precanned
            // implementation patterns, of which we have provided one in this demo (one that is implemented over an internal
            // memento which it uses to produce its snapshots).
            ToDoList toDoList = await ToDoList.ReadAsync(reader, aggregateId, partitionKey).ConfigureAwait(false);

            string currentUser = "Bill Gates";

            // Create an aggregate writer for the configured store. This is cheap and can be done every time. It is stateless.
            AggregateWriter<InMemoryEventWriter, InMemorySnapshotWriter> writer =
                InMemoryAggregateWriter.GetInstance(inMemoryEventStore, inMemorySnapshotStore);

            // Use one of the todo-list services handy methods to do some work.
            toDoList = toDoList.Initialize(DateTimeOffset.Now, currentUser);

            // Commit whatever batch of events we had for that Initialize operation.
            toDoList = await toDoList.CommitAsync(writer).ConfigureAwait(false);

            Console.WriteLine("Committed the initialization");

            // Example 2: Retrieve an instance of an aggregate from the store. Do more things to it and save it again.
            ToDoList toDoList2 = await ToDoList.ReadAsync(reader, aggregateId, partitionKey).ConfigureAwait(false);

            // Note that this is an atomic operation adding two items
            toDoList2 = toDoList2.AddToDoItem(Guid.NewGuid(), "This is my title", "This is my description");
            toDoList2 = toDoList2.AddToDoItem(Guid.NewGuid(), "Another day, another item", "This is the item in question");

            toDoList2 = await toDoList2.CommitAsync(writer).ConfigureAwait(false);

            Console.WriteLine("Committed having added a couple of items.");

            // Example 3: Just get it back again.
            ToDoList toDoList3 = await ToDoList.ReadAsync(reader, aggregateId, partitionKey).ConfigureAwait(false);

            Console.WriteLine("Committed having added a couple of items.");

            // Example 4: Concurrency issue
            // Now, if we try to update toDoList we will get a concurrency exception, as it has moved on since that instance
            // was created (i.e. someone else snuck in and updated it in another instance - in this case us!)
            toDoList = toDoList.AddToDoItem(Guid.NewGuid(), "Who has been eating *my* porridge?", "Bet it was that Goldilocks.");
            try
            {
                toDoList = await toDoList.CommitAsync(writer).ConfigureAwait(false);
            }
            catch (ConcurrencyException ex)
            {
                Console.WriteLine($"Bad luck - {ex.Message}");
            }

            // Example 5: Domain logic validation
            // Validation occurs in the domain layer (or e.g. the command handler if you were using commands instead of this domain object wrapper)
            // You inspect the current state of the system. Remember - you will get a concurrency exception on commit if someone has changed the
            // state beneath you and you have to get the aggregate back and start again, so this is perfectly safe!
            var newItemId = Guid.NewGuid();
            toDoList3 = toDoList3.AddToDoItem(newItemId, "You can add me once.", string.Empty);

            try
            {
                toDoList3 = toDoList3.AddToDoItem(newItemId, "But you can't add me twice.", string.Empty);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Oh dear - {ex.Message}");
            }
        }

        private static async Task RunWithTableStorageAsync()
        {
            // Configure the database (in this case a hand-rolled "in memory event store" database. But could be e.g. SQL, Cosmos, Table Storage
            // This would typically be done while you are setting up the container
            DevelopmentEventCloudTableFactory eventTableFactory = default;
            DevelopmentSnapshotCloudTableFactory snapshotTableFactory = default;

            // Example 1: Retrieve a new instance of an aggregate from the store. This type of aggregate is implemented over its own in-memory memento.
            // We Do things to it and commit it.

            // This is the ID of our aggregate - imagine this came in from the request, for example.
            string aggregateIdAsString = "2c46ed1c-474e-4c94-ac44-0570a46ceb30";
            var aggregateId = Guid.Parse(aggregateIdAsString);

            // Using the Id as the partition key.
            string partitionKey = aggregateIdAsString;

            // Create an aggregate reader for the configured store. This is cheap and can be done every time. It is stateless.
            // You would typically get this as a transient from the container. But as you can see you can just new everything up, too.
            // This happens to use an in memory reader for both the events and the snapshots, but you could (and frequently would) configure the AggregateReader with
            // different readers for events and snapshots so that they can go to different stores.
            AggregateReader<TableStorageEventReader, TableStorageSnapshotReader> reader =
                TableStorageAggregateReader.GetInstance(eventTableFactory, snapshotTableFactory);

            // Read a todolist from the store. This type uses an aggregate in its implementation, and provides
            // a nice domain-specific facade over it.
            // Note: We never create an instance of an aggregate with 'new AggregateType()'. We always request them
            // from the store. Aggregates will typically have private constructors.
            // While you can hand-roll your own aggregates from scratch, it is usually better to use one of the precanned
            // implementation patterns, of which we have provided one in this demo (one that is implemented over an internal
            // memento which it uses to produce its snapshots).
            ToDoList toDoList = await ToDoList.ReadAsync(reader, aggregateId, partitionKey).ConfigureAwait(false);

            string currentUser = "Bill Gates";

            // Create an aggregate writer for the configured store. This is cheap and can be done every time. It is stateless.
            AggregateWriter<TableStorageEventWriter, TableStorageSnapshotWriter> writer =
                TableStorageAggregateWriter.GetInstance(eventTableFactory, snapshotTableFactory);

            // Use one of the todo-list services handy methods to do some work.
            toDoList = toDoList.Initialize(DateTimeOffset.Now, currentUser);

            // Commit whatever batch of events we had for that Initialize operation.
            toDoList = await toDoList.CommitAsync(writer).ConfigureAwait(false);

            Console.WriteLine("Committed the initialization");

            // Example 2: Retrieve an instance of an aggregate from the store. Do more things to it and save it again.
            ToDoList toDoList2 = await ToDoList.ReadAsync(reader, aggregateId, partitionKey).ConfigureAwait(false);

            // Note that this is an atomic operation adding two items
            toDoList2 = toDoList2.AddToDoItem(Guid.NewGuid(), "This is my title", "This is my description");
            toDoList2 = toDoList2.AddToDoItem(Guid.NewGuid(), "Another day, another item", "This is the item in question");

            toDoList2 = await toDoList2.CommitAsync(writer).ConfigureAwait(false);

            Console.WriteLine("Committed having added a couple of items.");

            // Example 3: Just get it back again.
            ToDoList toDoList3 = await ToDoList.ReadAsync(reader, aggregateId, partitionKey).ConfigureAwait(false);

            Console.WriteLine("Committed having added a couple of items.");

            // Example 4: Concurrency issue
            // Now, if we try to update toDoList we will get a concurrency exception, as it has moved on since that instance
            // was created (i.e. someone else snuck in and updated it in another instance - in this case us!)
            toDoList = toDoList.AddToDoItem(Guid.NewGuid(), "Who has been eating *my* porridge?", "Bet it was that Goldilocks.");
            try
            {
                toDoList = await toDoList.CommitAsync(writer).ConfigureAwait(false);
            }
            catch (ConcurrencyException ex)
            {
                Console.WriteLine($"Bad luck - {ex.Message}");
            }

            // Example 5: Domain logic validation
            // Validation occurs in the domain layer (or e.g. the command handler if you were using commands instead of this domain object wrapper)
            // You inspect the current state of the system. Remember - you will get a concurrency exception on commit if someone has changed the
            // state beneath you and you have to get the aggregate back and start again, so this is perfectly safe!
            var newItemId = Guid.NewGuid();
            toDoList3 = toDoList3.AddToDoItem(newItemId, "You can add me once.", string.Empty);

            try
            {
                toDoList3 = toDoList3.AddToDoItem(newItemId, "But you can't add me twice.", string.Empty);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Oh dear - {ex.Message}");
            }
        }
    }
}
