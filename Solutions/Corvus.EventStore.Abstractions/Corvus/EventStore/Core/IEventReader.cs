// <copyright file="IEventReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for classes capable of reading events for an aggregate.
    /// </summary>
    public interface IEventReader
    {
        /// <summary>
        /// Read committed events from the store for an aggregate.
        /// </summary>
        /// <param name="aggregateId">The Id of the aggregate to retrieve commited events for.</param>
        /// <param name="partitionKey">The partition key of the aggregate.</param>
        /// <param name="fromSequenceNumber">The minimum <see cref="Commit.SequenceNumber"/> to retrieve.</param>
        /// <param name="toSequenceNumber">The maximum <see cref="Commit.SequenceNumber"/> to retreive.</param>
        /// <param name="maxItems">The maximum number of items to return.</param>
        /// <param name="cancellationToken">The cancellation token to abort the operation..</param>
        /// <returns>The results, contained in an <see cref="EventReaderResult"/>.</returns>
        ValueTask<EventReaderResult> ReadCommitsAsync(
            Guid aggregateId,
            string partitionKey,
            long fromSequenceNumber,
            long toSequenceNumber,
            int maxItems,
            CancellationToken cancellationToken);

        /// <summary>
        /// Reads the next block in a result set initially acquired by calling <see cref="ReadCommitsAsync(Guid, string, long, long, int, CancellationToken)"/>.
        /// </summary>
        /// <param name="continuationToken">A continuation token returned from a previous call that can be used to
        /// obtain the next set of results.</param>
        /// <param name="cancellationToken">The cancellation token to abort the operation.</param>
        /// <returns>The results, contained in an <see cref="EventReaderResult"/>.</returns>
        ValueTask<EventReaderResult> ReadCommitsAsync(ReadOnlySpan<byte> continuationToken, CancellationToken cancellationToken);
    }
}
