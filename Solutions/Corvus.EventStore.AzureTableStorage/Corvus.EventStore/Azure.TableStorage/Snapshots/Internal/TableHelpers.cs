// <copyright file="TableHelpers.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core.Internal
{
    using System;

    /// <summary>
    /// Helpers for table storage.
    /// </summary>
    internal static class TableHelpers
    {
        /// <summary>
        /// Builds a partition key from the aggregate ID.
        /// </summary>
        /// <param name="aggregateId">The aggregate ID.</param>
        /// <returns>The partition key.</returns>
        public static string BuildPK(Guid aggregateId)
        {
            return "AID" + aggregateId.ToString("D");
        }

        /// <summary>
        /// Builds a row key from the sequence number.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number.</param>
        /// <returns>The partition key.</returns>
        public static string BuildRK(long sequenceNumber)
        {
            return sequenceNumber.ToString("D21");
        }
    }
}
