// <copyright file="CosmosEventStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.AzureCosmos
{
    using System.Text.Json;

    /// <summary>
    /// Static helpers to get an instance of a <see cref="CosmosEventStore{TContainerFactory,TSnapshotReader}"/>.
    /// </summary>
    public static class CosmosEventStore
    {
        /// <summary>
        /// Get an instance of a <see cref="CosmosEventStore{TContainerFactory,TSnapshotReader}"/> using a standard connection.
        /// </summary>
        /// <typeparam name="TSnapshotReader">The type of snapshot reader to use for the event store.</typeparam>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="databaseName">The database name.</param>
        /// <param name="containerName">The container name.</param>
        /// <param name="snapshotReader">The snapshot reader.</param>
        /// <param name="options">The (optional) <see cref="JsonSerializerOptions"/>.</param>
        /// <returns>An instance of a <see cref="CosmosEventStore{TContainerFactory,TSnapshotReader}"/> initialized with an appropriate <see cref="ContainerFactory"/>.</returns>
        public static CosmosEventStore<ContainerFactory, TSnapshotReader> GetInstance<TSnapshotReader>(string connectionString, string databaseName, string containerName, TSnapshotReader snapshotReader, JsonSerializerOptions? options = null)
            where TSnapshotReader : ISnapshotReader
        {
            return new CosmosEventStore<ContainerFactory, TSnapshotReader>(new ContainerFactory(connectionString, databaseName, containerName), snapshotReader, options);
        }
    }
}
