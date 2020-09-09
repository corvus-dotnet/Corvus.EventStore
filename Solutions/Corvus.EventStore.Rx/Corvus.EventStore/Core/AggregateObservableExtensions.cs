// <copyright file="AggregateObservableExtensions.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using Corvus.EventStore.Aggregates;

    /// <summary>
    /// Extension methods to create Observables over aggregates.
    /// </summary>
    public static class AggregateObservableExtensions
    {
        /// <summary>
        /// Creates an observable from an aggregate reader.
        /// </summary>
        /// <typeparam name="TEventFeed">The type of the <see cref="IAggregateReader"/>.</typeparam>
        /// <param name="feed">The <see cref="IEventFeed"/>.</param>
        /// <param name="filter">The filter to use.</param>
        /// <param name="maxItems">The maximum number of items to return in a batch.</param>
        /// <param name="rateLimit">The (optional) rate limit.</param>
        /// <returns>The <see cref="IObservable{T}"/> of <see cref="SerializedEvent"/>s.</returns>
        /// <remarks>Note that this observable will always start from the beginning of the feed, and has no means of restarting
        /// from a particular checkpoint. This will typically be used in in-memory implementations that do not offer recoverability.</remarks>
        public static EventFeedObservable AsObservable<TEventFeed>(
            this TEventFeed feed,
            EventFeedFilter filter,
            int maxItems = 1000,
            TimeSpan? rateLimit = null)
            where TEventFeed : IEventFeed
        {
            return feed.AsObservable(CheckpointStore.Default, Guid.NewGuid(), filter, maxItems, rateLimit);
        }

        /// <summary>
        /// Creates an observable from an aggregate reader.
        /// </summary>
        /// <typeparam name="TEventFeed">The type of the <see cref="IAggregateReader"/>.</typeparam>
        /// <typeparam name="TCheckpointStore">The type of the <see cref="ICheckpointStore"/>.</typeparam>
        /// <param name="feed">The <see cref="IEventFeed"/>.</param>
        /// <param name="checkpointStore">The checkpoint store.</param>
        /// <param name="observerIdentity">The identity of the observer.</param>
        /// <param name="filter">The filter to use.</param>
        /// <param name="maxItems">The maximum number of items to return in a batch.</param>
        /// <param name="rateLimit">The (optional) rate limit.</param>
        /// <returns>The <see cref="IObservable{T}"/> of <see cref="SerializedEvent"/>s.</returns>
        /// <remarks>
        /// This version maintains a record of the last successfully processed checkpoint in the provided
        /// <paramref name="checkpointStore"/>.
        /// </remarks>
        public static EventFeedObservable AsObservable<TEventFeed, TCheckpointStore>(
        this TEventFeed feed,
        TCheckpointStore checkpointStore,
        Guid observerIdentity,
        EventFeedFilter filter,
        int maxItems = 1000,
        TimeSpan? rateLimit = null)
        where TEventFeed : IEventFeed
        where TCheckpointStore : ICheckpointStore
        {
            var subject = new Subject<Commit>();

            // We use a cancellation token source that we hand off to the AggregateObservable
            // so that we support either cancellation from the external source, or stopping and disposing.
            var cts = new CancellationTokenSource();

            Task observerTask = Task.Factory.StartNew<Task>(async () =>
            {
                try
                {
                    ReadOnlyMemory<byte>? checkpoint = await checkpointStore.ReadCheckpoint(observerIdentity);
                    while (!cts.IsCancellationRequested)
                    {
                        DateTimeOffset start = DateTimeOffset.Now;

                        EventFeedResult result;

                        if (checkpoint is null)
                        {
                            result = await feed.Get(filter, maxItems).ConfigureAwait(false);
                        }
                        else
                        {
                            result = await feed.Get(checkpoint.Value).ConfigureAwait(false);
                        }

                        foreach (Commit commit in result.Commits)
                        {
                            subject.OnNext(commit);

                            if (cts.IsCancellationRequested)
                            {
                                break;
                            }
                        }

                        await checkpointStore.SaveCheckpoint(observerIdentity, result.Checkpoint).ConfigureAwait(false);

                        checkpoint = result.Checkpoint;

                        TimeSpan elapsedTime = DateTimeOffset.Now - start;
                        if (!(rateLimit is null) && elapsedTime < rateLimit.Value)
                        {
                            await Task.Delay(rateLimit.Value - elapsedTime).ConfigureAwait(false);
                        }
                    }

                    subject.OnCompleted();
                }
                catch (Exception ex)
                {
                    subject.OnError(ex);
                }
            });

            return new EventFeedObservable(subject, observerTask, cts);
        }
    }
}
