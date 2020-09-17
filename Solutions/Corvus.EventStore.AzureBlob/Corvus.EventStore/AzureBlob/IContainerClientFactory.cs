// <copyright file="IContainerClientFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.AzureBlob
{
    using Azure.Storage.Blobs;

    /// <summary>
    /// A factory for creating blob containers.
    /// </summary>
    public interface IContainerClientFactory
    {
        /// <summary>
        /// Create a Cosmos container instance.
        /// </summary>
        /// <returns>The <see cref="BlobContainerClient"/> for the container.</returns>
        BlobContainerClient GetContainerClient();
    }
}
