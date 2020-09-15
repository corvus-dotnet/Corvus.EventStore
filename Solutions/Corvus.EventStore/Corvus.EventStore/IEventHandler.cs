// <copyright file="IEventHandler.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore
{
    using System;

    /// <summary>
    /// Implemented by handlers that can apply events to an aggregate.
    /// </summary>
    /// <typeparam name="TMemento">The type of the memento to produce.</typeparam>
    public interface IEventHandler<TMemento>
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
        TMemento HandleEvent<TPayload>(Guid aggregateId, long commitSequenceNumber, string eventType, long eventSequenceNumber, TMemento memento, TPayload payload);

        /// <summary>
        /// Read the event payload from the reader and apply it to the memento.
        /// </summary>
        /// <param name="aggregateId">The ID of the aggregate on which we are operating.</param>
        /// <param name="commitSequenceNumber">The sequence number of the current commit.</param>
        /// <param name="eventType">The type of the event to be handled.</param>
        /// <param name="eventSequenceNumber">The sequence number of the event to be handled.</param>
        /// <param name="memento">The memento to which the event is to applied.</param>
        /// <param name="payloadReader">The payload reader which can be used to reader the event payload as an instance of a particular type.</param>
        /// <returns>The memento with the event payload applied.</returns>
        TMemento HandleSerializedEvent(Guid aggregateId, long commitSequenceNumber, string eventType, long eventSequenceNumber, TMemento memento, IPayloadReader payloadReader);
    }
}
