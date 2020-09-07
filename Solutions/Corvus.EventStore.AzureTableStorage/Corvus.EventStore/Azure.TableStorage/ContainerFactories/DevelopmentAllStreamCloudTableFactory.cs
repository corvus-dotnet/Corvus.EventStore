// <copyright file="DevelopmentAllStreamCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.ContainerFactories
{
    using Corvus.EventStore.Azure.TableStorage.Core;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Provide an <see cref="IAllStreamCloudTableFactory"/> for development storage.
    /// </summary>
    public readonly struct DevelopmentAllStreamCloudTableFactory : IAllStreamCloudTableFactory
    {
        private readonly CloudTableClient client;
        private readonly CloudTable table;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevelopmentAllStreamCloudTableFactory"/> struct.
        /// </summary>
        /// <param name="tableName">The table name to use.</param>
        public DevelopmentAllStreamCloudTableFactory(string tableName)
        {
            this.TableName = tableName;
            CloudStorageAccount account = CloudStorageAccount.DevelopmentStorageAccount;
            this.client = account.CreateCloudTableClient(new TableClientConfiguration());
            this.table = GetTableReference(this.client, tableName);
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
            return client.GetTableReference(tableName ?? "corvusallstream");
        }
    }
}
