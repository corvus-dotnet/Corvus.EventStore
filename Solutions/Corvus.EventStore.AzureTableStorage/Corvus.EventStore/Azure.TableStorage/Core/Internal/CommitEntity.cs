// <copyright file="CommitEntity.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core.Internal
{
    using System;
    using Corvus.EventStore.Core;
    using Microsoft.Azure.Cosmos.Table;

    /// <summary>
    /// A table entity for a commit.
    /// </summary>
    public class CommitEntity : TableEntityAdapter<Commit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommitEntity"/> class.
        /// </summary>
        public CommitEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommitEntity"/> class.
        /// </summary>
        /// <param name="commit">The commit for which to build the table entity.</param>
        public CommitEntity(in Commit commit)
            : base(commit, BuildPK(commit.AggregateId), BuildRK(commit.SequenceNumber))
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
            return fromSequenceNumber.ToString("d:21");
        }
    }
}
