// <copyright file="EventReaderResult.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A result set from a call to <see cref="IEventReader.ReadCommitsAsync(Guid, string, long, long, int)"/>.
    /// </summary>
    public readonly struct EventReaderResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventReaderResult"/> struct.
        /// </summary>
        /// <param name="commits">The commits in the result.</param>
        /// <param name="continuationToken">The continuation token for the result.</param>
        public EventReaderResult(in IEnumerable<Commit> commits, in ReadOnlyMemory<byte>? continuationToken)
        {
            this.ContinuationToken = continuationToken;
            this.Commits = commits;
        }

        /// <summary>
        /// Gets the continuation token that can be used to retrieve the next block of events.
        /// </summary>
        public ReadOnlyMemory<byte>? ContinuationToken { get; }

        /// <summary>
        /// Gets the list of returned events.
        /// </summary>
        public IEnumerable<Commit> Commits { get; }
    }
}
