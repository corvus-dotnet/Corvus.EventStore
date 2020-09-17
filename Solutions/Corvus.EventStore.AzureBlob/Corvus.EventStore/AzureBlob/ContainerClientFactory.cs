// <copyright file="ContainerClientFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.AzureBlob
{
    using Azure.Storage.Blobs;

    /// <summary>
    /// Standard container factory for an AzureBlobEventStore.
    /// </summary>
    public readonly struct ContainerClientFactory : IContainerClientFactory
    {
        private readonly BlobServiceClient client;
        private readonly BlobContainerClient container;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerClientFactory"/> struct.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="containerName">The container name to use.</param>
        public ContainerClientFactory(string connectionString, string containerName)
            : this()
        {
            this.ContainerName = containerName;
            this.client = new BlobServiceClient(connectionString);
            this.container = GetContainerClientReference(this.client, this.ContainerName);
        }

        /// <summary>
        /// Gets the container name.
        /// </summary>
        public string ContainerName { get; }

        /// <inheritdoc/>
        public BlobContainerClient GetContainerClient()
        {
            return this.container;
        }

        private static BlobContainerClient GetContainerClientReference(BlobServiceClient client, string containerName)
        {
            return client.GetBlobContainerClient(containerName);
        }
    }
}
