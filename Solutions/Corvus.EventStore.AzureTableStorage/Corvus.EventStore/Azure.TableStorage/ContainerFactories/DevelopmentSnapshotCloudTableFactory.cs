// <copyright file="DevelopmentSnapshotCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.SnapshotStore.Azure.TableStorage.ContainerFactories
{
    using System;
    using System.Collections.Immutable;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Provide an <see cref="ISnapshotCloudTableFactory"/> for development storage.
    /// </summary>
    public readonly struct DevelopmentSnapshotCloudTableFactory : ISnapshotCloudTableFactory
    {
        private readonly CloudTableClient client;
        private readonly CloudTable table;
        private readonly ImmutableArray<CloudTable> tables;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevelopmentSnapshotCloudTableFactory"/> struct.
        /// </summary>
        /// <param name="tableName">The table name to use.</param>
        public DevelopmentSnapshotCloudTableFactory(string tableName)
        {
            this.TableName = tableName;
            CloudStorageAccount account = CloudStorageAccount.DevelopmentStorageAccount;
            this.client = account.CreateCloudTableClient(new TableClientConfiguration());
            this.table = GetTableReference(this.client, tableName);
            this.table.CreateIfNotExists();
            this.tables = ImmutableArray.Create(this.table);
        }

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string TableName { get; }

        /// <inheritdoc/>
        public CloudTable GetTable(Guid aggregateId, string partitionKey)
        {
            return this.table;
        }

        /// <inheritdoc/>
        public ImmutableArray<CloudTable> GetTables()
        {
            return this.tables;
        }

        private static CloudTable GetTableReference(CloudTableClient client, string tableName)
        {
            return client.GetTableReference(tableName ?? "corvussnapshots");
        }
    }
}
