// <copyright file="CosmosSnapshotReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.Snapshots
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.Cosmos.ContainerFactories;
    using Corvus.EventStore.Azure.Cosmos.Snapshots.Internal;
    using Corvus.EventStore.Snapshots;
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// In-memory implementation of <see cref="ISnapshotReader"/>.
    /// </summary>
    public readonly struct CosmosSnapshotReader : ISnapshotReader
    {
        private readonly ISnapshotContainerFactory containerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosSnapshotReader"/> struct.
        /// </summary>
        /// <param name="containerFactory">The factory for the cloud table for this store.</param>
        public CosmosSnapshotReader(ISnapshotContainerFactory containerFactory)
        {
            this.containerFactory = containerFactory;
        }

        /// <inheritdoc/>
        public async ValueTask<SerializedSnapshot> ReadAsync(Guid aggregateId, string partitionKey, long atSequenceNumber = long.MaxValue)
        {
            Container container = this.containerFactory.GetContainer();
            QueryDefinition queryDefinition =
                new QueryDefinition("SELECT * from Snapshots s WHERE s.AggregateId = @aggregateId AND s.CommitSequenceNumber <= @atSequenceNumber ORDER BY s.CommitSequenceNumber DESC OFFSET 0 LIMIT 1")
                .WithParameter("@aggregateId", aggregateId.ToString())
                .WithParameter("@atSequenceNumber", atSequenceNumber);

            FeedIterator<SnapshotDocument> iterator = container.GetItemQueryIterator<SnapshotDocument>(queryDefinition);
            if (iterator.HasMoreResults)
            {
                FeedResponse<SnapshotDocument> result = await iterator.ReadNextAsync().ConfigureAwait(false);
                SnapshotDocument? snapshot = result.Resource.FirstOrDefault();
                if (!(snapshot is null))
                {
                    return snapshot.GetSerializedSnapshot();
                }
            }

            return SerializedSnapshot.Empty(aggregateId, partitionKey);
        }
    }
}
