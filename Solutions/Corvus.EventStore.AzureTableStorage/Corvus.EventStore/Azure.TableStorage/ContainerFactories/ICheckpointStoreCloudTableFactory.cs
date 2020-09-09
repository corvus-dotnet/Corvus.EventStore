// <copyright file="ICheckpointStoreCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.ContainerFactories
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// A container factory to provide the <see cref="CloudTable"/> for the checkpoint store.
    /// </summary>
    public interface ICheckpointStoreCloudTableFactory
    {
        /// <summary>
        /// Gets an instance of the cloud table for the checkpoint store.
        /// </summary>
        /// <returns>The cloud table for the all stream.</returns>
        CloudTable GetTable();
    }
}