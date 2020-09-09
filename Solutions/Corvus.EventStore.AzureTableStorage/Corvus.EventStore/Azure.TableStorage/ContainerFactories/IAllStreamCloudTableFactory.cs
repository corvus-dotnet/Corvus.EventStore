// <copyright file="IAllStreamCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.ContainerFactories
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// A container factory to provide the <see cref="CloudTable"/> for the all stream.
    /// </summary>
    public interface IAllStreamCloudTableFactory
    {
        /// <summary>
        /// Gets an instance of the cloud table for the all stream.
        /// </summary>
        /// <returns>The cloud table for the all stream.</returns>
        CloudTable GetTable();

        /// <summary>
        /// Gets the UTC timestamp at which the table was created.
        /// </summary>
        /// <returns>The time at which this table was created.</returns>
        long GetCreationTimestamp();
    }
}