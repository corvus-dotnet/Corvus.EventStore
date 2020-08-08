// <copyright file="DevelopmentEventCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.ContainerFactories
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Provide an <see cref="IEventCloudTableFactory"/> for development storage.
    /// </summary>
    public readonly struct DevelopmentEventCloudTableFactory : IEventCloudTableFactory
    {
        /// <inheritdoc/>
        public async Task<CloudTable> GetTableAsync(Guid aggregateId, string partitionKey)
        {
            CloudStorageAccount account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudTableClient client = account.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = client.GetTableReference("corvuseventtable");
            await table.CreateIfNotExistsAsync().ConfigureAwait(false);
            return table;
        }
    }
}
