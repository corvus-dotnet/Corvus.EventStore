// <copyright file="CosmosEventFeed.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.Core
{
    using System;
    using System.Collections.Immutable;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.Cosmos.ContainerFactories;
    using Corvus.EventStore.Azure.Cosmos.Core.Internal;
    using Corvus.EventStore.Core;
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// Implementation of the <see cref="IEventFeed"/> over the Cosmos changed feed.
    /// </summary>
    public readonly struct CosmosEventFeed : IEventFeed
    {
        private readonly IEventContainerFactory containerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosEventFeed"/> struct.
        /// </summary>
        /// <param name="containerFactory">The <see cref="IEventContainerFactory"/>.</param>
        public CosmosEventFeed(IEventContainerFactory containerFactory)
        {
            this.containerFactory = containerFactory;
        }

        /// <inheritdoc/>
        public ValueTask<EventFeedResult> Get(EventFeedFilter filter, int maxItems)
        {
            Container container = this.containerFactory.GetContainer();
            FeedIterator<CommitDocument> iterator = container.GetChangeFeedIterator<CommitDocument>(ChangeFeedStartFrom.Beginning());
            return BuildResult(iterator, filter.AggregateIds, filter.PartitionKeys, maxItems, 0);
        }

        /// <inheritdoc/>
        public ValueTask<EventFeedResult> Get(ReadOnlyMemory<byte> fromCheckpoint)
        {
            Checkpoint cp = JsonSerializer.Deserialize<Checkpoint>(fromCheckpoint.Span);

            Container container = this.containerFactory.GetContainer();
            FeedIterator<CommitDocument> iterator = cp.CosmosContinuationToken is null ? container.GetChangeFeedIterator<CommitDocument>(ChangeFeedStartFrom.Beginning()) : container.GetChangeFeedIterator<CommitDocument>(ChangeFeedStartFrom.ContinuationToken(cp.CosmosContinuationToken));
            return BuildResult(iterator, cp.AggregateIds, cp.PartitionKeys, cp.MaxItems, cp.IndexInPage);
        }

        private static async ValueTask<EventFeedResult> BuildResult(FeedIterator<CommitDocument> iterator, ImmutableArray<Guid> aggregateIds, ImmutableArray<string> partitionKeys, int maxItems, int indexInPage)
        {
            string? continuationToken = null;
            ImmutableArray<Commit>.Builder commits = ImmutableArray.CreateBuilder<Commit>();

            int currentIndex = 0;
            while (iterator.HasMoreResults)
            {
                FeedResponse<CommitDocument> results = await iterator.ReadNextAsync().ConfigureAwait(false);
                bool completedBatch = true;
                foreach (CommitDocument commit in results.Resource)
                {
                    // Filter the items.
                    if (currentIndex < indexInPage)
                    {
                        currentIndex++;
                        continue;
                    }

                    if (!aggregateIds.IsEmpty && !aggregateIds.Contains(commit.AggregateId))
                    {
                        currentIndex++;
                        continue;
                    }

                    if (!partitionKeys.IsEmpty && !partitionKeys.Contains(commit.PartitionKey))
                    {
                        currentIndex++;
                        continue;
                    }

                    currentIndex++;
                    commits.Add(commit.GetCommit());
                    if (commits.Count >= maxItems)
                    {
                        completedBatch = false;
                        break;
                    }
                }

                if (completedBatch)
                {
                    currentIndex = 0;
                    indexInPage = 0;
                    continuationToken = results.ContinuationToken;
                }

                if (commits.Count >= maxItems)
                {
                    break;
                }
            }

            var checkpoint = new Checkpoint(aggregateIds, partitionKeys, maxItems, currentIndex, continuationToken);

            byte[] encodedCheckpoint = JsonSerializer.SerializeToUtf8Bytes(checkpoint);
            return new EventFeedResult(commits.ToImmutable(), encodedCheckpoint);
        }

        private struct Checkpoint
        {
            public Checkpoint(ImmutableArray<Guid> aggregateIds, ImmutableArray<string> partitionKeys, int maxItems, int indexInPage, string? cosmosContinuationToken)
            {
                this.AggregateIds = aggregateIds;
                this.PartitionKeys = partitionKeys;
                this.MaxItems = maxItems;
                this.IndexInPage = indexInPage;
                this.CosmosContinuationToken = cosmosContinuationToken;
            }

            public ImmutableArray<Guid> AggregateIds { get; set; }

            public ImmutableArray<string> PartitionKeys { get; set; }

            public int MaxItems { get; set; }

            public int IndexInPage { get; set; }

            public string? CosmosContinuationToken { get; set;  }
        }
    }
}
