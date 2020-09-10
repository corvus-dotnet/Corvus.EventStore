// <copyright file="ISnapshotContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.ContainerFactories
{
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// A container factory to provide the <see cref="Container"/> for a particular
    /// aggregate.
    /// </summary>
    public interface ISnapshotContainerFactory
    {
        /// <summary>
        /// Gets an instance of the cloud container for the given store.
        /// </summary>
        /// <returns>The cloud container for that store.</returns>
        Container GetContainer();
    }
}