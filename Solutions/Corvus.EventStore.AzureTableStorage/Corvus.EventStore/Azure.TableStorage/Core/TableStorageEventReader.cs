// <copyright file="TableStorageEventReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core
{
    using System;
    using System.Collections.Immutable;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Corvus.EventStore.Azure.TableStorage.Core.Internal;
    using Corvus.EventStore.Core;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// In-memory implementation of <see cref="IEventReader"/>.
    /// </summary>
    public readonly struct TableStorageEventReader : IEventReader
    {
        private readonly IEventCloudTableFactory cloudTableFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageEventReader"/> struct.
        /// </summary>
        /// <param name="cloudTableFactory">The underlying store.</param>
        public TableStorageEventReader(IEventCloudTableFactory cloudTableFactory)
        {
            this.cloudTableFactory = cloudTableFactory;
        }

        /// <inheritdoc/>
        public async ValueTask<EventReaderResult> ReadCommitsAsync(Guid aggregateId, string partitionKey, long fromSequenceNumber, long toSequenceNumber, int maxItems, CancellationToken cancellationToken)
        {
            CloudTable cloudTable = this.cloudTableFactory.GetTable(aggregateId, partitionKey);

            TableQuery query = new TableQuery()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, TableHelpers.BuildPK(aggregateId)),
                        TableOperators.And,
                        TableQuery.CombineFilters(
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, TableHelpers.BuildRK(fromSequenceNumber)),
                            TableOperators.And,
                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, TableHelpers.BuildRK(toSequenceNumber)))))
                .Take(maxItems);

            var resultBuilder = ImmutableArray<Commit>.Empty.ToBuilder();
            TableContinuationToken? continuationToken = null;
            do
            {
                TableQuerySegment<DynamicTableEntity> result = await cloudTable.ExecuteQuerySegmentedAsync(query, continuationToken, new TableRequestOptions { TableQueryMaxItemCount = maxItems }, null, cancellationToken).ConfigureAwait(false);
                continuationToken = result.ContinuationToken;
                foreach (DynamicTableEntity commitEntity in result.Results)
                {
                    byte[] entityListAsBinary = commitEntity.Properties["Commit" + nameof(Commit.Events)].BinaryValue;
                    ImmutableArray<SerializedEvent> events = Utf8JsonEventListSerializer.DeserializeEventList(entityListAsBinary);

                    var commit = new Commit(
                        commitEntity.Properties["Commit" + nameof(Commit.AggregateId)].GuidValue!.Value,
                        commitEntity.Properties["Commit" + nameof(Commit.PartitionKey)].StringValue,
                        commitEntity.Properties["Commit" + nameof(Commit.SequenceNumber)].Int64Value!.Value,
                        commitEntity.Properties["Commit" + nameof(Commit.Timestamp)].Int64Value!.Value,
                        events);
                    resultBuilder.Add(commit);
                    cancellationToken.ThrowIfCancellationRequested();
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
            while (!(continuationToken is null));

            if (resultBuilder.Count == maxItems)
            {
                var externalContinuationToken = new ContinuationToken(aggregateId, partitionKey, fromSequenceNumber + maxItems, toSequenceNumber, maxItems);

                byte[] encodedContinuationToken = JsonSerializer.SerializeToUtf8Bytes(externalContinuationToken);
                return new EventReaderResult(resultBuilder.ToImmutable(), encodedContinuationToken);
            }

            return new EventReaderResult(resultBuilder.ToImmutable(), null);
        }

        /// <inheritdoc/>
        public ValueTask<EventReaderResult> ReadCommitsAsync(ReadOnlySpan<byte> encodedContinuationToken, CancellationToken cancellationToken)
        {
            ContinuationToken continuationToken = JsonSerializer.Deserialize<ContinuationToken>(encodedContinuationToken);

            return this.ReadCommitsAsync(continuationToken.AggregateId, continuationToken.PartitionKey, continuationToken.FromSequenceNumber, continuationToken.ToSequenceNumber, continuationToken.MaxItems, cancellationToken);
        }

        private readonly struct ContinuationToken
        {
            public ContinuationToken(Guid aggregateId, string partitionKey, long fromSequenceNumber, long toSequenceNumber, int maxItems)
            {
                this.FromSequenceNumber = fromSequenceNumber;
                this.ToSequenceNumber = toSequenceNumber;
                this.MaxItems = maxItems;
                this.AggregateId = aggregateId;
                this.PartitionKey = partitionKey;
            }

            public long FromSequenceNumber { get; }

            public long ToSequenceNumber { get; }

            public int MaxItems { get; }

            public Guid AggregateId { get; }

            public string PartitionKey { get; }
        }
    }
}
