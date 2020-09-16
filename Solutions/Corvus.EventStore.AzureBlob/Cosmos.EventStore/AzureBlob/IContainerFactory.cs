// <copyright file="IContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.AzureCosmos
{
    using Azure.Storage.Blobs;

    /// <summary>
    /// A factory for creating blob containers.
    /// </summary>
    public interface IContainerFactory
    {
        /// <summary>
        /// Create a Cosmos container instance.
        /// </summary>
        /// <param name="partitionKey">The partition key for which to get the container.</param>
        /// <returns>The <see cref="BlobContainerClient"/> for the container.</returns>
        BlobContainerClient GetContainer(string partitionKey);
    }
}
