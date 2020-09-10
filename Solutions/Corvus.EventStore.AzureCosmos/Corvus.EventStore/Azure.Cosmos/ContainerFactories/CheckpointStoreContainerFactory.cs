// <copyright file="CheckpointStoreContainerFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.ContainerFactories
{
    using global::Azure.Cosmos;

    /// <summary>
    /// Provide an <see cref="ICheckpointStoreContainerFactory"/> for development storage.
    /// </summary>
    public readonly struct CheckpointStoreContainerFactory : ICheckpointStoreContainerFactory
    {
        private readonly CosmosClient client;
        private readonly CosmosContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckpointStoreContainerFactory"/> struct.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="databaseName">The database name to use.</param>
        /// <param name="containerName">The container name to use.</param>
        public CheckpointStoreContainerFactory(string connectionString, string databaseName, string containerName)
        {
            this.DatabaseName = databaseName;
            this.ContainerName = containerName;
            this.client = new CosmosClient(connectionString);
            this.container = GetContainerReference(this.client, this.DatabaseName, this.ContainerName);
        }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        public string DatabaseName { get; }

        /// <summary>
        /// Gets the container name.
        /// </summary>
        public string ContainerName { get; }

        /// <inheritdoc/>
        public CosmosContainer GetContainer()
        {
            return this.container;
        }

        private static CosmosContainer GetContainerReference(CosmosClient client, string databaseName, string containerName)
        {
            return client.GetDatabase(databaseName).GetContainer(containerName);
        }
    }
}
