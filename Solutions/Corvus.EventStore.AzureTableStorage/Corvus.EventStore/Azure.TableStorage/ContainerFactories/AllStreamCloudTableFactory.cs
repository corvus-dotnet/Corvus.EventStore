// <copyright file="AllStreamCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.ContainerFactories
{
    using Corvus.EventStore.Azure.TableStorage.Core;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Provide an <see cref="IAllStreamCloudTableFactory"/> for development storage.
    /// </summary>
    public readonly struct AllStreamCloudTableFactory : IAllStreamCloudTableFactory
    {
        private readonly CloudTableClient client;
        private readonly CloudTable table;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllStreamCloudTableFactory"/> struct.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="tableName">The table name to use.</param>
        public AllStreamCloudTableFactory(string connectionString, string tableName)
        {
            this.TableName = tableName;
            var account = CloudStorageAccount.Parse(connectionString);
            this.client = account.CreateCloudTableClient(new TableClientConfiguration());
            this.table = GetTableReference(this.client, this.TableName);
            if (this.table.CreateIfNotExists())
            {
                TableStorageEventMerger.SetCreationTimestamp(this.table);
            }
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

        /// <inheritdoc/>
        public long GetCreationTimestamp()
        {
            return TableStorageEventMerger.GetCreationTimestamp(this.table);
        }

        private static CloudTable GetTableReference(CloudTableClient client, string tableName)
        {
            return client.GetTableReference(tableName ?? "corvusevents");
        }
    }
}
