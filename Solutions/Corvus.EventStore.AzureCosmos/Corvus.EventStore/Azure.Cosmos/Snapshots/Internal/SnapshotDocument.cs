// <copyright file="SnapshotDocument.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.Snapshots.Internal
{
    using System;
    using Corvus.EventStore.Snapshots;
    using Newtonsoft.Json;

    /// <summary>
    /// A document containing a commit.
    /// </summary>
    public class SnapshotDocument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotDocument"/> class.
        /// </summary>
        /// <param name="memento">The <see cref="Memento"/>.</param>
        /// <param name="aggregateId">The <see cref="AggregateId"/>.</param>
        /// <param name="commitSequenceNumber">The <see cref="CommitSequenceNumber"/>.</param>
        /// <param name="eventSequenceNumber">The <see cref="EventSequenceNumber"/>.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="id">The <see cref="Id"/>.</param>
        [JsonConstructor]
        public SnapshotDocument(string memento, Guid aggregateId, long commitSequenceNumber, long eventSequenceNumber, string partitionKey, string id)
        {
            this.Memento = memento;
            this.AggregateId = aggregateId;
            this.CommitSequenceNumber = commitSequenceNumber;
            this.EventSequenceNumber = eventSequenceNumber;
            this.PartitionKey = partitionKey;
            this.Id = id;
        }

        /// <summary>
        /// Constructs the commit document from the commit.
        /// </summary>
        /// <param name="snapshot">The <see cref="SerializedSnapshot"/>.</param>
        public SnapshotDocument(SerializedSnapshot snapshot)
        {
            this.PartitionKey = snapshot.PartitionKey;
            this.Id = snapshot.AggregateId + "__" + snapshot.CommitSequenceNumber;
            this.AggregateId = snapshot.AggregateId;
            this.CommitSequenceNumber = snapshot.CommitSequenceNumber;
            this.Memento = Convert.ToBase64String(snapshot.Memento.Span);
        }

        /// <summary>
        /// Gets or sets the memento as a base64 string.
        /// </summary>
        public string Memento { get; set; }

        /// <summary>
        /// Gets or sets the aggregate ID.
        /// </summary>
        public Guid AggregateId { get; set; }

        /// <summary>
        /// Gets or sets the sequence number of the commit.
        /// </summary>
        public long CommitSequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the sequence number of the event sequence number in the commit.
        /// </summary>
        public long EventSequenceNumber { get; set; }

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
        /// Gets the snapshot corresponding to this document.
        /// </summary>
        /// <returns>The snapshot for the document.</returns>
        public SerializedSnapshot GetSerializedSnapshot()
        {
            return new SerializedSnapshot(this.AggregateId, this.PartitionKey, this.CommitSequenceNumber, this.EventSequenceNumber, Convert.FromBase64String(this.Memento));
        }
    }
}
