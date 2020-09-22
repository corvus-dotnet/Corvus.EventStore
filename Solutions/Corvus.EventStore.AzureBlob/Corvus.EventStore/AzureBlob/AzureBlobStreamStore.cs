// <copyright file="AzureBlobStreamStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.AzureBlob
{
    using System;
    using System.Buffers;
    using System.Buffers.Binary;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Azure;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Azure.Storage.Blobs.Specialized;
    using Corvus.EventStore;

    /// <summary>
    /// A <see cref="IStreamStore"/> implementation over a Cosmos container.
    /// </summary>
    public class AzureBlobStreamStore : IStreamStore
    {
        /// <summary>
        /// The name of the AllStream blob.
        /// </summary>
        internal const string AllStreamBlob = "corvusallstream";

        /// <summary>
        /// This is an illegal UTF8 byte sequence, so it is good as a separator.
        /// </summary>
        internal static readonly ReadOnlyMemory<byte> Utf8BlockSeparator = new ReadOnlyMemory<byte>(new byte[] { 0x84, 0xDD });

        private static readonly AppendBlobRequestConditions ConditionsForCreate = new AppendBlobRequestConditions { IfNoneMatch = ETag.All };
        private static readonly AppendBlobCreateOptions CreateOptions = new AppendBlobCreateOptions { Conditions = ConditionsForCreate };

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStreamStore"/> struct.
        /// </summary>
        /// <param name="containerClient">The cosmos container for the JSON store.</param>
        public AzureBlobStreamStore(BlobContainerClient containerClient)
        {
            this.ContainerClient = containerClient;
        }

        private BlobContainerClient ContainerClient { get; }

        /// <inheritdoc/>
        public async Task<ReadOnlyMemory<byte>> Write(Stream stream, Guid aggregateId, long commitSequenceNumber, string partitionKey, ReadOnlyMemory<byte> storeMetadata)
        {
            AppendBlobClient appendBlobClient = this.ContainerClient.GetAppendBlobClient(GetBlobName(aggregateId));

            AppendBlobRequestConditions conditions = storeMetadata.IsEmpty ? ConditionsForCreate : new AppendBlobRequestConditions { IfMatch = new ETag(GetEtagFromMetadata(storeMetadata)) };

            try
            {
                long initialPosition = stream.Position;
                Stream streamToWrite = stream;

                if (commitSequenceNumber == 0)
                {
                    await appendBlobClient.CreateAsync(CreateOptions).ConfigureAwait(false);
                }
                else
                {
                    streamToWrite = new AppendStream(stream, Utf8BlockSeparator, ReadOnlyMemory<byte>.Empty);
                }

                Response<BlobAppendInfo> response = await appendBlobClient.AppendBlockAsync(streamToWrite, conditions: conditions).ConfigureAwait(false);
                long bytesWritten = stream.Position - initialPosition;
                long offset = long.Parse(response.Value.BlobAppendOffset);
                return GetMetadata(offset + bytesWritten, response.Value.ETag);
            }
            catch (RequestFailedException ex) when (ex.Status == 304 || ex.Status == 412 || ex.Status == 409)
            {
                throw new ConcurrencyException($"A commit for aggregate {aggregateId} with sequence number {commitSequenceNumber} has already been applied.");
            }
        }

        /// <summary>
        /// Gets the blob name for the given aggregate ID.
        /// </summary>
        /// <param name="aggregateId">The aggregate ID for which to get the blob name.</param>
        /// <returns>The blob name for the Aggregate ID.</returns>
        internal static string GetBlobName(Guid aggregateId)
        {
            return aggregateId.ToString();
        }

        /// <summary>
        /// Builds the store metadata from the bytes written, offset, and eTag.
        /// </summary>
        /// <param name="head">The pointer in the blob to the end of this record.</param>
        /// <param name="eTag">The etag for the record.</param>
        /// <returns>A <see cref="ReadOnlyMemory{T}"/> of byte containing the store metadata.</returns>
        /// <remarks>
        /// Our store metadata consists of:
        /// <list type="table">
        /// <item>8 bytes: a little-endian encoded long representing the offset in the blob of the end of this record ('head')</item>
        /// <item>N bytes: utf8 encoded text as the ETAG.</item>
        /// </list>
        /// </remarks>
        internal static ReadOnlyMemory<byte> GetMetadata(long head, ETag eTag)
        {
            // Rent a small buffer.
            byte[] buffer = ArrayPool<byte>.Shared.Rent(1024);
            Span<byte> span = buffer.AsSpan();
            try
            {
                // Write the head as a little endian byte array
                BinaryPrimitives.WriteInt64LittleEndian(span.Slice(0, 8), head);

                // Write the etag as a UTF8 encoded byte array
                string etagString = eTag.ToString();
                UTF8Encoding.UTF8.GetEncoder().Convert(etagString.AsSpan(), span.Slice(8), true, out int charsUsed, out int bytesUsed, out bool completed);
                if (!completed)
                {
                    throw new ArgumentException("The ETag exceeded the maximum length permitted.");
                }

                // Finally, copy out the bit of the buffer we used and return it.
                return buffer.AsSpan(0, bytesUsed + 8).ToArray();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Gets the etag from the store metadata.
        /// </summary>
        /// <param name="storeMetadata">The store metadata.</param>
        /// <returns>The etag from the metadata, or null if the metadata is empty.</returns>
        internal static string GetEtagFromMetadata(ReadOnlyMemory<byte> storeMetadata)
        {
            return UTF8Encoding.UTF8.GetString(storeMetadata.Slice(8).Span);
        }

        /// <summary>
        /// Gets the offset from the store metadata.
        /// </summary>
        /// <param name="storeMetadata">The store metadata.</param>
        /// <returns>The etag from the metadata, or null if the metadata is empty.</returns>
        internal static long GetHead(ReadOnlySpan<byte> storeMetadata)
        {
            return BinaryPrimitives.ReadInt64LittleEndian(storeMetadata.Slice(0, 8));
        }
    }
}
