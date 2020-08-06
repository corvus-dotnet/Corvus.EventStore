// <copyright file="NeverSnapshotPolicy.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Aggregates
{
    /// <summary>
    /// A snapshot policy which ensures a snapshot is never created.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the aggregate for which this is a policy.</typeparam>
    public readonly struct NeverSnapshotPolicy<TAggregate> : IAggregateWriterSnapshotPolicy<TAggregate>
        where TAggregate : IAggregateRoot<TAggregate>
    {
        /// <summary>
        /// The singleton instance of the policy.
        /// </summary>
        public static readonly NeverSnapshotPolicy<TAggregate> Instance = default;

        /// <inheritdoc/>
        public bool ShouldSnapshot(in TAggregate aggregate, long timestamp)
        {
            return false;
        }
    }
}
