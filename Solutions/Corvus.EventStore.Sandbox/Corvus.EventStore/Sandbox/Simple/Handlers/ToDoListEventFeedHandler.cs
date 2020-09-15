// <copyright file="ToDoListEventFeedHandler.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Sandbox.Simple.Handlers
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Corvus.EventStore.Sandbox.Events;

    /// <summary>
    /// A basic event feed handler for the event feed.
    /// </summary>
    public class ToDoListEventFeedHandler : IEventFeedHandler
    {
        private int commitCount;
        private int eventCount;

        private StringBuilder log = new StringBuilder();

        /// <summary>
        /// Gets the total event count seen by this handler.
        /// </summary>
        public int TotalEventCount { get; private set; }

        /// <summary>
        /// Gets the total commit count seen by this handler.
        /// </summary>
        public int TotalCommitCount { get; private set; }

        /// <summary>
        /// Gets the log.
        /// </summary>
        public string Log => this.log.ToString();

        /// <inheritdoc/>
        public Task HandleBatchComplete(string checkpoint)
        {
            // Write the checkpoint
            this.TotalCommitCount += this.commitCount;
            Console.WriteLine($"(c: {this.TotalCommitCount}, e: {this.TotalEventCount})Seen a batch of {this.commitCount} commits");
            this.commitCount = 0;
            this.log.Clear();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void HandleCommitComplete(Guid aggregateId, long commitSequenceNumber)
        {
            this.TotalEventCount += this.eventCount;
            this.eventCount = 0;
            this.commitCount += 1;
        }

        /// <inheritdoc/>
        public void HandleSerializedEvent(Guid aggregateId, long commitSequenceNumber, string eventType, long eventSequenceNumber, IPayloadReader payloadReader)
        {
            switch (eventType)
            {
                case ToDoItemAddedEventPayload.EventType:
                    ToDoItemAddedEventPayload added = payloadReader.Read<ToDoItemAddedEventPayload>();
                    this.log.AppendLine($"{aggregateId} : {commitSequenceNumber:D21}.{eventSequenceNumber:D21} ToDoItemAdded : {added.Id} {added.Title}");
                    break;
                case ToDoItemRemovedEventPayload.EventType:
                    ToDoItemRemovedEventPayload removed = payloadReader.Read<ToDoItemRemovedEventPayload>();
                    this.log.AppendLine($"{aggregateId} : {commitSequenceNumber:D21}.{eventSequenceNumber:D21} ToDoItemRemoved : {removed.Id}");
                    break;
                case ToDoListOwnerSetEventPayload.EventType:
                    ToDoListOwnerSetEventPayload owner = payloadReader.Read<ToDoListOwnerSetEventPayload>();
                    this.log.AppendLine($"{aggregateId} : {commitSequenceNumber:D21}.{eventSequenceNumber:D21} ToDoListOwnerSet : {owner.Owner}");
                    break;
                case ToDoListStartDateSetEventPayload.EventType:
                    ToDoListStartDateSetEventPayload startDate = payloadReader.Read<ToDoListStartDateSetEventPayload>();
                    this.log.AppendLine($"{aggregateId} : {commitSequenceNumber:D21}.{eventSequenceNumber:D21} ToDoListStartDateSet : {startDate.StartDate}");
                    break;
                default:
                    this.log.AppendLine($"The event for aggregate {aggregateId} in commit {commitSequenceNumber} with event sequence number {eventSequenceNumber} had event type {eventType} which was not recognized as a valid event type for the ToDoListAggregate.");
                    break;
            }

            this.eventCount += 1;
        }
    }
}
