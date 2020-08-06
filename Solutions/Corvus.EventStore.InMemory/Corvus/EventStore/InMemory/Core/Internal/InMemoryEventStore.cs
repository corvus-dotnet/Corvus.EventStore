// <copyright file="InMemoryEventStore.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Core.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Corvus.EventStore.Core;

    /// <summary>
    /// Underlying store used by <see cref="InMemoryEventReader"/> and <see cref="InMemoryEventReader"/>.
    /// </summary>
    /// <remarks>This is the equivalent to "SQL Server" or "Cosmos DB" for other stores - but we've had to implement it ourselves. We could have used a popular in-memory database instead.</remarks>
    public class InMemoryEventStore
    {
        private readonly ConcurrentDictionary<Guid, CommitList> store =
            new ConcurrentDictionary<Guid, CommitList>();

        /// <summary>
        /// Reads events from the store for an aggregate.
        /// </summary>
        /// <param name="aggregateId">The Id of the aggregate to retrieve events for.</param>
        /// <param name="fromSequenceNumber">The minimum <see cref="Commit.SequenceNumber"/> to retrieve.</param>
        /// <param name="toSequenceNumber">The maximum <see cref="Commit.SequenceNumber"/> to retreive.</param>
        /// <param name="maxItems">The maximum number of items to return.</param>
        /// <returns>The results, contained in an <see cref="EventReaderResult"/>.</returns>
        public ValueTask<EventReaderResult> ReadCommitsAsync(Guid aggregateId, long fromSequenceNumber, long toSequenceNumber, int maxItems)
        {
            if (!this.store.TryGetValue(aggregateId, out CommitList list))
            {
                return new ValueTask<EventReaderResult>(new EventReaderResult(Enumerable.Empty<Commit>(), null));
            }

            Commit[] commits = list.Commits
                .Where(item => item.Key >= fromSequenceNumber && item.Key <= toSequenceNumber)
                .Take(maxItems)
                .Select(item => item.Value)
                .ToArray();

            if (commits.Length == maxItems)
            {
                var continuationToken = new ContinuationToken(aggregateId, fromSequenceNumber + maxItems, toSequenceNumber, maxItems);

                byte[] encodedContinuationToken = JsonSerializer.SerializeToUtf8Bytes(continuationToken);
                return new ValueTask<EventReaderResult>(new EventReaderResult(commits, encodedContinuationToken));
            }

            return new ValueTask<EventReaderResult>(new EventReaderResult(commits, null));
        }

        /// <summary>
        /// Reads the next block in a result set initially acquired by calling <see cref="ReadCommitsAsync(Guid, long, long, int)"/>.
        /// </summary>
        /// <param name="encodedContinuationToken">A continuation token returned from a previous call that can be used to
        /// obtain the next set of results.</param>
        /// <returns>The results, contained in an <see cref="EventReaderResult"/>.</returns>
        public ValueTask<EventReaderResult> ReadAsync(ReadOnlySpan<byte> encodedContinuationToken)
        {
            ContinuationToken continuationToken = JsonSerializer.Deserialize<ContinuationToken>(encodedContinuationToken);

            return this.ReadCommitsAsync(continuationToken.AggregateId, continuationToken.FromSequenceNumber, continuationToken.ToSequenceNumber, continuationToken.MaxItems);
        }

        /// <summary>
        /// Writes the supplied commit to the store as a single transaction.
        /// </summary>
        /// <param name="commit">The commit to write.</param>
        /// <returns>A task that completes when the events have been written to the store.</returns>
        public Task WriteCommitAsync(Commit commit)
        {
            this.store.AddOrUpdate(
                commit.AggregateId,
                seq =>
                {
                    if (commit.SequenceNumber != 0)
                    {
                        throw new InMemoryEventStoreEventOutOfSequenceException();
                    }

                    return new CommitList(0, ImmutableDictionary<long, Commit>.Empty.Add(0, commit));
                },
                (aggregateId, list) =>
                {
                    if (list.LastCommitSequenceNumber >= commit.SequenceNumber)
                    {
                        throw new InMemoryEventStoreConcurrencyException();
                    }

                    if (list.LastCommitSequenceNumber != commit.SequenceNumber - 1)
                    {
                        throw new InMemoryEventStoreEventOutOfSequenceException();
                    }

                    return list.AddCommits(ImmutableArray<Commit>.Empty.Add(commit));
                });

            return Task.CompletedTask;
        }

        private readonly struct ContinuationToken
        {
            public ContinuationToken(Guid aggregateId, long fromSequenceNumber, long toSequenceNumber, int maxItems)
            {
                this.FromSequenceNumber = fromSequenceNumber;
                this.ToSequenceNumber = toSequenceNumber;
                this.MaxItems = maxItems;
                this.AggregateId = aggregateId;
            }

            public long FromSequenceNumber { get; }

            public long ToSequenceNumber { get; }

            public int MaxItems { get; }

            public Guid AggregateId { get; }
        }

        private readonly struct CommitList
        {
            public CommitList(long lastCommitSequenceNumber, ImmutableDictionary<long, Commit> commits)
            {
                this.Commits = commits;
                this.LastCommitSequenceNumber = lastCommitSequenceNumber;
            }

            public long LastCommitSequenceNumber { get; }

            public ImmutableDictionary<long, Commit> Commits { get; }

            public CommitList AddCommits(ImmutableArray<Commit> commits)
            {
                return new CommitList(
                    this.LastCommitSequenceNumber + 1,
                    this.Commits.AddRange(commits.Select(commit => KeyValuePair.Create(commit.SequenceNumber, commit))));
            }
        }
    }
}
