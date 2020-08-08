// <copyright file="SerializedSnapshotEntity.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core.Internal
{
    using System;
    using Corvus.EventStore.Snapshots;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// A table entity for a serializedSnapshot.
    /// </summary>
    public class SerializedSnapshotEntity : TableEntityAdapter<SerializedSnapshot>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedSnapshotEntity"/> class.
        /// </summary>
        public SerializedSnapshotEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedSnapshotEntity"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot for which to buidl the adapter.</param>
        public SerializedSnapshotEntity(SerializedSnapshot snapshot)
            : base(snapshot, BuildPK(snapshot.AggregateId), BuildRK(snapshot.CommitSequenceNumber))
        {
        }

        /// <summary>
        /// Builds a PK from an aggregate ID.
        /// </summary>
        /// <param name="aggregateId">The aggregate ID from which to create the parittion key.</param>
        /// <returns>The formatted partition key.</returns>
        internal static string BuildPK(Guid aggregateId)
        {
            return "AID" + aggregateId;
        }

        /// <summary>
        /// Builds a row key from a sequence number.
        /// </summary>
        /// <param name="fromSequenceNumber">The sequence number from which to build the row key.</param>
        /// <returns>The row key.</returns>
        internal static string BuildRK(long fromSequenceNumber)
        {
            // We need the row key to be reverse sequence number so we can get the most
            // recent snapshot since a particular time.
            return (long.MaxValue - fromSequenceNumber).ToString("d:21");
        }
    }
}
