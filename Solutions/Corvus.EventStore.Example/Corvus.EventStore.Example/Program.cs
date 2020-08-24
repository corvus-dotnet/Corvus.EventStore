// <copyright file="Program.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Example
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
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
    using Corvus.Extensions;
    using Corvus.SnapshotStore.Azure.TableStorage.ContainerFactories;
    using Microsoft.Extensions.Configuration;

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
            ////Console.WriteLine("Running in-memory.");
            ////await RunInMemoryAsync().ConfigureAwait(false);

            ////Console.WriteLine("Running in memory with multiple iterations");
            ////await RunInMemoryMultipleIterationsAsync().ConfigureAwait(false);

            ////Console.WriteLine("Running with table storage.");
            ////await RunWithTableStorageAsync().ConfigureAwait(false);

            ////Console.WriteLine("Running with multi-partition table storage");
            ////await RunWithMultiPartitionTableStorageAsync(false).ConfigureAwait(false);

            Console.WriteLine("Running with multi-partition table storage in Azure");

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddJsonFile($"local.settings.json", true, true);

            IConfigurationRoot config = builder.Build();

            await RunWithMultiPartitionTableStorageInAzureAsync(config.GetConnectionString("TableStorageConnectionString"), config.GetConnectionString("TableStorageConnectionStringAllStream"), true, true).ConfigureAwait(false);

            Console.ReadKey();
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

            var eventFeedCancellationTokenSource = new CancellationTokenSource();
            Task eventFeedTask = StartEventFeed(inMemoryEventStore, eventFeedCancellationTokenSource.Token);

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

            Console.ReadKey();

            try
            {
                eventFeedCancellationTokenSource.Cancel();
                await eventFeedTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // The task cancelled exception.
            }
        }

        private static async Task RunWithTableStorageAsync()
        {
            // Configure the database (in this case our cloud table factories)
            // This would typically be done while you are setting up the container
            var eventTableFactory = new DevelopmentEventCloudTableFactory("corvusevents");
            var snapshotTableFactory = new DevelopmentSnapshotCloudTableFactory("corvussnapshots");

            // Example 1: Retrieve a new instance of an aggregate from the store. This type of aggregate is implemented over its own in-memory memento.
            // We Do things to it and commit it.

            // This is the ID of our aggregate - imagine this came in from the request, for example.
            var aggregateId = Guid.NewGuid();
            string aggregateIdAsString = aggregateId.ToString();

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

        private static async Task RunWithMultiPartitionTableStorageAsync(bool writeMore = true)
        {
            // Configure the database (in this case our cloud table factories)
            // This would typically be done while you are setting up the container
            // Here, we are setting up two separate physical partitions for the storage
            // (in a real implementation this would typically be in two entirely different storage accounts to improve throughput)
            var eventTableFactoryP1 = new DevelopmentEventCloudTableFactory("corvusevents1");
            var eventTableFactoryP2 = new DevelopmentEventCloudTableFactory("corvusevents2");
            var eventTableFactory = new PartitionedEventCloudTableFactory<DevelopmentEventCloudTableFactory>(eventTableFactoryP1, eventTableFactoryP2);
            var snapshotTableFactory = new DevelopmentSnapshotCloudTableFactory("corvussnapshots");
            var allStreamTableFactory = new DevelopmentAllStreamCloudTableFactory("corvusallstream");

            var eventMerger = TableStorageEventMerger.From(eventTableFactory, allStreamTableFactory);

            try
            {
                if (writeMore)
                {
                    // Example 1: Retrieve a new instance of an aggregate from the store. This type of aggregate is implemented over its own in-memory memento.
                    // We Do things to it and commit it.

                    // Create an aggregate reader for the configured store. This is cheap and can be done every time. It is stateless.
                    // You would typically get this as a transient from the container. But as you can see you can just new everything up, too.
                    // This happens to use an in memory reader for both the events and the snapshots, but you could (and frequently would) configure the AggregateReader with
                    // different readers for events and snapshots so that they can go to different stores.
                    AggregateReader<TableStorageEventReader, TableStorageSnapshotReader> reader =
                        TableStorageAggregateReader.GetInstance(eventTableFactory, snapshotTableFactory);

                    // Create an aggregate writer for the configured store. This is cheap and can be done every time. It is stateless.
                    AggregateWriter<TableStorageEventWriter, TableStorageSnapshotWriter> writer =
                        TableStorageAggregateWriter.GetInstance(eventTableFactory, snapshotTableFactory);

                    // This is the ID of our aggregate - imagine this came in from the request, for example.
                    Guid[] aggregateIds = Enumerable.Range(0, 100)
                        .Select(i => Guid.NewGuid())
                        .ToArray();

                    const int batchSize = 10;
                    const int initializationBatchSize = 10;
                    const int iterations = 2;

                    var aggregates = new ToDoList[aggregateIds.Length];

                    Console.Write("Initializing aggregates");

                    var loadSw = Stopwatch.StartNew();

                    for (int i = 0; i < (int)Math.Ceiling((double)aggregateIds.Length / initializationBatchSize); ++i)
                    {
                        IEnumerable<int> range = Enumerable.Range(initializationBatchSize * i, Math.Min(initializationBatchSize, aggregates.Length - (initializationBatchSize * i)));
                        ToDoList[] results = await Task.WhenAll(range.Select(index => ToDoList.ReadAsync(reader, aggregateIds[index], aggregateIds[index].ToString()).AsTask()).ToList()).ConfigureAwait(false);
                        results.CopyTo(aggregates, range.First());
                        Console.Write(".");
                    }

                    loadSw.Stop();
                    Console.WriteLine($"Loaded in {loadSw.ElapsedMilliseconds / 1000.0} seconds");

                    Console.WriteLine();

                    var executeSw = Stopwatch.StartNew();

                    for (int i = 0; i < iterations; ++i)
                    {
                        Console.WriteLine($"Iteration {i}");
                        for (int batch = 0; batch < aggregateIds.Length / batchSize; ++batch)
                        {
                            var sw = Stopwatch.StartNew();
                            Console.WriteLine($"Batch {batch}");

                            var taskList = new List<Task<ToDoList>>();

                            for (int taskCount = 0; taskCount < batchSize; ++taskCount)
                            {
                                ToDoList toDoList = aggregates[(batch * batchSize) + taskCount];
                                toDoList = toDoList.AddToDoItem(Guid.NewGuid(), "This is my title", "This is my description");
                                toDoList = toDoList.AddToDoItem(Guid.NewGuid(), "Another day, another item", "This is the item in question");
                                ValueTask<ToDoList> task = toDoList.CommitAsync(writer);
                                taskList.Add(task.AsTask());
                            }

                            ToDoList[] batchAggregates = await Task.WhenAll(taskList).ConfigureAwait(false);
                            batchAggregates.CopyTo(aggregates, batch * batchSize);
                            sw.Stop();
                            Console.WriteLine(sw.ElapsedMilliseconds);
                        }
                    }

                    executeSw.Stop();
                    Console.WriteLine($"Executed {aggregateIds.Length * iterations} atomic commits in {executeSw.ElapsedMilliseconds / 1000.0} seconds");
                }

                Console.ReadKey();
            }
            finally
            {
                await eventMerger.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static async Task RunWithMultiPartitionTableStorageInAzureAsync(string connectionString, string allStreamConnectionString, bool writeMore = true, bool startEventMerger = true)
        {
            // Configure the database (in this case our cloud table factories)
            // This would typically be done while you are setting up the container
            // Here, we are setting up two separate physical partitions for the storage
            // (in a real implementation this would typically be in two entirely different storage accounts to improve throughput)
            var eventTableFactoryP1 = new EventCloudTableFactory(connectionString, "mwacorvusevents1");
            var eventTableFactoryP2 = new EventCloudTableFactory(connectionString, "mwacorvusevents2");
            var eventTableFactoryP3 = new EventCloudTableFactory(connectionString, "mwacorvusevents3");
            var eventTableFactoryP4 = new EventCloudTableFactory(connectionString, "mwacorvusevents4");
            var eventTableFactoryP5 = new EventCloudTableFactory(connectionString, "mwacorvusevents5");
            var eventTableFactoryP6 = new EventCloudTableFactory(connectionString, "mwacorvusevents6");
            var eventTableFactoryP7 = new EventCloudTableFactory(connectionString, "mwacorvusevents7");
            var eventTableFactoryP8 = new EventCloudTableFactory(connectionString, "mwacorvusevents8");
            var eventTableFactoryP9 = new EventCloudTableFactory(connectionString, "mwacorvusevents9");
            var eventTableFactoryP10 = new EventCloudTableFactory(connectionString, "mwacorvusevents10");
            var eventTableFactory = new PartitionedEventCloudTableFactory<EventCloudTableFactory>(eventTableFactoryP1, eventTableFactoryP2, eventTableFactoryP3, eventTableFactoryP4, eventTableFactoryP5, eventTableFactoryP6, eventTableFactoryP7, eventTableFactoryP8, eventTableFactoryP9, eventTableFactoryP10);
            var snapshotTableFactory = new SnapshotCloudTableFactory(connectionString, "mwacorvussnapshots");
            var allStreamTableFactory = new AllStreamCloudTableFactory(allStreamConnectionString, "mwacorvusallstream");

            // Now were done, start the merger, just to see whether that works.
            TableStorageEventMerger<PartitionedEventCloudTableFactory<EventCloudTableFactory>, AllStreamCloudTableFactory>? eventMerger = null;

            if (startEventMerger)
            {
                eventMerger = TableStorageEventMerger.From(eventTableFactory, allStreamTableFactory);
            }

            try
            {
                if (writeMore)
                {
                    // Example 1: Retrieve a new instance of an aggregate from the store. This type of aggregate is implemented over its own in-memory memento.
                    // We Do things to it and commit it.

                    // Create an aggregate reader for the configured store. This is cheap and can be done every time. It is stateless.
                    // You would typically get this as a transient from the container. But as you can see you can just new everything up, too.
                    // This happens to use an in memory reader for both the events and the snapshots, but you could (and frequently would) configure the AggregateReader with
                    // different readers for events and snapshots so that they can go to different stores.
                    AggregateReader<TableStorageEventReader, TableStorageSnapshotReader> reader =
                        TableStorageAggregateReader.GetInstance(eventTableFactory, snapshotTableFactory);

                    // Create an aggregate writer for the configured store. This is cheap and can be done every time. It is stateless.
                    AggregateWriter<TableStorageEventWriter, TableStorageSnapshotWriter> writer =
                        TableStorageAggregateWriter.GetInstance(eventTableFactory, snapshotTableFactory);

                    // This is the ID of our aggregate - imagine this came in from the request, for example.
                    Guid[] aggregateIds = Enumerable.Range(0, 625)
                        .Select(i => Guid.NewGuid())
                        .ToArray();

                    const int batchSize = 625;
                    const int initializationBatchSize = 625;
                    const int iterations = 50;
                    const int nodesPerAggregate = 8;
                    const int maxRatePerNode = 1000;
                    const int eventsPerCommit = 8;

                    var aggregates = new ToDoList[aggregateIds.Length];

                    Console.WriteLine("Initializing aggregates.");

                    var initTaskList = new Task<ToDoList>[initializationBatchSize];

                    var loadSw = Stopwatch.StartNew();
                    for (int i = 0; i < (int)Math.Ceiling((double)aggregateIds.Length / initializationBatchSize); ++i)
                    {
                        for (int index = 0; index < initializationBatchSize; ++index)
                        {
                            Guid id = aggregateIds[(initializationBatchSize * i) + index];
                            initTaskList[index] = ToDoList.ReadAsync(reader, id, id.ToString()).AsTask();
                        }

                        ToDoList[] results = await Task.WhenAll(initTaskList).ConfigureAwait(false);
                        results.CopyTo(aggregates, initializationBatchSize * i);
                    }

                    loadSw.Stop();

                    Console.WriteLine($"Loaded {aggregateIds.Length} aggregates in {loadSw.ElapsedMilliseconds / 1000.0} seconds ({aggregateIds.Length / (loadSw.ElapsedMilliseconds / 1000.0)} agg/sec)");

                    Console.WriteLine();

                    var executeSw = Stopwatch.StartNew();

                    var taskList = new Task<ToDoList>[batchSize];

                    for (int i = 0; i < iterations; ++i)
                    {
                        DateTimeOffset startTime = DateTimeOffset.Now;

                        Console.WriteLine($"Iteration {i}");
                        for (int batch = 0; batch < aggregateIds.Length / batchSize; ++batch)
                        {
                            Console.WriteLine($"Batch {batch}");

                            var sw = Stopwatch.StartNew();

                            for (int node = 0; node < nodesPerAggregate; ++node)
                            {
                                for (int taskCount = 0; taskCount < batchSize; ++taskCount)
                                {
                                    ToDoList toDoList = aggregates[(batch * batchSize) + taskCount];
                                    for (int eventCount = 0; eventCount < eventsPerCommit; ++eventCount)
                                    {
                                        toDoList = toDoList.AddToDoItem(Guid.NewGuid(), "This is my title", "This is my description");
                                    }

                                    ValueTask<ToDoList> task = toDoList.CommitAsync(writer);
                                    taskList[taskCount] = task.AsTask();
                                }

                                ToDoList[] batchAggregates = await Task.WhenAll(taskList).ConfigureAwait(false);
                                batchAggregates.CopyTo(aggregates, batch * batchSize);
                            }

                            sw.Stop();

                            Console.WriteLine($"{aggregateIds.Length * nodesPerAggregate} commits in {sw.ElapsedMilliseconds}, ({(aggregateIds.Length * nodesPerAggregate) / (sw.ElapsedMilliseconds / 1000.0)} commits/s)");
                            Console.WriteLine($"{(aggregateIds.Length * nodesPerAggregate * eventsPerCommit) / (sw.ElapsedMilliseconds / 1000.0)} events/s");
                        }

                        double elapsed = (DateTimeOffset.Now - startTime).TotalMilliseconds;

                        // Rate limit to ~1 per second per node
                        if (elapsed < maxRatePerNode)
                        {
                            int delay = maxRatePerNode - (int)elapsed;
                            Console.WriteLine($"Delaying {delay}ms");

                            await Task.Delay(delay).ConfigureAwait(false);
                        }
                    }

                    executeSw.Stop();
                    Console.WriteLine($"Executed {aggregateIds.Length * iterations * nodesPerAggregate} atomic commits in {executeSw.ElapsedMilliseconds / 1000.0} seconds ({aggregateIds.Length * iterations * nodesPerAggregate / (executeSw.ElapsedMilliseconds / 1000.0)} ) commits/sec");
                    Console.WriteLine($"({aggregateIds.Length * iterations * nodesPerAggregate * eventsPerCommit / (executeSw.ElapsedMilliseconds / 1000.0)} ) events/sec");
                }
            }
            finally
            {
                Console.ReadKey();

                if (eventMerger.HasValue)
                {
                    try
                    {
                        await eventMerger.Value.DisposeAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }

        private static async Task RunInMemoryMultipleIterationsAsync()
        {
            // Configure the database (in this case a hand-rolled "in memory event store" database. But could be e.g. SQL, Cosmos, Table Storage
            // This would typically be done while you are setting up the container
            var inMemoryEventStore = new InMemoryEventStore();
            var inMemorySnapshotStore = new InMemorySnapshotStore();

            AggregateReader<InMemoryEventReader, InMemorySnapshotReader> reader =
                InMemoryAggregateReader.GetInstance(inMemoryEventStore, inMemorySnapshotStore);

            // Create an aggregate writer for the configured store. This is cheap and can be done every time. It is stateless.
            AggregateWriter<InMemoryEventWriter, InMemorySnapshotWriter> writer =
                InMemoryAggregateWriter.GetInstance(inMemoryEventStore, inMemorySnapshotStore);

            const int aggregateCount = 1000000;
            Guid[] aggregateIds = Enumerable.Range(0, aggregateCount)
                .Select(i => Guid.NewGuid())
                .ToArray();

            var aggregates = new ToDoList[aggregateIds.Length];

            Console.Write("Initializing aggregates");

            await aggregateIds.ForEachAtIndexAsync(async (aggregateId, index) =>
            {
                if (index % (aggregateCount / 100) == 0)
                {
                    Console.Write($".");
                }

                aggregates[index] = await ToDoList.ReadAsync(reader, aggregateId, aggregateId.ToString()).ConfigureAwait(false);
            }).ConfigureAwait(false);

            Console.WriteLine();

            var eventFeedCancellationTokenSource = new CancellationTokenSource();

            const int batchSize = 10000;
            const int iterationCount = 2;

            Task eventFeedTask = StartEventFeed(inMemoryEventStore, eventFeedCancellationTokenSource.Token, false, aggregateCount * iterationCount);

            var sw1 = Stopwatch.StartNew();
            for (int i = 0; i < iterationCount; ++i)
            {
                Console.WriteLine($"Iteration {i}");
                for (int batch = 0; batch < aggregateIds.Length / batchSize; ++batch)
                {
                    Console.WriteLine($"Batch {batch}");

                    var sw = Stopwatch.StartNew();

                    for (int taskCount = 0; taskCount < batchSize; ++taskCount)
                    {
                        ToDoList toDoList = aggregates[(batch * batchSize) + taskCount];
                        toDoList = toDoList.AddToDoItem(Guid.NewGuid(), "This is my title", "This is my description");
                        toDoList = toDoList.AddToDoItem(Guid.NewGuid(), "Another day, another item", "This is the item in question");
                        ValueTask<ToDoList> task = toDoList.CommitAsync(writer);
                        if (!task.IsCompleted)
                        {
                            aggregates[(batch * batchSize) + taskCount] = await task.ConfigureAwait(false);
                        }
                        else
                        {
                            aggregates[(batch * batchSize) + taskCount] = task.Result;
                        }
                    }

                    sw.Stop();
                    Console.WriteLine(sw.ElapsedMilliseconds);
                }
            }

            sw1.Stop();

            var swOuter = Stopwatch.StartNew();

            await eventFeedTask.ConfigureAwait(false);

            swOuter.Stop();

            Console.WriteLine($"Wrote the events in {sw1.ElapsedMilliseconds / 1000.0} seconds.");
            Console.WriteLine($"The event feed caught up and stopped after a further {swOuter.ElapsedMilliseconds / 1000.0} seconds.");
        }

        private static Task StartEventFeed(InMemoryEventStore inMemoryEventStore, CancellationToken token, bool writeEvents = true, int targetCommitCount = int.MaxValue)
        {
            return Task.Factory.StartNew(
                async () =>
                {
                    int commitCount = 0;
                    int eventCount = 0;
                    InMemoryEventFeed? eventFeed = null;
                    var sw = Stopwatch.StartNew();
                    try
                    {
                        eventFeed = new InMemoryEventFeed(inMemoryEventStore);

                        // Get the events 1000 sat a time without a filter
                        EventFeedResult result = await eventFeed.Get(default, 10000).ConfigureAwait(false);

                        while (commitCount < targetCommitCount)
                        {
                            try
                            {
                                int currentCommitCount = commitCount;

                                foreach (Commit @commit in result.Commits)
                                {
                                    commitCount += 1;
                                    foreach (SerializedEvent @event in commit.Events)
                                    {
                                        eventCount += 1;
                                        if (writeEvents)
                                        {
                                            //// Process the result
                                            Console.ForegroundColor = ConsoleColor.Cyan;
                                            Console.WriteLine($"Seen event {@event.EventType}");
                                            Console.ResetColor();
                                        }
                                    }
                                }

                                if (currentCommitCount != commitCount)
                                {
                                    Console.WriteLine($"Commits: {commitCount}");
                                }

                                token.ThrowIfCancellationRequested();

                                result = await eventFeed.Get(result.Checkpoint).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                throw;
                            }
                        }
                    }
                    finally
                    {
                        sw.Stop();
                        Console.WriteLine($"Seen {eventCount:N0} events in {commitCount:N0} commits in {sw.ElapsedMilliseconds / 1000.0} seconds");

                        if (eventFeed != null)
                        {
                            await eventFeed.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                });
        }
    }
}
