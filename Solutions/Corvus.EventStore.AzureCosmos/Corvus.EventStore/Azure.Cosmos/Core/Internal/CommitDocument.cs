// <copyright file="CommitDocument.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.Core.Internal
{
    using System;
    using Corvus.EventStore.Azure.Cosmos.Serialization;
    using Corvus.EventStore.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// A document containing a commit.
    /// </summary>
    public class CommitDocument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommitDocument"/> class.
        /// </summary>
        /// <param name="serializedCommit">The <see cref="SerializedCommit"/>.</param>
        /// <param name="aggregateId">The <see cref="AggregateId"/>.</param>
        /// <param name="sequenceNumber">The <see cref="SequenceNumber"/>.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="id">The <see cref="Id"/>.</param>
        [JsonConstructor]
        public CommitDocument(string serializedCommit, Guid aggregateId, long sequenceNumber, string partitionKey, string id)
        {
            this.SerializedCommit = serializedCommit;
            this.AggregateId = aggregateId;
            this.SequenceNumber = sequenceNumber;
            this.PartitionKey = partitionKey;
            this.Id = id;
        }

        /// <summary>
        /// Constructs the commit document from the commit.
        /// </summary>
        /// <param name="commit">The commit.</param>
        public CommitDocument(in Commit commit)
        {
            this.SerializedCommit = Utf8JsonSerializer.Serialize(commit);
            this.Id = commit.AggregateId + "__" + commit.SequenceNumber;
            this.AggregateId = commit.AggregateId;
            this.SequenceNumber = commit.SequenceNumber;
            this.PartitionKey = commit.PartitionKey;
        }

        /// <summary>
        /// Gets or sets the <see cref="Commit"/>.
        /// </summary>
        public string SerializedCommit { get; set; }

        /// <summary>
        /// Gets or sets the aggregate ID.
        /// </summary>
        public Guid AggregateId { get; set; }

        /// <summary>
        /// Gets or sets the sequence number of the commit.
        /// </summary>
        public long SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the document ID.
        /// </summary>
        /// <remarks>
        /// This is composed from the aggregate ID and the sequence number.
        /// </remarks>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Get the deserialized commit.
        /// </summary>
        /// <returns>The deserialized commit.</returns>
        public Commit GetCommit()
        {
            return Utf8JsonSerializer.Deserialize<Commit>(this.SerializedCommit);
        }
    }
}
