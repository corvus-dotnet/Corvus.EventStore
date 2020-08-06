// <copyright file="IAggregateReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Aggregates
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Snapshots;

    /// <summary>
    /// An interface implemented by aggregate readers.
    /// </summary>
    public interface IAggregateReader
    {
        /// <summary>
        /// Reads the aggregate with the specified Id.
        /// </summary>
        /// <typeparam name="TAggregate">The type of aggregate being read.</typeparam>
        /// <param name="aggregateFactory">A factory method for creating an aggregate from a snapshot.</param>
        /// <param name="aggregateId">The Id of the aggregate.</param>
        /// <param name="partitionKey">The partition key of the aggregate.</param>
        /// <param name="sequenceNumber">The maximum sequence number to retrieve.</param>
        /// <returns>The specified aggregate.</returns>
        /// <remarks>
        /// If a sequence number is supplied, the aggregate will be recreated using events with sequence numbers lower
        /// than or equal to the specified value.
        /// </remarks>
        ValueTask<TAggregate> ReadAsync<TAggregate>(
            Func<SerializedSnapshot, TAggregate> aggregateFactory,
            Guid aggregateId,
            string partitionKey,
            long sequenceNumber = long.MaxValue)
        where TAggregate : IAggregateRoot<TAggregate>;
    }
}