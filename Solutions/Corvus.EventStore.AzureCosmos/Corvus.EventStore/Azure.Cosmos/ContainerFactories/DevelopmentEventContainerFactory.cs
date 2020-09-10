// <copyright file="DevelopmentEventContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.ContainerFactories
{
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// Provide an <see cref="IEventContainerFactory"/> for development storage.
    /// </summary>
    public readonly struct DevelopmentEventContainerFactory : IEventContainerFactory
    {
        private readonly CosmosClient client;
        private readonly Container container;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevelopmentEventContainerFactory"/> struct.
        /// </summary>
        /// <param name="databaseName">The <see cref="DatabaseName"/>.</param>
        /// <param name="containerName">The <see cref="ContainerName"/>.</param>
        public DevelopmentEventContainerFactory(string databaseName, string containerName)
        {
            this.DatabaseName = databaseName;
            this.ContainerName = containerName;
            this.client = new CosmosClient("AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
            this.container = GetContainerReference(this.client, databaseName, containerName);
        }

        /// <summary>
        /// Gets the database name.
        /// </summary>
        public string DatabaseName { get; }

        /// <summary>
        /// Gets the container name.
        /// </summary>
        public string ContainerName { get; }

        /// <inheritdoc/>
        public Container GetContainer()
        {
            return this.container;
        }

        private static Container GetContainerReference(CosmosClient client, string databaseName, string containerName)
        {
            return client.GetDatabase(databaseName).GetContainer(containerName);
        }
    }
}
