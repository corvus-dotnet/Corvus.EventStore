// <copyright file="DevelopmentEventCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.ContainerFactories
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// Provide an <see cref="IEventCloudTableFactory"/> for development storage.
    /// </summary>
    public readonly struct DevelopmentEventCloudTableFactory : IEventCloudTableFactory
    {
        private readonly CloudTableClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevelopmentEventCloudTableFactory"/> struct.
        /// </summary>
        /// <param name="tableName">The table name to use.</param>
        public DevelopmentEventCloudTableFactory(string tableName)
        {
            this.TableName = tableName;
            CloudStorageAccount account = CloudStorageAccount.DevelopmentStorageAccount;
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
            return this.client.GetTableReference(this.TableName ?? "corvusevents");
        }
    }
}
