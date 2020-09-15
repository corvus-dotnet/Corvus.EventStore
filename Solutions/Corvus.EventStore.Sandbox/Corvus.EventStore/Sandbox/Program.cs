// <copyright file="Program.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Corvus.EventStore.AzureCosmos;
    using Corvus.EventStore.Sandbox.Simple.Handlers;
    using Microsoft.Extensions.Configuration;

    using TDL = ToDoList<Json.JsonAggregateRoot<Mementos.ToDoListMemento, AzureCosmos.CosmosJsonStore>>;
    using TDLJ = ToDoListJson<Json.JsonAggregateRoot<Mementos.ToDoListMementoJson, AzureCosmos.CosmosJsonStore>>;

    /// <summary>
    /// Main program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Program arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Running with Cosmos in Azure");
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddJsonFile($"local.settings.json", true, true);

            IConfigurationRoot config = builder.Build();

            string connectionstring = config.GetConnectionString("CosmosConnectionString");

            var eventStore = CosmosEventStore.GetInstance(connectionstring, "corvuseventstore", "corvusevents", NullSnapshotReader.Instance);

            var cts = new CancellationTokenSource();
            var eventFeedHandler = new ToDoListEventFeedHandler();
            Task feedTask = eventStore.ReadFeed(eventFeedHandler, 100, null, cts.Token);

            var toDoListId = Guid.NewGuid();

            // Read and modify a todoitem using the simple aggregate.
            TDL toDoList = await ToDoList.ReadOrCreate(eventStore, toDoListId).ConfigureAwait(false);

            toDoList = toDoList.Initialize(DateTimeOffset.Now, "Bill Gates");
            toDoList = await toDoList.Commit().ConfigureAwait(false);

            // Now load it using the "json-specific" aggregate.
            TDLJ toDoListJson = await ToDoListJson.ReadOrCreate(eventStore, toDoListId).ConfigureAwait(false);
            toDoListJson = toDoListJson.AddToDoItem(Guid.NewGuid(), "This is my title", "This is my description");
            toDoListJson = toDoListJson.AddToDoItem(Guid.NewGuid(), "This is my second title", "This is my second description");
            toDoListJson = await toDoListJson.Commit().ConfigureAwait(false);

            // Reload the toDoList
            toDoList = await ToDoList.ReadOrCreate(eventStore, toDoListId).ConfigureAwait(false);
            toDoList = toDoList.AddToDoItem(Guid.NewGuid(), "This is another title", "This is another description");
            toDoList = toDoList.AddToDoItem(Guid.NewGuid(), "This is yet another title", "This is yet another description");
            toDoList = await toDoList.Commit().ConfigureAwait(false);

            // Reload the toDoListJson
            toDoListJson = await ToDoListJson.ReadOrCreate(eventStore, toDoListId).ConfigureAwait(false);

            Console.ReadKey();

            cts.Cancel();
            await feedTask.ConfigureAwait(false);

            Console.WriteLine($"Seen {eventFeedHandler.TotalCommitCount} commits containing {eventFeedHandler.TotalEventCount} events.");
            ////await WriteVolume(connectionstring).ConfigureAwait(false);
        }

        private static async Task WriteVolume(string connectionString)
        {
            var eventStore = CosmosEventStore.GetInstance(connectionString, "corvuseventstore", "corvusevents", NullSnapshotReader.Instance);

            // This is the ID of our aggregate - imagine this came in from the request, for example.
            Guid[] aggregateIds = Enumerable.Range(0, 650)
                .Select(i => Guid.NewGuid())
                .ToArray();

            const int batchSize = 50;
            const int initializationBatchSize = 10;
            const int iterations = 50;
            const int nodesPerAggregate = 8;
            const int minTimePerIteration = 1000;
            const int eventsPerCommit = 8;

            var aggregates = new TDLJ[aggregateIds.Length];

            Console.WriteLine("Initializing aggregates.");

            var initTaskList = new Task<TDLJ>[initializationBatchSize];

            var loadSw = Stopwatch.StartNew();
            for (int i = 0; i < (int)Math.Ceiling((double)aggregateIds.Length / initializationBatchSize); ++i)
            {
                for (int index = 0; index < initializationBatchSize; ++index)
                {
                    Guid id = aggregateIds[(initializationBatchSize * i) + index];
                    initTaskList[index] = ToDoListJson.ReadOrCreate(eventStore, id);
                }

                TDLJ[] results = await Task.WhenAll(initTaskList).ConfigureAwait(false);
                results.CopyTo(aggregates, initializationBatchSize * i);
            }

            loadSw.Stop();

            Console.WriteLine($"Readed {aggregateIds.Length} aggregates in {loadSw.ElapsedMilliseconds / 1000.0} seconds ({aggregateIds.Length / (loadSw.ElapsedMilliseconds / 1000.0)} agg/sec)");

            Console.WriteLine();

            var executeSw = Stopwatch.StartNew();

            var taskList = new Task<TDLJ>[batchSize];

            for (int i = 0; i < iterations; ++i)
            {
                Console.WriteLine($"Iteration {i}");
                for (int batch = 0; batch < aggregateIds.Length / batchSize; ++batch)
                {
                    DateTimeOffset startTime = DateTimeOffset.Now;

                    Console.WriteLine($"Batch {batch}");

                    var sw = Stopwatch.StartNew();

                    for (int node = 0; node < nodesPerAggregate; ++node)
                    {
                        for (int taskCount = 0; taskCount < batchSize; ++taskCount)
                        {
                            TDLJ toDoList = aggregates[(batch * batchSize) + taskCount];
                            for (int eventCount = 0; eventCount < eventsPerCommit; ++eventCount)
                            {
                                toDoList = toDoList.AddToDoItem(Guid.NewGuid(), "This is my title", "This is my description");
                            }

                            taskList[taskCount] = toDoList.Commit();
                        }

                        TDLJ[] batchAggregates = await Task.WhenAll(taskList).ConfigureAwait(false);
                        batchAggregates.CopyTo(aggregates, batch * batchSize);
                    }

                    sw.Stop();

                    Console.WriteLine($"{batchSize * nodesPerAggregate} commits in {sw.ElapsedMilliseconds}, ({(batchSize * nodesPerAggregate) / (sw.ElapsedMilliseconds / 1000.0)} commits/s)");
                    Console.WriteLine($"{(batchSize * nodesPerAggregate * eventsPerCommit) / (sw.ElapsedMilliseconds / 1000.0)} events/s");

                    double elapsed = (DateTimeOffset.Now - startTime).TotalMilliseconds;

                    // Rate limit
                    if (elapsed < minTimePerIteration)
                    {
                        int delay = minTimePerIteration - (int)elapsed;
                        Console.WriteLine($"Delaying {delay}ms");

                        await Task.Delay(delay).ConfigureAwait(false);
                    }
                }
            }

            executeSw.Stop();
            Console.WriteLine($"Executed {aggregateIds.Length * iterations * nodesPerAggregate} atomic commits in {executeSw.ElapsedMilliseconds / 1000.0} seconds ({aggregateIds.Length * iterations * nodesPerAggregate / (executeSw.ElapsedMilliseconds / 1000.0)} ) commits/sec");
            Console.WriteLine($"({aggregateIds.Length * iterations * nodesPerAggregate * eventsPerCommit / (executeSw.ElapsedMilliseconds / 1000.0)} ) events/sec");
        }
    }
}
