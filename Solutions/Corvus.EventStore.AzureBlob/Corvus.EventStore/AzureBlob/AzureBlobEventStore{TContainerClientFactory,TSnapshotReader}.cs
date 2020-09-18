// <copyright file="AzureBlobEventStore{TContainerClientFactory,TSnapshotReader}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.AzureBlob
{
    using System;
    using System.Buffers;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Azure.Storage.Blobs.Specialized;
    using Corvus.EventStore;
    using Corvus.EventStore.Json;

    /// <summary>
    /// A Cosmos implementation of an event store.
    /// </summary>
    /// <typeparam name="TContainerClientFactory">The type of the <see cref="IContainerClientFactory"/> to use.</typeparam>
    /// <typeparam name="TSnapshotReader">The type of snapshot reader to use.</typeparam>
    public class AzureBlobEventStore<TContainerClientFactory, TSnapshotReader> : IJsonEventStore
        where TContainerClientFactory : IContainerClientFactory
        where TSnapshotReader : ISnapshotReader
    {
        private readonly AzureBlobStreamStore jsonStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobEventStore{TContainerClientFactory, TSnapshotReader}"/> class.
        /// </summary>
        /// <param name="containerClientFactory">The containerClient factory to use for the event store.</param>
        /// <param name="snapshotReader">The snapshot reader to use for the event store.</param>
        /// <param name="options">The <see cref="Options"/>.</param>
        public AzureBlobEventStore(TContainerClientFactory containerClientFactory, TSnapshotReader snapshotReader, JsonSerializerOptions? options = null)
        {
            this.ContainerClientFactory = containerClientFactory;
            this.SnapshotReader = snapshotReader;
            this.Options = options ?? new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            this.jsonStore = new AzureBlobStreamStore(containerClientFactory.GetContainerClient());
        }

        /// <summary>
        /// Gets the <see cref="ContainerClientFactory"/> for the store.
        /// </summary>
        public TContainerClientFactory ContainerClientFactory { get; }

        /// <summary>
        /// Gets the snapshot reader in use for the store.
        /// </summary>
        public TSnapshotReader SnapshotReader { get; }

        /// <summary>
        /// Gets the <see cref="JsonSerializerOptions"/> for the event store.
        /// </summary>
        public JsonSerializerOptions Options { get; }

        /// <inheritdoc/>
        async Task<IAggregateRoot<TMemento>> IEventStore.Read<TMemento>(Guid id, TMemento emptyMemento, IEventHandler<TMemento> eventHandler)
        {
            return await this.Read(this.ContainerClientFactory.GetContainerClient(), this.SnapshotReader, id, emptyMemento, eventHandler, this.Options).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        async Task<IAggregateRoot<TMemento>> IEventStore.Read<TMemento>(Guid id, string partitionKey, TMemento emptyMemento, IEventHandler<TMemento> eventHandler)
        {
            return await this.Read(this.ContainerClientFactory.GetContainerClient(), this.SnapshotReader, id, emptyMemento, eventHandler, this.Options).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        IAggregateRoot<TMemento> IEventStore.Create<TMemento>(Guid id, TMemento emptyMemento, IEventHandler<TMemento> eventHandler)
        {
            return this.Create(id, emptyMemento, new JsonAggregateRoot<TMemento>.JsonEventHandlerOverJsonSerializer(eventHandler, this.Options));
        }

        /// <inheritdoc/>
        IAggregateRoot<TMemento> IEventStore.Create<TMemento>(Guid id, string partitionKey, TMemento emptyMemento, IEventHandler<TMemento> eventHandler)
        {
            return this.Create(id, partitionKey, emptyMemento, new JsonAggregateRoot<TMemento>.JsonEventHandlerOverJsonSerializer(eventHandler, this.Options));
        }

        /// <inheritdoc/>
        public JsonAggregateRoot<TMemento> Create<TMemento, TEventHandler>(Guid id, TMemento emptyMemento, TEventHandler eventHandler)
            where TEventHandler : IJsonEventHandler<TMemento>
        {
            return this.Create(id, id.ToString(), emptyMemento, eventHandler);
        }

        /// <inheritdoc/>
        public JsonAggregateRoot<TMemento> Create<TMemento, TEventHandler>(Guid id, string partitionKey, TMemento emptyMemento, TEventHandler eventHandler)
            where TEventHandler : IJsonEventHandler<TMemento>
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            var utf8JsonWriter = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions { Encoder = this.Options.Encoder, Indented = this.Options.WriteIndented, SkipValidation = false });

            return new JsonAggregateRoot<TMemento>(id, emptyMemento, this.jsonStore, bufferWriter, utf8JsonWriter, partitionKey, -1, -1, false, ReadOnlyMemory<byte>.Empty, this.Options);
        }

        /// <inheritdoc/>
        public Task<JsonAggregateRoot<TMemento>> Read<TMemento, TEventHandler>(Guid id, string partitionKey, TMemento emptyMemento, TEventHandler eventHandler)
            where TEventHandler : IJsonEventHandler<TMemento>
        {
            return this.ReadJson(this.ContainerClientFactory.GetContainerClient(), this.SnapshotReader, id, partitionKey, emptyMemento, eventHandler, this.Options);
        }

        /// <inheritdoc/>
        public Task<JsonAggregateRoot<TMemento>> Read<TMemento, TEventHandler>(Guid id, TMemento emptyMemento, TEventHandler eventHandler)
            where TEventHandler : IJsonEventHandler<TMemento>
        {
            return this.ReadJson(this.ContainerClientFactory.GetContainerClient(), this.SnapshotReader, id, id.ToString(), emptyMemento, eventHandler, this.Options);
        }

        /////// <summary>
        /////// Reads an event feed.
        /////// </summary>
        /////// <param name="eventHandler">The event reader capable of decoding and applying the event payloads for this aggregate root.</param>
        /////// <param name="pageSizeHint">A hint as to the number of items you get in a page of results.</param>
        /////// <param name="continuationToken">The (optional) continuation token to resume the feed from a particular point.</param>
        /////// <param name="cancellationToken">The cancellation token to terminate reading the feed.</param>
        /////// <returns>A <see cref="Task{TResult}"/> which completes when the feed terminates.</returns>
        ////public Task ReadFeed(IEventFeedHandler eventHandler, int pageSizeHint, string? continuationToken, CancellationToken cancellationToken)
        ////{
        ////    return this.ReadFeed(this.ContainerClientFactory.GetContainerClient(), eventHandler, pageSizeHint, continuationToken, this.Options, cancellationToken);
        ////}

        /////// <summary>
        /////// Reads an aggregate root.
        /////// </summary>
        /////// <typeparam name="TEventHandler">The type of the event reader for the aggregate root. This is an <see cref="IJsonEventFeedHandler"/>.</typeparam>
        /////// <param name="eventHandler">The event reader capable of decoding and applying the event payloads for this aggregate root.</param>
        /////// <param name="pageSizeHint">A hint as to the number of items you get in a page of results.</param>
        /////// <param name="continuationToken">The (optional) continuation token to resume the feed from a particular point.</param>
        /////// <param name="cancellationToken">The cancellation token to terminate reading the feed.</param>
        /////// <returns>A <see cref="Task{TResult}"/> which completes when the feed terminates.</returns>
        ////public Task ReadFeedJson<TEventHandler>(TEventHandler eventHandler, int pageSizeHint, string? continuationToken, CancellationToken cancellationToken)
        ////    where TEventHandler : IJsonEventFeedHandler
        ////{
        ////    return this.ReadFeedJson(this.ContainerClientFactory.GetContainerClient(), eventHandler, pageSizeHint, continuationToken, this.Options, cancellationToken);
        ////}

        private Task<JsonAggregateRoot<TMemento>> Read<TMemento>(BlobContainerClient containerClient, TSnapshotReader snapshotReader, Guid id, TMemento emptyMemento, IEventHandler<TMemento> eventHandler, JsonSerializerOptions options)
        {
            return this.ReadJson(containerClient, snapshotReader, id, id.ToString(), emptyMemento, new JsonAggregateRoot<TMemento>.JsonEventHandlerOverJsonSerializer(eventHandler, options), options);
        }

        private async Task<JsonAggregateRoot<TMemento>> ReadJson<TMemento, TEventHandler>(BlobContainerClient containerClient, TSnapshotReader snapshotReader, Guid id, string partitionKey, TMemento emptyMemento, TEventHandler eventHandler, JsonSerializerOptions options)
            where TEventHandler : IJsonEventHandler<TMemento>
        {
            long commitSequenceNumber = -1;
            long eventSequenceNumber = -1;
            TMemento memento = emptyMemento;
            long head = 0L;
            ReadOnlyMemory<byte> metadata = ReadOnlyMemory<byte>.Empty;

            Snapshot<TMemento>? snapshot = await snapshotReader.Read<TMemento>(id, long.MaxValue).ConfigureAwait(false);

            if (snapshot.HasValue)
            {
                commitSequenceNumber = snapshot.Value.CommitSequenceNumber;
                eventSequenceNumber = snapshot.Value.EventSequenceNumber;
                memento = snapshot.Value.Memento;
                head = AzureBlobStreamStore.GetHead(snapshot.Value.StoreMetadata.Span);
                metadata = snapshot.Value.StoreMetadata;
            }

            AppendBlobClient blobClient = containerClient.GetAppendBlobClient(AzureBlobStreamStore.GetBlobName(id));

            try
            {
                Response<BlobDownloadInfo> response = await blobClient.DownloadAsync(new HttpRange(head)).ConfigureAwait(false);

                try
                {
                    (memento, commitSequenceNumber, eventSequenceNumber) = this.ProcessStream(id, commitSequenceNumber, eventSequenceNumber, memento, eventHandler, response.Value.Content, response.Value.ContentLength, options.DefaultBufferSize);

                    metadata = AzureBlobStreamStore.GetMetadata(head + response.Value.ContentLength, response.Value.Details.ETag);
                }
                finally
                {
                    response.Value.Dispose();
                }
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // NOP - we don't mind a 404; we just return the empty values.
            }

            var bufferWriter = new ArrayBufferWriter<byte>();
            var utf8JsonWriter = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions { Encoder = options.Encoder, Indented = options.WriteIndented, SkipValidation = false });
            return new JsonAggregateRoot<TMemento>(id, memento!, this.jsonStore, bufferWriter, utf8JsonWriter, partitionKey, eventSequenceNumber, commitSequenceNumber, false, metadata, options);
        }

        ////private Task ReadFeed(BlobContainerClient containerClient, IEventFeedHandler eventHandler, int pageSizeHint, string? continuationToken, JsonSerializerOptions options, CancellationToken cancallationToken)
        ////{
        ////    return this.ReadFeedJson(containerClient, new JsonEventFeed.JsonEventFeedHandlerOverJsonSerializer(eventHandler, options), pageSizeHint, continuationToken, options, cancallationToken);
        ////}

        ////private async Task ReadFeedJson<TEventHandler>(BlobContainerClient containerClient, TEventHandler eventHandler, int pageSizeHint, string? continuationToken, JsonSerializerOptions options, CancellationToken cancellationToken)
        ////     where TEventHandler : IJsonEventFeedHandler
        ////{
        ////    var requestOptions = new ChangeFeedRequestOptions { PageSizeHint = pageSizeHint };
        ////    FeedIterator iterator = containerClient.GetChangeFeedStreamIterator(continuationToken is null ? ChangeFeedStartFrom.Beginning() : ChangeFeedStartFrom.ContinuationToken(continuationToken), requestOptions);

        ////    while (iterator.HasMoreResults && !cancellationToken.IsCancellationRequested)
        ////    {
        ////        using ResponseMessage response = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);

        ////        if (cancellationToken.IsCancellationRequested)
        ////        {
        ////            break;
        ////        }

        ////        response.EnsureSuccessStatusCode();

        ////        this.ProcessEventFeedStream(eventHandler, response.Content, options.DefaultBufferSize, cancellationToken);
        ////        await eventHandler.HandleBatchComplete(response.ContinuationToken).ConfigureAwait(false);
        ////    }
        ////}

        ////private void ProcessEventFeedStream<TEventHandler>(TEventHandler eventHandler, Stream content, int defaultBufferSize, CancellationToken cancellationToken)
        ////    where TEventHandler : IJsonEventFeedHandler
        ////{
        ////    var streamReader = new Utf8JsonStreamReader(content, defaultBufferSize);

        ////    if (!FindDocumentsProperty(ref streamReader))
        ////    {
        ////        throw new JsonException("Unable to find the Documents property in the output stream.");
        ////    }

        ////    if (!FindDocumentsArray(ref streamReader))
        ////    {
        ////        throw new JsonException("The Documents propery was expected to be a JSON array.");
        ////    }

        ////    // We are now at the start of an array of commits
        ////    JsonEventFeed.ProcessCommits(eventHandler, ref streamReader, cancellationToken);
        ////}

        private (TMemento, long, long) ProcessStream<TMemento, TEventHandler>(Guid aggregateId, long commitSequenceNumber, long eventSequenceNumber, TMemento memento, TEventHandler eventHandler, Stream content, long contentLength, int defaultBufferSize)
            where TEventHandler : IJsonEventHandler<TMemento>
        {
            using var streamProvider = new StreamProvider(content, contentLength, AzureBlobStreamStore.Utf8BlockSeparator, defaultBufferSize);

            while (streamProvider.NextStream(out Stream? stream))
            {
                var streamReader = new Utf8JsonStreamReader(stream, defaultBufferSize);

                try
                {
                    streamReader.Read();
                    (memento, eventSequenceNumber) = JsonAggregateRoot<TMemento>.ProcessCommit(aggregateId, commitSequenceNumber, eventSequenceNumber, memento, eventHandler, ref streamReader);
                    commitSequenceNumber += 1;
                }
                finally
                {
                    streamReader.Dispose();
                }
            }

            return (memento, commitSequenceNumber, eventSequenceNumber);
        }
    }
}
