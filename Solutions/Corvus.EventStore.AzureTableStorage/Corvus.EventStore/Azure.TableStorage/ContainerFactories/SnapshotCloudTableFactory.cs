﻿// <copyright file="SnapshotCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.SnapshotStore.Azure.TableStorage.ContainerFactories
{
    using System;
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Provide an <see cref="ISnapshotCloudTableFactory"/> for development storage.
    /// </summary>
    public readonly struct SnapshotCloudTableFactory : ISnapshotCloudTableFactory
    {
        private readonly CloudTableClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventCloudTableFactory"/> struct.
        /// </summary>
        /// <param name="connectionString">The connection string to use.</param>
        /// <param name="tableName">The table name to use.</param>
        public SnapshotCloudTableFactory(string connectionString, string tableName)
        {
            this.TableName = tableName;
            var account = CloudStorageAccount.Parse(connectionString);
            this.client = account.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = this.GetTableReference();
            table.CreateIfNotExists();
        }

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string TableName { get; }

        /// <inheritdoc/>
        public CloudTable GetTable(Guid aggregateId, string partitionKey)
        {
            return this.GetTableReference();
        }

        /// <inheritdoc/>
        public ImmutableArray<CloudTable> GetTables()
        {
            return ImmutableArray.Create(this.GetTableReference());
        }

        private CloudTable GetTableReference()
        {
            return this.client.GetTableReference(this.TableName ?? "corvussnapshots");
        }
    }
}