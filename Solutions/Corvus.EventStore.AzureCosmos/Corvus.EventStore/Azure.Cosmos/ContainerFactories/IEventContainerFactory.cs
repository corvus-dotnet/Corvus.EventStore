// <copyright file="IEventContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.ContainerFactories
{
    using global::Azure.Cosmos;

    /// <summary>
    /// A container factory to provide the <see cref="CosmosContainer"/> for an event store.
    /// </summary>
    public interface IEventContainerFactory
    {
        /// <summary>
        /// Gets an instance of the cloud container for event store.
        /// </summary>
        /// <returns>The cloud container for that event store.</returns>
        CosmosContainer GetContainer();
    }
}