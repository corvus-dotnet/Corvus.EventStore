// <copyright file="IEventFeedHandler.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Implemented by handlers that can receive events from an event feed.
    /// </summary>
    public interface IEventFeedHandler
    {
        /// <summary>
        /// Read the event payload from the reader and apply it to the memento.
        /// </summary>
        /// <param name="aggregateId">The ID of the aggregate on which we are operating.</param>
        /// <param name="commitSequenceNumber">The sequence number of the current commit.</param>
        /// <param name="eventType">The type of the event to be handled.</param>
        /// <param name="eventSequenceNumber">The sequence number of the event to be handled.</param>
        /// <param name="payloadReader">The payload reader which can be used to reader the event payload as an instance of a particular type.</param>
        void HandleSerializedEvent(Guid aggregateId, long commitSequenceNumber, string eventType, long eventSequenceNumber, IPayloadReader payloadReader);

        /// <summary>
        /// Called when a set of events in a commit is complete.
        /// </summary>
        /// <param name="aggregateId">The ID of the aggregate on which we are operating.</param>
        /// <param name="commitSequenceNumber">The sequence number of the current commit.</param>
        /// <remarks>
        /// This allows you to identify the events that come in a single commit, while still processing in an event-by-event manner.
        /// </remarks>
        void HandleCommitComplete(Guid aggregateId, long commitSequenceNumber);

        /// <summary>
        /// Called when the current batch of events is complete.
        /// </summary>
        /// <param name="checkpoint">The checkpoint for the batch.</param>
        /// <returns>A <see cref="Task"/> which completes when the batch is complete.</returns>
        /// <remarks>
        /// <para>
        /// Typically, implementers will batch up changes applied by the calls to <see cref="HandleSerializedEvent(Guid, long, string, long, IPayloadReader)"/>
        /// and then commit them when <see cref="HandleBatchComplete(string)"/> is called.
        /// </para>
        /// <para>
        /// You would normally also store your checkpoint at this point, and resume from that point if the session aborts.
        /// </para>
        /// <para>
        /// You should ensure that your implementation of this handler is idempotent. While we guarantee in-order provision of the events
        /// in the feed (at least on a per-aggregate basis), we do not guarantee single-delivery.
        /// </para>
        /// </remarks>
        Task HandleBatchComplete(string checkpoint);
    }
}
