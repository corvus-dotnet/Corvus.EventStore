// <copyright file="AlwaysSnapshotPolicy.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Aggregates
{
    /// <summary>
    /// A snapshot policy which ensures a snapshot is created with every commit.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the aggregate for which this is a policy.</typeparam>
    public readonly struct AlwaysSnapshotPolicy<TAggregate> : IAggregateWriterSnapshotPolicy<TAggregate>
        where TAggregate : IAggregateRoot<TAggregate>
    {
        /// <summary>
        /// The singleton instance of the policy.
        /// </summary>
        public static readonly AlwaysSnapshotPolicy<TAggregate> Instance = default;

        /// <inheritdoc/>
        public bool ShouldSnapshot(in TAggregate aggregate, long timestamp)
        {
            return true;
        }
    }
}
