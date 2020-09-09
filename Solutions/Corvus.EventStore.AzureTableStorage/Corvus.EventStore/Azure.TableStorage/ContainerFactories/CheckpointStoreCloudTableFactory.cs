// <copyright file="CheckpointStoreCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.ContainerFactories
{
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Provide an <see cref="ICheckpointStoreCloudTableFactory"/> for development storage.
    /// </summary>
    public readonly struct CheckpointStoreCloudTableFactory : ICheckpointStoreCloudTableFactory
    {
        private readonly CloudTableClient client;
        private readonly CloudTable table;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckpointStoreCloudTableFactory"/> struct.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="tableName">The table name to use.</param>
        public CheckpointStoreCloudTableFactory(string connectionString, string tableName)
        {
            this.TableName = tableName;
            var account = CloudStorageAccount.Parse(connectionString);
            this.client = account.CreateCloudTableClient(new TableClientConfiguration());
            this.table = GetTableReference(this.client, this.TableName);
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
