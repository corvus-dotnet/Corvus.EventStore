// <copyright file="AzureBlobJsonStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.AzureCosmos
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Specialized;
    using Corvus.EventStore;
    using Corvus.EventStore.Json;

    /// <summary>
    /// A <see cref="IJsonStore"/> implementation over a Cosmos container.
    /// </summary>
    public class AzureBlobJsonStore : IJsonStore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobJsonStore"/> struct.
        /// </summary>
        /// <param name="containerClient">The cosmos container for the JSON store.</param>
        public AzureBlobJsonStore(BlobContainerClient containerClient)
        {
            this.ContainerClient = containerClient;
        }

        private BlobContainerClient ContainerClient { get; }

        /// <inheritdoc/>
        public Task Write(Stream stream, Guid aggregateId, long commitSequenceNumber, JsonEncodedText encodedPartitionKey)
        {
            ////var appendBlobClient = this.ContainerClient.GetAppendBlobClient(aggregateId.ToString())
            ////ResponseMessage response = await this.ContainerClient.GetAppendBlobClient((stream,).ConfigureAwait(false);

            ////if (response.IsSuccessStatusCode)
            ////{
            ////    return;
            ////}

            ////if (response.StatusCode == HttpStatusCode.Conflict)
            ////{
            ////    throw new ConcurrencyException($"An commit for aggregate {aggregateId} with sequence number {commitSequenceNumber} has already been applied.");
            ////}

            ////// Just throw if there was another reason for the failure.
            ////response.EnsureSuccessStatusCode();
            throw new NotImplementedException();
        }
    }
}
