// <copyright file="Program.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.EventStore.AzureBlob;
    using Corvus.EventStore.AzureCosmos;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Main program.
    /// </summary>
    public class Program
    {
        private static readonly Guid InterestingId = Guid.Parse("01a8a6a1-24ea-4a7f-9be9-2d40d3b6b49d");

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Program arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddJsonFile($"local.settings.json", true, true);

            IConfigurationRoot config = builder.Build();

            await ReadBlobAzure(config, InterestingId).ConfigureAwait(false);

            Console.ReadKey();
        }

        private static async Task ReadBlobAzure(IConfigurationRoot config, Guid id)
        {
            Console.WriteLine("Running with AzureBlob in Azure");
            string connectionstring = config.GetConnectionString("BlobStorageConnectionString");

            var eventStore = AzureBlobEventStore.GetInstance(connectionstring, "corvusevents", NullSnapshotReader.Instance);

            var sw = Stopwatch.StartNew();

            ToDoListJson root = await ToDoListJson.ReadOrCreate(eventStore, id).ConfigureAwait(false);
            sw.Stop();

            Console.WriteLine($"Read an aggregate root with {root.CommitSequenceNumber} commits and {root.EventSequenceNumber} events in {sw.ElapsedMilliseconds}ms");
        }

        private static async Task SimpleAzureBlob(IConfigurationRoot config)
        {
            Console.WriteLine("Running with AzureBlob in Azure");
            string connectionstring = config.GetConnectionString("BlobStorageConnectionString");

            var eventStore = AzureBlobEventStore.GetInstance(connectionstring, "corvusevents", NullSnapshotReader.Instance);

            await ExecuteSimple(eventStore).ConfigureAwait(false);

            Console.WriteLine("Finished running with AzureBlob in Azure");
        }

        private static async Task SimpleCosmos(IConfigurationRoot config)
        {
            Console.WriteLine("Running with Cosmos in Azure");
            string connectionstring = config.GetConnectionString("CosmosConnectionString");

            var eventStore = CosmosEventStore.GetInstance(connectionstring, "corvuseventstore", "corvusevents", NullSnapshotReader.Instance);

            await ExecuteSimple(eventStore).ConfigureAwait(false);

            Console.WriteLine("Finished running with Cosmos in Azure");
        }

        private static async Task ExecuteSimple(IJsonEventStore eventStore)
        {
            var toDoListId = Guid.NewGuid();

            // Read and modify a todoitem using the simple aggregate.
            ToDoList toDoList = await ToDoList.ReadOrCreate(eventStore, toDoListId).ConfigureAwait(false);

            toDoList = toDoList.Initialize(DateTimeOffset.Now, "Bill Gates");
            toDoList = await toDoList.Commit().ConfigureAwait(false);

            // Now load it using the "json-specific" aggregate.
            ToDoListJson toDoListJson = await ToDoListJson.ReadOrCreate(eventStore, toDoListId).ConfigureAwait(false);
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
        }

        private static async Task WriteVolumeAzureBlob(IConfigurationRoot config)
        {
            string connectionstring = config.GetConnectionString("BlobStorageConnectionString");

            var eventStore = AzureBlobEventStore.GetInstance(connectionstring, "corvusevents", NullSnapshotReader.Instance);

            await ExecuteVolume(eventStore).ConfigureAwait(false);
        }

        private static async Task WriteVolumeCosmos(IConfigurationRoot config)
        {
            string connectionstring = config.GetConnectionString("CosmosConnectionString");

            var eventStore = CosmosEventStore.GetInstance(connectionstring, "corvuseventstore", "corvusevents", NullSnapshotReader.Instance);

            await ExecuteVolume(eventStore).ConfigureAwait(false);
        }

        private static async Task ExecuteVolume(IJsonEventStore eventStore)
        {
            // This is the ID of our aggregate - imagine this came in from the request, for example.
            Guid[] aggregateIds = Enumerable.Range(0, 625)
                .Select(i => Guid.NewGuid())
                .ToArray();

            const int batchSize = 625;
            const int iterations = 50;
            const int nodesPerAggregate = 8;
            const int minTimePerIteration = 0;
            const int eventsPerCommit = 8;

            var aggregates = new ToDoListJson[aggregateIds.Length];

            Console.WriteLine("Initializing aggregates.");

            var loadSw = Stopwatch.StartNew();
            for (int i = 0; i < aggregateIds.Length; ++i)
            {
                Guid id = aggregateIds[i];
                aggregates[i] = ToDoListJson.Create(eventStore, id);
            }

            loadSw.Stop();

            Console.WriteLine($"Read {aggregateIds.Length} aggregates in {loadSw.ElapsedMilliseconds / 1000.0} seconds ({aggregateIds.Length / (loadSw.ElapsedMilliseconds / 1000.0)} agg/sec)");

            Console.WriteLine();

            var executeSw = Stopwatch.StartNew();

            var taskList = new Task<ToDoListJson>[batchSize];

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
                            ToDoListJson toDoList = aggregates[(batch * batchSize) + taskCount];
                            for (int eventCount = 0; eventCount < eventsPerCommit; ++eventCount)
                            {
                                toDoList = toDoList.AddToDoItem(Guid.NewGuid(), "This is my title", "This is my description");
                            }

                            taskList[taskCount] = toDoList.Commit();
                        }

                        ToDoListJson[] batchAggregates = await Task.WhenAll(taskList).ConfigureAwait(false);
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
