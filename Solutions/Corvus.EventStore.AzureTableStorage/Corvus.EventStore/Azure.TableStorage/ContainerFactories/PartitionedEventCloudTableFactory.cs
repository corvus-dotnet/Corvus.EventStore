// <copyright file="PartitionedEventCloudTableFactory.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.ContainerFactories
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// A cloud table factory which partitions results by partition key to one of a number of
    /// subsidiary factories of type <typeparamref name="TFactory"/>, to generate the corresponding
    /// physical partitions.
    /// </summary>
    /// <typeparam name="TFactory">The type of the factories that will be used to create cloud tables for the data.</typeparam>
    public readonly struct PartitionedEventCloudTableFactory<TFactory> : IEventCloudTableFactory
        where TFactory : IEventCloudTableFactory
    {
        private readonly ImmutableArray<TFactory> factories;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartitionedEventCloudTableFactory{TFactory}"/> struct.
        /// </summary>
        /// <param name="factories">The cloud table factories in which to put partitioned data.</param>
        public PartitionedEventCloudTableFactory(params TFactory[] factories)
        {
            this.factories = ImmutableArray.Create(factories);
        }

        /// <inheritdoc/>
        public CloudTable GetTable(Guid aggregateId, string partitionKey)
        {
            int physicalPartition = (int)((ulong)partitionKey.GetHashCode() % (ulong)this.factories.Length);
            return this.factories[physicalPartition].GetTable(aggregateId, partitionKey);
        }

        /// <inheritdoc/>
        public ImmutableArray<CloudTable> GetTables()
        {
            ImmutableArray<CloudTable>.Builder builder = ImmutableArray.CreateBuilder<CloudTable>();

            foreach (TFactory factory in this.factories)
            {
                builder.AddRange(factory.GetTables());
            }

            return builder.ToImmutable();
        }
    }
}
