// <copyright file="CosmosEventStore{TContainerFactory,TSnapshotReader}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.AzureCosmos
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Corvus.EventStore;
    using Corvus.EventStore.Json;
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// A Cosmos implementation of an event store.
    /// </summary>
    /// <typeparam name="TContainerFactory">The type of the <see cref="IContainerFactory"/> to use.</typeparam>
    /// <typeparam name="TSnapshotReader">The type of snapshot reader to use.</typeparam>
    public readonly struct CosmosEventStore<TContainerFactory, TSnapshotReader>
        where TContainerFactory : IContainerFactory
        where TSnapshotReader : ISnapshotReader
    {
        private const string DocumentsName = "Documents";

        private readonly CosmosJsonStore jsonStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosEventStore{TContainerFactory, TSnapshotReader}"/> class.
        /// </summary>
        /// <param name="containerFactory">The container factory to use for the event store.</param>
        /// <param name="snapshotReader">The snapshot reader to use for the event store.</param>
        /// <param name="options">The <see cref="Options"/>.</param>
        public CosmosEventStore(TContainerFactory containerFactory, TSnapshotReader snapshotReader, JsonSerializerOptions? options = null)
        {
            this.ContainerFactory = containerFactory;
            this.SnapshotReader = snapshotReader;
            this.Options = options ?? new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            this.jsonStore = new CosmosJsonStore(containerFactory.GetContainer());
        }

        /// <summary>
        /// Gets the <see cref="ContainerFactory"/> for the store.
        /// </summary>
        public TContainerFactory ContainerFactory { get; }

        /// <summary>
        /// Gets the snapshot reader in use for the store.
        /// </summary>
        public TSnapshotReader SnapshotReader { get; }

        /// <summary>
        /// Gets the <see cref="JsonSerializerOptions"/> for the event store.
        /// </summary>
        public JsonSerializerOptions Options { get; }

        /// <summary>
        /// Reads an aggregate root.
        /// </summary>
        /// <typeparam name="TMemento">The type of the memento for the aggregate root.</typeparam>
        /// <typeparam name="TEventHandler">The type of the event reader for the aggregate root. This is an <see cref="IEventHandler{TMemento}"/>.</typeparam>
        /// <param name="id">The ID of the aggregate root.</param>
        /// <param name="partitionKey">The partition key for the aggregate root.</param>
        /// <param name="emptyMemento">The initial empty memento.</param>
        /// <param name="eventHandler">The event reader capable of decoding and applying the event payloads for this aggregate root.</param>
        /// <returns>A <see cref="Task{TResult}"/> which provide the <see cref="JsonAggregateRoot{TMemento, TJsonStore}"/> loaded from the store.</returns>
        public Task<JsonAggregateRoot<TMemento, CosmosJsonStore>> Read<TMemento, TEventHandler>(Guid id, string partitionKey, TMemento emptyMemento, TEventHandler eventHandler)
            where TEventHandler : IEventHandler<TMemento>
        {
            return this.Read(this.ContainerFactory.GetContainer(), this.SnapshotReader, id, partitionKey, emptyMemento, eventHandler, this.Options);
        }

        /// <summary>
        /// Reads an aggregate root.
        /// </summary>
        /// <typeparam name="TMemento">The type of the memento> for the aggregate root.</typeparam>
        /// <typeparam name="TEventHandler">The type of the payload reader for the aggregate root. This is an <see cref="IJsonEventHandler{TMemento}"/>.</typeparam>
        /// <param name="id">The ID of the aggregate root.</param>
        /// <param name="partitionKey">The partition key for the aggregate root.</param>
        /// <param name="emptyMemento">The initial empty memento.</param>
        /// <param name="eventHandler">The payload reader capable of decoding and applying the event payloads for this aggregate root.</param>
        /// <returns>A <see cref="Task{TResult}"/> which provide the <see cref="JsonAggregateRoot{TMemento, TJsonStore}"/> loaded from the store.</returns>
        public Task<JsonAggregateRoot<TMemento, CosmosJsonStore>> ReadJson<TMemento, TEventHandler>(Guid id, string partitionKey, TMemento emptyMemento, TEventHandler eventHandler)
            where TEventHandler : IJsonEventHandler<TMemento>
        {
            return this.ReadJson(this.ContainerFactory.GetContainer(), this.SnapshotReader, id, partitionKey, emptyMemento, eventHandler, this.Options);
        }

        /// <summary>
        /// Reads an event feed.
        /// </summary>
        /// <typeparam name="TEventHandler">The type of the event reader for the event feed. This is an <see cref="IEventFeedHandler"/>.</typeparam>
        /// <param name="eventHandler">The event reader capable of decoding and applying the event payloads for this aggregate root.</param>
        /// <param name="pageSizeHint">A hint as to the number of items you get in a page of results.</param>
        /// <param name="continuationToken">The (optional) continuation token to resume the feed from a particular point.</param>
        /// <param name="cancellationToken">The cancellation token to terminate reading the feed.</param>
        /// <returns>A <see cref="Task{TResult}"/> which completes when the feed terminates.</returns>
        public Task ReadFeed<TEventHandler>(TEventHandler eventHandler, int pageSizeHint, string? continuationToken, CancellationToken cancellationToken)
            where TEventHandler : IEventFeedHandler
        {
            return this.ReadFeed(this.ContainerFactory.GetContainer(), eventHandler, pageSizeHint, continuationToken, this.Options, cancellationToken);
        }

        /// <summary>
        /// Reads an aggregate root.
        /// </summary>
        /// <typeparam name="TEventHandler">The type of the event reader for the aggregate root. This is an <see cref="IJsonEventFeedHandler"/>.</typeparam>
        /// <param name="eventHandler">The event reader capable of decoding and applying the event payloads for this aggregate root.</param>
        /// <param name="pageSizeHint">A hint as to the number of items you get in a page of results.</param>
        /// <param name="continuationToken">The (optional) continuation token to resume the feed from a particular point.</param>
        /// <param name="cancellationToken">The cancellation token to terminate reading the feed.</param>
        /// <returns>A <see cref="Task{TResult}"/> which completes when the feed terminates.</returns>
        public Task ReadFeedJson<TEventHandler>(TEventHandler eventHandler, int pageSizeHint, string? continuationToken, CancellationToken cancellationToken)
            where TEventHandler : IJsonEventFeedHandler
        {
            return this.ReadFeedJson(this.ContainerFactory.GetContainer(), eventHandler, pageSizeHint, continuationToken, this.Options, cancellationToken);
        }

        private static bool FindDocumentsArray(ref Utf8JsonStreamReader streamReader)
        {
            streamReader.Read();
            if (streamReader.TokenType != JsonTokenType.StartArray)
            {
                return false;
            }

            return true;
        }

        private static bool FindDocumentsProperty(ref Utf8JsonStreamReader streamReader)
        {
            while (streamReader.TokenType != JsonTokenType.PropertyName || !streamReader.Match(DocumentsName))
            {
                if (!streamReader.Read())
                {
                    return false;
                }
            }

            return true;
        }

        private Task<JsonAggregateRoot<TMemento, CosmosJsonStore>> Read<TMemento, TEventHandler>(Container container, TSnapshotReader snapshotReader, Guid id, string partitionKeyValue, TMemento emptyMemento, TEventHandler eventReader, JsonSerializerOptions options)
            where TEventHandler : IEventHandler<TMemento>
        {
            return this.ReadJson(container, snapshotReader, id, partitionKeyValue, emptyMemento, new JsonAggregateRoot<TMemento, CosmosJsonStore>.JsonEventHandlerOverJsonSerializer<TEventHandler>(eventReader, options), options);
        }

        private async Task<JsonAggregateRoot<TMemento, CosmosJsonStore>> ReadJson<TMemento, TEventHandler>(Container container, TSnapshotReader snapshotReader, Guid id, string partitionKeyValue, TMemento emptyMemento, TEventHandler eventHandler, JsonSerializerOptions options)
            where TEventHandler : IJsonEventHandler<TMemento>
        {
            var partitionKey = new PartitionKey(partitionKeyValue);

            var encodedPartitionKey = JsonEncodedText.Encode(partitionKeyValue);

            long commitSequenceNumber = -1;
            long eventSequenceNumber = -1;
            TMemento memento = emptyMemento;

            Snapshot<TMemento>? snapshot = await snapshotReader.Read<TMemento>(id, long.MaxValue).ConfigureAwait(false);

            if (snapshot.HasValue)
            {
                commitSequenceNumber = snapshot.Value.CommitSequenceNumber;
                eventSequenceNumber = snapshot.Value.EventSequenceNumber;
                memento = snapshot.Value.Memento;
            }

            var queryRequestOptions = new QueryRequestOptions { PartitionKey = partitionKey };

            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM CommitDocuments d WHERE d.aggregateId = @aggregateId AND d.sequenceNumber > @fromSequenceNumber ORDER BY d.sequenceNumber")
                .WithParameter("@aggregateId", id)
                .WithParameter("@fromSequenceNumber", commitSequenceNumber);

            FeedIterator iterator = container.GetItemQueryStreamIterator(queryDefinition, requestOptions: queryRequestOptions);

            while (iterator.HasMoreResults)
            {
                using ResponseMessage response = await iterator.ReadNextAsync().ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                // The response stream includes a JSON array property called "Documents" which
                // contains the set of results in which we are interested
                (memento, commitSequenceNumber, eventSequenceNumber) = this.ProcessStream(id, commitSequenceNumber, eventSequenceNumber, memento, eventHandler, response.Content, options.DefaultBufferSize);
            }

            var bufferWriter = new ArrayBufferWriter<byte>();
            var utf8JsonWriter = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions { Encoder = options.Encoder, Indented = options.WriteIndented, SkipValidation = false });
            return new JsonAggregateRoot<TMemento, CosmosJsonStore>(id, memento!, this.jsonStore, bufferWriter, utf8JsonWriter, encodedPartitionKey, eventSequenceNumber, commitSequenceNumber, false, ReadOnlyMemory<byte>.Empty, options);
        }

        private Task ReadFeed<TEventHandler>(Container container, TEventHandler eventHandler, int pageSizeHint, string? continuationToken, JsonSerializerOptions options, CancellationToken cancallationToken)
            where TEventHandler : IEventFeedHandler
        {
            return this.ReadFeedJson(container, new JsonEventFeed.JsonEventFeedHandlerOverJsonSerializer<TEventHandler>(eventHandler, options), pageSizeHint, continuationToken, options, cancallationToken);
        }

        private async Task ReadFeedJson<TEventHandler>(Container container, TEventHandler eventHandler, int pageSizeHint, string? continuationToken, JsonSerializerOptions options, CancellationToken cancellationToken)
             where TEventHandler : IJsonEventFeedHandler
        {
            var requestOptions = new ChangeFeedRequestOptions { PageSizeHint = pageSizeHint };
            FeedIterator iterator = container.GetChangeFeedStreamIterator(continuationToken is null ? ChangeFeedStartFrom.Beginning() : ChangeFeedStartFrom.ContinuationToken(continuationToken), requestOptions);

            while (iterator.HasMoreResults && !cancellationToken.IsCancellationRequested)
            {
                using ResponseMessage response = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                response.EnsureSuccessStatusCode();

                this.ProcessEventFeedStream(eventHandler, response.Content, options.DefaultBufferSize, cancellationToken);
                await eventHandler.HandleBatchComplete(response.ContinuationToken).ConfigureAwait(false);
            }
        }

        private void ProcessEventFeedStream<TEventHandler>(TEventHandler eventHandler, Stream content, int defaultBufferSize, CancellationToken cancellationToken)
            where TEventHandler : IJsonEventFeedHandler
        {
            var streamReader = new Utf8JsonStreamReader(content, defaultBufferSize);

            if (!FindDocumentsProperty(ref streamReader))
            {
                throw new JsonException("Unable to find the Documents property in the output stream.");
            }

            if (!FindDocumentsArray(ref streamReader))
            {
                throw new JsonException("The Documents propery was expected to be a JSON array.");
            }

            // We are now at the start of an array of commits
            JsonEventFeed.ProcessCommits(eventHandler, ref streamReader, cancellationToken);
        }

        private (TMemento, long, long) ProcessStream<TMemento, TEventHandler>(Guid aggregateId, long commitSequenceNumber, long eventSequenceNumber, TMemento memento, TEventHandler eventHandler, Stream content, int defaultBufferSize)
            where TEventHandler : IJsonEventHandler<TMemento>
        {
            var streamReader = new Utf8JsonStreamReader(content, defaultBufferSize);

            if (!FindDocumentsProperty(ref streamReader))
            {
                throw new JsonException("Unable to find the Documents property in the output stream.");
            }

            if (!FindDocumentsArray(ref streamReader))
            {
                throw new JsonException("The Documents propery was expected to be a JSON array.");
            }

            // We are now at the start of an array of commits
            return JsonAggregateRoot<TMemento, CosmosJsonStore>.ProcessCommits(aggregateId, commitSequenceNumber, eventSequenceNumber, memento, eventHandler, ref streamReader);
        }
    }
}
