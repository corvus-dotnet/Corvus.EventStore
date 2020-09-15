// <copyright file="IJsonEventHandler.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Json
{
    using System;
    using System.Text.Json;

    /// <summary>
    /// Implements an optimized json handler for the events that may be applied to an aggregate.
    /// </summary>
    /// <typeparam name="TMemento">The type of the memento to produce.</typeparam>
    public interface IJsonEventHandler<TMemento>
    {
        /// <summary>
        /// Apply the event payload to the memento.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload.</typeparam>
        /// <param name="aggregateId">The ID of the aggregate on which we are operating.</param>
        /// <param name="commitSequenceNumber">The sequence number of the current commit.</param>
        /// <param name="eventType">The type of the event to be handled.</param>
        /// <param name="eventSequenceNumber">The sequence number of the event to be handled.</param>
        /// <param name="memento">The memento to which the event is to applied.</param>
        /// <param name="payload">The event payload to apply.</param>
        /// <returns>The memento with the event payload applied.</returns>
        TMemento HandleEvent<TPayload>(Guid aggregateId, long commitSequenceNumber, JsonEncodedText eventType, long eventSequenceNumber, TMemento memento, TPayload payload);

        /// <summary>
        /// Read the event payload from the reader and apply it to the memento.
        /// </summary>
        /// <param name="streamReader">The utf8 JSON stream reader pointing at the current event.</param>
        /// <param name="aggregateId">The aggregate ID on which we are operating.</param>
        /// <param name="commitSequenceNumber">The sequence number of the current commit.</param>
        /// <param name="expectedEventSequenceNumber">The expected sequence number of the event to be processed.</param>
        /// <param name="memento">The memento to which the event is to be applied.</param>
        /// <returns>The memento with the event payload applied.</returns>
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
        /// The ordering of elements within the schema is guaranteed.
        /// </para>
        /// </remarks>
        TMemento HandleSerializedEvent(ref Utf8JsonStreamReader streamReader, Guid aggregateId, long commitSequenceNumber, long expectedEventSequenceNumber, TMemento memento);
    }
}
