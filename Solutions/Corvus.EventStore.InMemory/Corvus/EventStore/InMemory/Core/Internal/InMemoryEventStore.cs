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
    using Corvus.Retry;

    /// <summary>
    /// Underlying store used by <see cref="InMemoryEventReader"/> and <see cref="InMemoryEventReader"/>.
    /// </summary>
    /// <remarks>This is the equivalent to "SQL Server" or "Cosmos DB" for other stores - but we've had to implement it ourselves. We could have used a popular in-memory database instead.</remarks>
    public class InMemoryEventStore
    {
        private readonly ConcurrentDictionary<Guid, CommitList> store =
            new ConcurrentDictionary<Guid, CommitList>();

        private ImmutableDictionary<Guid, long> aggregateIndices = ImmutableDictionary<Guid, long>.Empty;
        private ImmutableList<(Guid, long)> allStream = ImmutableList<(Guid, long)>.Empty;

        private ReliableTaskRunner? allStreamTask;

        /// <summary>
        /// Gets the feed from the beginning, given the specified filter.
        /// </summary>
        /// <param name="filter">The filter to apply to the feed.</param>
        /// <param name="maxItems">The maximum number of items to return in the result.</param>
        /// <returns>A <see cref="ValueTask{T}"/> of type <see cref="EventFeedResult"/>.</returns>
        public ValueTask<EventFeedResult> GetFeed(EventFeedFilter filter, int maxItems)
        {
            return this.GetFeedCore(filter.AggregateIds, filter.PartitionKeys, maxItems, 0);
        }

        /// <summary>
        /// Gets the feed from a given checkpoint.
        /// </summary>
        /// <param name="fromCheckpoint">The checkpoint from which to read the feed.</param>
        /// <returns>A <see cref="ValueTask{T}"/> of type <see cref="EventFeedResult"/>.</returns>
        /// <remarks>The <paramref name="fromCheckpoint"/> will have been obtained from the <see cref="EventFeedResult"/> returned by
        /// a prior call to <see cref="GetFeed(EventFeedFilter, int)"/> or <see cref="GetFeed(ReadOnlyMemory{byte})"/>.
        /// It encapsulates everything needed to continue reading the feed with the same filters applied.</remarks>
        public ValueTask<EventFeedResult> GetFeed(ReadOnlyMemory<byte> fromCheckpoint)
        {
            Checkpoint cp = JsonSerializer.Deserialize<Checkpoint>(fromCheckpoint.Span);
            return this.GetFeedCore(cp.AggregateIds, cp.PartitionKeys, cp.MaxItems, cp.CommitIndex);
        }

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
            int eventIndex = 0;
            this.store.AddOrUpdate(
                commit.AggregateId,
                aggregateId =>
                {
                    if (commit.SequenceNumber != 0)
                    {
                        throw new InMemoryEventStoreEventOutOfSequenceException();
                    }

                    ImmutableDictionary<long, Commit>.Builder builder = ImmutableDictionary.CreateBuilder<long, Commit>();
                    builder.Add(0, commit);
                    eventIndex = 0;
                    return new CommitList(0, builder.ToImmutable());
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

                    eventIndex = list.Commits.Count;
                    return list.AddCommits(ImmutableArray.Create(commit));
                });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Starts the all stream builder.
        /// </summary>
        public void StartAllStreamBuilder()
        {
            if (this.allStreamTask is null)
            {
                this.allStreamTask = ReliableTaskRunner.Run(token =>
                {
                    return Task.Factory.StartNew(
                        () =>
                        {
                            this.allStream = ImmutableList<(Guid, long)>.Empty;
                            this.aggregateIndices = ImmutableDictionary<Guid, long>.Empty;

                            while (true)
                            {
                                var allStream = this.allStream.ToBuilder();
                                var aggregateIndices = this.aggregateIndices.ToBuilder();
                                foreach (KeyValuePair<Guid, CommitList> item in this.store)
                                {
                                    CommitList list = this.store[item.Key];
                                    if (this.aggregateIndices.TryGetValue(item.Key, out long index))
                                    {
                                        if (list.LastCommitSequenceNumber == index)
                                        {
                                            // We are up-to-date
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        index = -1;
                                    }

                                    for (long i = index + 1; i <= list.LastCommitSequenceNumber; ++i)
                                    {
                                        allStream.Add((item.Key, i));
                                    }

                                    aggregateIndices[item.Key] = list.LastCommitSequenceNumber;
                                }

                                // Switch in the new all stream info
                                this.allStream = allStream.ToImmutable();

                                // It is conceivable that we could die between these two operations "for some reason"
                                // in which case the in-memory store would be hosed, and it would be built from scratch
                                // when this thread restarts.
                                this.aggregateIndices = aggregateIndices.ToImmutable();

                                if (token.IsCancellationRequested)
                                {
                                    return Task.CompletedTask;
                                }
                            }
                        }, token);
                });
            }
        }

        /// <summary>
        /// Stop the all stream builder if it was already running.
        /// </summary>
        /// <returns>A <see cref="Task"/> which completes when the all stream builder has stopped.</returns>
        public Task StopAllStreamBuilderAsync()
        {
            if (this.allStreamTask is null)
            {
                return Task.CompletedTask;
            }

            return this.allStreamTask.StopAsync();
        }

        private ValueTask<EventFeedResult> GetFeedCore(ImmutableArray<Guid> aggregateIds, ImmutableArray<string> partitionKeys, int maxItems, int initialCommitIndex)
        {
            // Because the allstream is an immutable list, we are safe to operate on it like this.
            ImmutableList<(Guid, long)> localAllStream = this.allStream;

            bool hasAggregateIds = aggregateIds.Any();
            bool hasParitionKeys = partitionKeys.Any();

            ImmutableArray<Commit>.Builder builder = ImmutableArray.CreateBuilder<Commit>();

            int commitIndex = initialCommitIndex;
            while (commitIndex < localAllStream.Count)
            {
                (Guid, long) current = localAllStream[commitIndex];
                if (!this.store.TryGetValue(current.Item1, out CommitList aggregateCommitList) || !aggregateCommitList.Commits.TryGetValue(current.Item2, out Commit commit))
                {
                    throw new InvalidOperationException("The store `allStream` is out of sync with the individual aggregate stores, and should be rebuilt.");
                }

                commitIndex++;

                if (hasAggregateIds && !aggregateIds.Contains(commit.AggregateId))
                {
                    continue;
                }

                if (hasParitionKeys && !partitionKeys.Contains(commit.PartitionKey))
                {
                    continue;
                }

                builder.Add(commit);

                if (builder.Count == maxItems)
                {
                    break;
                }
            }

            // If we got to the end of the list, point at the next commit (even if it doesn't exist yet).
            return new ValueTask<EventFeedResult>(new EventFeedResult(builder.ToImmutable(), this.BuildCheckpoint(aggregateIds, partitionKeys, maxItems, initialCommitIndex + builder.Count)));
        }

        private ReadOnlyMemory<byte> BuildCheckpoint(ImmutableArray<Guid> aggregateIds, ImmutableArray<string> partitionKeys, int maxItems, int commitIndex)
        {
            return JsonSerializer.SerializeToUtf8Bytes(new Checkpoint(commitIndex, aggregateIds, partitionKeys, maxItems));
        }

        private struct ContinuationToken
        {
            public ContinuationToken(Guid aggregateId, long fromSequenceNumber, long toSequenceNumber, int maxItems)
            {
                this.FromSequenceNumber = fromSequenceNumber;
                this.ToSequenceNumber = toSequenceNumber;
                this.MaxItems = maxItems;
                this.AggregateId = aggregateId;
            }

            public long FromSequenceNumber { get; set; }

            public long ToSequenceNumber { get; set; }

            public int MaxItems { get; set; }

            public Guid AggregateId { get; set; }
        }

        private struct Checkpoint
        {
            public Checkpoint(int commitIndex, ImmutableArray<Guid> aggregateIds, ImmutableArray<string> partitionKeys, int maxItems)
            {
                this.CommitIndex = commitIndex;
                this.AggregateIds = aggregateIds;
                this.PartitionKeys = partitionKeys;
                this.MaxItems = maxItems;
            }

            public int CommitIndex { get; set; }

            public ImmutableArray<Guid> AggregateIds { get; set; }

            public ImmutableArray<string> PartitionKeys { get; set; }

            public int MaxItems { get; set; }
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
