// <copyright file="EventFeedFilter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;
    using System.Collections.Immutable;

    /// <summary>
    /// A filter for the event feed.
    /// </summary>
    /// <remarks>
    /// If any of the filter types are empty, then NO filter is applied for that type.
    /// For each filter type which is not empty, the list is narrowed to only those events which
    /// match ANY OF the supplied values of that type. Each type is applied in turn.
    /// </remarks>
    public readonly struct EventFeedFilter
    {
        /// <summary>
        /// The empty filter.
        /// </summary>
        public static readonly EventFeedFilter Empty = default;

        private EventFeedFilter(ImmutableArray<Guid> aggregateIds, ImmutableArray<string> partitionKeys, ImmutableArray<string> eventTypes)
        {
            this.AggregateIds = aggregateIds;
            this.PartitionKeys = partitionKeys;
            this.EventTypes = eventTypes;
        }

        /// <summary>
        /// Gets the list of <see cref="PartitionKeys"/> whose events are to be included in the result set.
        /// </summary>
        public ImmutableArray<string> PartitionKeys { get; }

        /// <summary>
        /// Gets the list of <see cref="AggregateIds"/> whose events are to be included in the result set.
        /// </summary>
        public ImmutableArray<Guid> AggregateIds { get; }

        /// <summary>
        /// Gets the list of <see cref="EventTypes"/> whose events are to be included in the result set.
        /// </summary>
        public ImmutableArray<string> EventTypes { get; }

        /// <summary>
        /// Adds aggregate IDs to the filter.
        /// </summary>
        /// <param name="aggregateIds">The aggregate IDs to add.</param>
        /// <returns>The event feed filter with the added aggregate Ids.</returns>
        public EventFeedFilter WithAggregateIds(params Guid[] aggregateIds)
        {
            return new EventFeedFilter(this.AggregateIds.AddRange(aggregateIds), this.PartitionKeys, this.EventTypes);
        }

        /// <summary>
        /// Adds partition keys to the filter.
        /// </summary>
        /// <param name="partitionKeys">The partition keys to add.</param>
        /// <returns>The event feed filter with the added partition keys.</returns>
        public EventFeedFilter WithPartitionKeys(params string[] partitionKeys)
        {
            return new EventFeedFilter(this.AggregateIds, this.PartitionKeys.AddRange(partitionKeys), this.EventTypes);
        }

        /// <summary>
        /// Adds event types to the filter.
        /// </summary>
        /// <param name="partitionKeys">The event types to add.</param>
        /// <returns>The event feed filter with the added event types.</returns>
        public EventFeedFilter WithEventTypes(params string[] partitionKeys)
        {
            return new EventFeedFilter(this.AggregateIds, this.PartitionKeys.AddRange(partitionKeys), this.EventTypes);
        }
    }
}