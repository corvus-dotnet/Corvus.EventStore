// <copyright file="ContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.AzureCosmos
{
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// Standard container factory for a <see cref="CosmosEventStore"/>.
    /// </summary>
    public readonly struct ContainerFactory : IContainerFactory
    {
        private readonly CosmosClient client;
        private readonly Container container;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerFactory"/> struct.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="databaseName">The name of the database to use.</param>
        /// <param name="containerName">The container name to use.</param>
        public ContainerFactory(string connectionString, string databaseName, string containerName)
            : this()
        {
            this.DatabaseName = databaseName;
            this.ContainerName = containerName;
            this.client = new CosmosClient(connectionString);
            this.container = GetContainerReference(this.client, this.DatabaseName, this.ContainerName);
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
