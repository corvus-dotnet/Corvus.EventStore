// <copyright file="IEventFeed.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;

    /// <summary>
    /// Implemented by types which provide a checkpointed
    /// feed of events from the Event Store.
    /// </summary>
    public interface IEventFeed
    {
        /// <summary>
        /// Get the filtered events from the event feed with a filter.
        /// </summary>
        /// <param name="filter">The filter for the event feed.</param>
        /// <param name="maxItems">The maximum number of items in the result set. <c>0 &lt;= result count &lt;= maxItems</c>.</param>
        /// <returns>A set of items from the feed.</returns>
        EventFeedResult Get(EventFeedFilter filter, int maxItems);

        /// <summary>
        /// Gets the filtered events from the given checkpoint.
        /// </summary>
        /// <param name="fromCheckpoint">The checkpoint from which to start reading the stream.</param>
        /// <returns>A set of items from the feed, and the next checkpoint.</returns>
        EventFeedResult Get(ReadOnlyMemory<byte> fromCheckpoint);
    }
}
