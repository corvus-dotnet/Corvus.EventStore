﻿// <copyright file="CosmosStreamStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.AzureCosmos
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Corvus.EventStore;
    using Microsoft.Azure.Cosmos;

    /// <summary>
    /// A <see cref="IStreamStore"/> implementation over a Cosmos container.
    /// </summary>
    public class CosmosStreamStore : IStreamStore
    {
        private static readonly ItemRequestOptions Options = new ItemRequestOptions { EnableContentResponseOnWrite = false };

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosStreamStore"/> struct.
        /// </summary>
        /// <param name="container">The cosmos container for the JSON store.</param>
        public CosmosStreamStore(Container container)
        {
            this.Container = container;
        }

        private Container Container { get; }

        /// <inheritdoc/>
        public async Task<ReadOnlyMemory<byte>> Write(Stream stream, Guid aggregateId, long commitSequenceNumber, string partitionKey, ReadOnlyMemory<byte> storeMetadata)
        {
            ResponseMessage response = await this.Container.CreateItemStreamAsync(stream, new PartitionKey(partitionKey), Options).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return ReadOnlyMemory<byte>.Empty;
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new ConcurrencyException($"An commit for aggregate {aggregateId} with sequence number {commitSequenceNumber} has already been applied.");
            }

            // Just throw if there was another reason for the failure.
            response.EnsureSuccessStatusCode();

            // We will never reach this code because response.IsSuccessStatusCode was false, so EnsureSuccessStatusCode() must throw.
            throw new Exception("response.EnsureSuccessStatusCode() did not throw when previously response.IsSuccessStatusCode was false.");
        }
    }
}
