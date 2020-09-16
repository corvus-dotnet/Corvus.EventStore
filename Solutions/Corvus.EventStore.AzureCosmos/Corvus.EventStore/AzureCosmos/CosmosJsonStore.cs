// <copyright file="CosmosJsonStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.AzureCosmos
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Corvus.EventStore;
    using Corvus.EventStore.Json;
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// A <see cref="IJsonStore"/> implementation over a Cosmos container.
    /// </summary>
    public class CosmosJsonStore : IJsonStore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosJsonStore"/> struct.
        /// </summary>
        /// <param name="container">The cosmos container for the JSON store.</param>
        public CosmosJsonStore(Container container)
        {
            this.Container = container;
        }

        private Container Container { get; }

        /// <inheritdoc/>
        public async Task Write(Stream stream, Guid aggregateId, long commitSequenceNumber, JsonEncodedText encodedPartitionKey)
        {
            var options = new ItemRequestOptions { EnableContentResponseOnWrite = false };

            ResponseMessage response = await this.Container.CreateItemStreamAsync(stream, new PartitionKey(encodedPartitionKey.ToString()), options).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new ConcurrencyException($"An commit for aggregate {aggregateId} with sequence number {commitSequenceNumber} has already been applied.");
            }

            // Just throw if there was another reason for the failure.
            response.EnsureSuccessStatusCode();
        }
    }
}
