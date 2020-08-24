// <copyright file="BatchedCommit.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core.Internal
{
    using System;
    using System.Text.Json.Serialization;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// A commit in a batch persisted to table storage.
    /// </summary>
    internal struct BatchedCommit
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchedCommit"/> struct.
        /// </summary>
        /// <param name="commit">The commit from which to initialize this batched commit.</param>
        /// <param name="partitionKey">The partition key for the batched commit.</param>
        /// <param name="commitSequenceNumber">The sequence number for this commit.</param>
        public BatchedCommit(DynamicTableEntity commit, string partitionKey, long commitSequenceNumber)
            : this()
        {
            this.PartitionKey = partitionKey;
            this.RowKey = commitSequenceNumber.ToString("D21");
            this.CommitAggregateId = commit.Properties[TableStorageEventWriter.CommitAggregateId].GuidValue!.Value;
            this.CommitPartitionKey = commit.Properties[TableStorageEventWriter.CommitPartitionKey].StringValue;
            this.CommitSequenceNumber = commit.Properties[TableStorageEventWriter.CommitSequenceNumber].Int64Value!.Value;
            this.CommitTimestamp = commit.Properties[TableStorageEventWriter.CommitTimestamp].Int64Value!.Value;
            this.CommitEvents = commit.Properties[TableStorageEventWriter.CommitEvents].BinaryValue;
        }

        /// <summary>
        /// Gets or sets the partition key for the commit.
        /// </summary>
        [JsonIgnore]
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the row key for the commit.
        /// </summary>
        [JsonIgnore]
        public string RowKey { get; set; }

        /// <summary>
        /// Gets or sets the aggregate ID.
        /// </summary>
        public Guid CommitAggregateId { get; set; }

        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        public string CommitPartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the sequence number.
        /// </summary>
        public long CommitSequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public long CommitTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the serialized events.
        /// </summary>
        public ReadOnlyMemory<byte> CommitEvents { get; set; }

        /// <summary>
        /// Gets the length of the record.
        /// </summary>
        public int Length => sizeof(long) + sizeof(long) + ((this.CommitPartitionKey.Length + this.PartitionKey.Length + this.RowKey.Length) * sizeof(char)) + 16 + this.CommitEvents.Length;
    }
}
