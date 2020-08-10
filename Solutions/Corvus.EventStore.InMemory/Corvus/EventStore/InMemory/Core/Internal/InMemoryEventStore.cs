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
        /// This represents the ordered set of commits across all aggregates. It is a lookup into the store.
        /// </summary>
        private object allStreamLock = new object();
        private ImmutableList<(int, Guid, int)> allStream = ImmutableList<(int, Guid, int)>.Empty;

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

            // It is possible that our thread could be murdered between writing the event commit and
            // writing the all stream. We would then have the even in the store, but not in this stream.
            // That would not be highly desirable. A more robust implementation would use the separate process
            // to write the all stream, and checkpoint where it had got to.
            // There is also the issue that this could be interleaved with other updates for the aggregate (although
            // the commit itself will appear as an atomic unit). This a general issue with the events as they may
            // be delivered out-of-order or more than once, when you are no longer in the context of a single commit. You have
            // to inspect the commit and determine if you are receving it "in order" or not.
            lock (this.allStreamLock)
            {
                this.allStream = this.allStream.Add((this.allStream.Count, commit.AggregateId, eventIndex));
            }

            return Task.CompletedTask;
        }

        private ValueTask<EventFeedResult> GetFeedCore(ImmutableArray<Guid> aggregateIds, ImmutableArray<string> partitionKeys, int maxItems, int initialCommitIndex)
        {
            // Because the allstream is an immutable list, we are safe to operate on it like this.
            ImmutableList<(int, Guid, int)> localAllStream = this.allStream;
            IEnumerable<(int, Guid, int)> stream = localAllStream.Skip(initialCommitIndex);

            if (aggregateIds.Any())
            {
                stream = stream.Where(item => aggregateIds.Contains(this.store[item.Item2].Commits[item.Item3].AggregateId));
            }

            if (partitionKeys.Any())
            {
                stream = stream.Where(item => partitionKeys.Contains(this.store[item.Item2].Commits[item.Item3].PartitionKey));
            }

            ImmutableArray<Commit>.Builder builder = ImmutableArray.CreateBuilder<Commit>();

            foreach ((int, Guid, int) current in stream)
            {
                Commit commit;
                if (!this.store.TryGetValue(current.Item2, out CommitList aggregateCommitList) || !aggregateCommitList.Commits.TryGetValue(current.Item3, out commit))
                {
                    throw new InvalidOperationException("The store `allStream` is out of sync with the individual aggregate stores, and should be rebuilt.");
                }

                builder.Add(commit);
                if (builder.Count == maxItems)
                {
                    break;
                }
            }

            // If we got to the end of the list, point at the next commit (even if it doesn't exist yet).
            return new ValueTask<EventFeedResult>(new EventFeedResult(builder.ToImmutable(), this.BuildCheckpoint(aggregateIds, partitionKeys, maxItems, localAllStream.Count)));
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
