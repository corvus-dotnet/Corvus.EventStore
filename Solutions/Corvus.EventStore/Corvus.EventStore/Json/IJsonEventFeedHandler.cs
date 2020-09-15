// <copyright file="IJsonEventFeedHandler.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Json
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements an optimized json handler for the events that may be applied to an aggregate.
    /// </summary>
    public interface IJsonEventFeedHandler
    {
        /// <summary>
        /// Handle an event in the payload.
        /// </summary>
        /// <param name="streamReader">The utf8 JSON stream reader pointing at the current event.</param>
        /// <param name="aggregateId">The aggregate ID on which we are operating.</param>
        /// <param name="commitSequenceNumber">The sequence number of the current commit.</param>
        /// <remarks>
        /// <para>
        /// You are expected to consume the stream to the end of your serialized event object.
        /// </para>
        /// <para>
        /// The schema is of the form:
        /// <code>
        /// <![CDATA[
        /// {
        ///     "sequenceNumber": [event sequence number; int64],
        ///     "type": "[event type; string]",
        ///     "payload": {your payload; determined by your IJsonEventPayloadWriter},
        /// }
        /// ]]>
        /// </code>
        /// </para>
        /// <para>
        /// The ordering of properties within the schema is guaranteed.
        /// </para>
        /// </remarks>
        void HandleSerializedEvent(ref Utf8JsonStreamReader streamReader, Guid aggregateId, long commitSequenceNumber);

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
        /// Typically, implementers will batch up changes applied by the calls to <see cref="HandleSerializedEvent(ref Utf8JsonStreamReader, Guid, long)"/>
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
