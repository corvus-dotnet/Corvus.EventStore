// <copyright file="Builder.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
namespace Corvus.EventStore.AzureBlob.AllStreamBuilder
{
    using Microsoft.Azure.EventGrid.Models;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.EventGrid;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Functions host for the event grid subscription.
    /// </summary>
    public static class Builder
    {
        /// <summary>
        /// Process the blob append events from the EventGrid for events coming in to the event store.
        /// </summary>
        /// <param name="eventGridEvent">The event grid event to process.</param>
        /// <param name="log">The logger.</param>
        [FunctionName("CorvusAllStreamBuilder")]
        public static void Run([EventGridTrigger] EventGridEvent eventGridEvent, ILogger log)
        {
            // 1. Get the current append blob
            // 2. Pull the aggregate pointed to
            // 3. Acquire a lease on this aggregate's "current position" blob and read the current position
            // 3a. If we can't acquire the lease, skip it, because someone else will pick it up (see below)
            // 4. Write cmmits from that position to the end into our append blob
            // 5. If our append blob reaches our block size threshold (say 2MB as a starting point; we will need to tune this so it should come from configuration?) or we reach a timeout, write our entire blob to the all stream
            // 6. Reset our output blob.
            // 7. Once we've got to the end of the blob, update the "start position" for the blob, release the lease, and head back to 3 above to try again
            //
            // We can run as many of these as we like in parallel, consuming events from the event grid (although naturally partitioning them in some way would be a good idea)
            log.LogInformation(eventGridEvent.Data.ToString());
        }
    }
}
