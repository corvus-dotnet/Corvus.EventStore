// <copyright file="IEventCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.ContainerFactories
{
    using System;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// A container factory to provide the <see cref="CloudTable"/> for a particular
    /// aggregate.
    /// </summary>
    public interface IEventCloudTableFactory
    {
        /// <summary>
        /// Gets an instance of the cloud table for the given aggregate ID and logical partition Key.
        /// </summary>
        /// <param name="aggregateId">The aggregate ID for which to retrieve the table.</param>
        /// <param name="partitionKey">The logical partition key.</param>
        /// <returns>The cloud table for that partition and aggregate.</returns>
        CloudTable GetTable(Guid aggregateId, string partitionKey);
    }
}