// <copyright file="DevelopmentCheckpointStoreCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.ContainerFactories
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Provide an <see cref="ICheckpointStoreCloudTableFactory"/> for development storage.
    /// </summary>
    public readonly struct DevelopmentCheckpointStoreCloudTableFactory : ICheckpointStoreCloudTableFactory
    {
        private readonly CloudTableClient client;
        private readonly CloudTable table;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevelopmentCheckpointStoreCloudTableFactory"/> struct.
        /// </summary>
        /// <param name="tableName">The table name to use.</param>
        public DevelopmentCheckpointStoreCloudTableFactory(string tableName)
        {
            this.TableName = tableName;
            CloudStorageAccount account = CloudStorageAccount.DevelopmentStorageAccount;
            this.client = account.CreateCloudTableClient(new TableClientConfiguration());
            this.table = GetTableReference(this.client, tableName);
            this.table.CreateIfNotExists();
        }

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string TableName { get; }

        /// <inheritdoc/>
        public CloudTable GetTable()
        {
            return this.table;
        }

        private static CloudTable GetTableReference(CloudTableClient client, string tableName)
        {
            return client.GetTableReference(tableName ?? "corvuscheckpoints");
        }
    }
}
