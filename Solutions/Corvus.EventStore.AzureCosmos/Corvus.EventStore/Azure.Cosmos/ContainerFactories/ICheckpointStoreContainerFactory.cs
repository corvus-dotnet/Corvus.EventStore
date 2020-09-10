// <copyright file="ICheckpointStoreContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.ContainerFactories
{
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// A container factory to provide the <see cref="Container"/> for the checkpoint store.
    /// </summary>
    public interface ICheckpointStoreContainerFactory
    {
        /// <summary>
        /// Gets an instance of the cloud container for the checkpoint store.
        /// </summary>
        /// <returns>The cloud container for the all stream.</returns>
        Container GetContainer();
    }
}