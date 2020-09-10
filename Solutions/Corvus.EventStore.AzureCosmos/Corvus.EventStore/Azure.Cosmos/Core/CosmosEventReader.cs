// <copyright file="CosmosEventReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.Core
{
    using System;
    using System.Collections.Immutable;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.Cosmos.ContainerFactories;
    using Corvus.EventStore.Azure.Cosmos.Core.Internal;
    using Corvus.EventStore.Core;
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// Implements an event reader over Cosmos DBV SQL API.
    /// </summary>
    public readonly struct CosmosEventReader : IEventReader
    {
        private readonly IEventContainerFactory containerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosEventWriter"/> struct.
        /// </summary>
        /// <param name="containerFactory">The container factory to use.</param>
        public CosmosEventReader(IEventContainerFactory containerFactory)
        {
            this.containerFactory = containerFactory;
        }

        /// <inheritdoc/>
        public async ValueTask<EventReaderResult> ReadCommitsAsync(Guid aggregateId, string partitionKey, long fromSequenceNumber, long toSequenceNumber, int maxItems, CancellationToken cancellationToken)
        {
            ImmutableArray<Commit>.Builder commits = ImmutableArray.CreateBuilder<Commit>();
            Container container = this.containerFactory.GetContainer();
            var options = new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) };
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM CommitDocuments d WHERE d.AggregateId = @aggregateId AND d.SequenceNumber >= @fromSequenceNumber AND d.SequenceNumber <= @toSequenceNumber ORDER BY d.SequenceNumber")
                .WithParameter("@aggregateId", aggregateId)
                .WithParameter("@fromSequenceNumber", fromSequenceNumber)
                .WithParameter("@toSequenceNumber", toSequenceNumber);

            FeedIterator<CommitDocument> iterator = container.GetItemQueryIterator<CommitDocument>(queryDefinition, null, options);
            while (iterator.HasMoreResults)
            {
                FeedResponse<CommitDocument> results = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
                foreach (CommitDocument commit in results.Resource)
                {
                    commits.Add(commit.GetCommit());
                    if (commits.Count >= maxItems)
                    {
                        break;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }

                if (commits.Count >= maxItems)
                {
                    break;
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            if (commits.Count == maxItems)
            {
                var externalContinuationToken = new ContinuationToken(aggregateId, partitionKey, fromSequenceNumber + maxItems, toSequenceNumber, maxItems);

                byte[] encodedContinuationToken = JsonSerializer.SerializeToUtf8Bytes(externalContinuationToken);
                return new EventReaderResult(commits.ToImmutable(), encodedContinuationToken);
            }

            return new EventReaderResult(commits.ToImmutable(), null);
        }

        /// <inheritdoc/>
        public ValueTask<EventReaderResult> ReadCommitsAsync(ReadOnlySpan<byte> encodedContinuationToken, CancellationToken cancellationToken)
        {
            ContinuationToken continuationToken = JsonSerializer.Deserialize<ContinuationToken>(encodedContinuationToken);

            return this.ReadCommitsAsync(continuationToken.AggregateId, continuationToken.PartitionKey, continuationToken.FromSequenceNumber, continuationToken.ToSequenceNumber, continuationToken.MaxItems, cancellationToken);
        }

        private struct ContinuationToken
        {
            public ContinuationToken(Guid aggregateId, string partitionKey, long fromSequenceNumber, long toSequenceNumber, int maxItems)
            {
                this.FromSequenceNumber = fromSequenceNumber;
                this.ToSequenceNumber = toSequenceNumber;
                this.MaxItems = maxItems;
                this.AggregateId = aggregateId;
                this.PartitionKey = partitionKey;
            }

            public long FromSequenceNumber { get; set; }

            public long ToSequenceNumber { get; set; }

            public int MaxItems { get; set; }

            public Guid AggregateId { get; set; }

            public string PartitionKey { get; set; }
        }
    }
}
