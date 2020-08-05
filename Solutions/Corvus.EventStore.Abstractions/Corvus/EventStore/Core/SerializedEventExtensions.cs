// <copyright file="SerializedEventExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extensions for the <see cref="SerializedEvent"/>.
    /// </summary>
    public static class SerializedEventExtensions
    {
        /// <summary>
        /// Validate that an ordered list of events can be applied to a given aggregate.
        /// </summary>
        /// <param name="events">The events to validate.</param>
        /// <param name="aggregateId">The ID of the aggregate.</param>
        /// <param name="currentSequenceNumber">The current sequence number of the aggregate.</param>
        /// <exception cref="InvalidOperationException">The enumerable was not valid. The reason is in the exception message.</exception>
        public static void ValidateEvents(this IEnumerable<SerializedEvent> events, string aggregateId, long currentSequenceNumber)
        {
            long previousSequenceNumber = currentSequenceNumber;

            foreach (SerializedEvent @event in events)
            {
                if (@event.AggregateId != aggregateId)
                {
                    // TODO: consider a custom exception
                    throw new InvalidOperationException($"Incorrect aggregate Id for event with sequence number {@event.SequenceNumber}. Expected {aggregateId}, actual {@event.AggregateId}");
                }

                if (@event.SequenceNumber != previousSequenceNumber + 1)
                {
                    // TODO: consider a custom exception
                    throw new InvalidOperationException($"Incorrect sequence number. Expected {previousSequenceNumber + 1}, actual {@event.SequenceNumber}");
                }

                ++previousSequenceNumber;
            }
        }
    }
}
