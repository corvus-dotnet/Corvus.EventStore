// <copyright file="DevelopmentSnapshotCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.SnapshotStore.Azure.TableStorage.ContainerFactories
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Provide an <see cref="ISnapshotCloudTableFactory"/> for development storage.
    /// </summary>
    public readonly struct DevelopmentSnapshotCloudTableFactory : ISnapshotCloudTableFactory
    {
        /// <inheritdoc/>
        public async Task<CloudTable> GetTableAsync(Guid aggregateId, string partitionKey)
        {
            CloudStorageAccount account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudTableClient client = account.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = client.GetTableReference("corvussnapshottable");
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);
            return table;
        }
    }
}
