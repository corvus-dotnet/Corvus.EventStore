// <copyright file="EventFeedResult.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A set of results from an <see cref="IEventFeed"/>.
    /// </summary>
    public readonly struct EventFeedResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventFeedResult"/> struct.
        /// </summary>
        /// <param name="commits">The <see cref="Commits"/>.</param>
        /// <param name="checkpoint">The <see cref="Checkpoint"/>.</param>
        public EventFeedResult(in IEnumerable<Commit> commits, ReadOnlyMemory<byte> checkpoint)
        {
            this.Commits = commits;
            this.Checkpoint = checkpoint;
        }

        /// <summary>
        /// Gets the checkpoint for this result.
        /// </summary>
        /// <remarks>
        /// You pass this to <see cref="IEventFeed.Get(ReadOnlyMemory{byte})"/> to
        /// retrieve the next batch of results. Typically, you would stash this in
        /// a durable store somewhere when you have successfully processed the events
        /// in this result set.
        /// </remarks>
        public ReadOnlyMemory<byte> Checkpoint { get; }

        /// <summary>
        /// Gets the commits in this result set.
        /// </summary>
        /// <remarks>
        /// There may be no commits in this particular result set either because of filtering
        /// or because there are literally no more commits at the time of asking.
        /// </remarks>
        public IEnumerable<Commit> Commits { get; }
    }
}