// <copyright file="IContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.AzureCosmos
{
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// A factory for creating Cosmos containers.
    /// </summary>
    public interface IContainerFactory
    {
        /// <summary>
        /// Create a Cosmos container instance.
        /// </summary>
        /// <returns>The <see cref="Container"/>.</returns>
        Container GetContainer();
    }
}
