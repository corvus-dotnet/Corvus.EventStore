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
    /// For each filter type which is not empty, the list is narrowed to only those commits which
    /// match ANY OF the supplied values of that type. Each type is applied in turn.
    /// </remarks>
    public readonly struct EventFeedFilter
    {
        /// <summary>
        /// The empty filter.
        /// </summary>
        public static readonly EventFeedFilter Empty = default;
        private readonly ImmutableArray<Guid> aggregateIds;
        private readonly ImmutableArray<string> partitionKeys;

        private EventFeedFilter(ImmutableArray<Guid> aggregateIds, ImmutableArray<string> partitionKeys)
        {
            this.aggregateIds = aggregateIds;
            this.partitionKeys = partitionKeys;
        }

        /// <summary>
        /// Gets the list of <see cref="PartitionKeys"/> whose events are to be included in the result set.
        /// </summary>
        public ImmutableArray<string> PartitionKeys => this.partitionKeys.IsDefault ? ImmutableArray<string>.Empty : this.partitionKeys;

        /// <summary>
        /// Gets the list of <see cref="AggregateIds"/> whose events are to be included in the result set.
        /// </summary>
        public ImmutableArray<Guid> AggregateIds => this.aggregateIds.IsDefault ? ImmutableArray<Guid>.Empty : this.aggregateIds;

        /// <summary>
        /// Adds aggregate IDs to the filter.
        /// </summary>
        /// <param name="aggregateIds">The aggregate IDs to add.</param>
        /// <returns>The event feed filter with the added aggregate Ids.</returns>
        public EventFeedFilter WithAggregateIds(params Guid[] aggregateIds)
        {
            return new EventFeedFilter(this.AggregateIds.AddRange(aggregateIds), this.PartitionKeys);
        }

        /// <summary>
        /// Adds partition keys to the filter.
        /// </summary>
        /// <param name="partitionKeys">The partition keys to add.</param>
        /// <returns>The event feed filter with the added partition keys.</returns>
        public EventFeedFilter WithPartitionKeys(params string[] partitionKeys)
        {
            return new EventFeedFilter(this.AggregateIds, this.PartitionKeys.AddRange(partitionKeys));
        }
    }
}