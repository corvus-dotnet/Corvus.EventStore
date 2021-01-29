// <copyright file="AzureBlobEventStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.AzureBlob
{
    using System.Text.Json;

    /// <summary>
    /// Static helpers to get an instance of a <see cref="AzureBlobEventStore{TContainerClientFactory,TSnapshotReader}"/>.
    /// </summary>
    public static class AzureBlobEventStore
    {
        /// <summary>
        /// Get an instance of a <see cref="AzureBlobEventStore{TContainerClientFactory,TSnapshotReader}"/> using a standard connection.
        /// </summary>
        /// <typeparam name="TSnapshotReader">The type of snapshot reader to use for the event store.</typeparam>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="containerName">The container name.</param>
        /// <param name="snapshotReader">The snapshot reader.</param>
        /// <param name="options">The (optional) <see cref="JsonSerializerOptions"/>.</param>
        /// <returns>An instance of a <see cref="AzureBlobEventStore{TContainerClientFactory,TSnapshotReader}"/> initialized with an appropriate <see cref="ContainerClientFactory"/>.</returns>
        public static AzureBlobEventStore<ContainerClientFactory, TSnapshotReader> GetInstance<TSnapshotReader>(string connectionString, string containerName, in TSnapshotReader snapshotReader, JsonSerializerOptions? options = null)
            where TSnapshotReader : ISnapshotReader
        {
            return new AzureBlobEventStore<ContainerClientFactory, TSnapshotReader>(new ContainerClientFactory(connectionString, containerName), snapshotReader, options);
        }
    }
}
