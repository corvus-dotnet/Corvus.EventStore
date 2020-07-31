// <copyright file="EventReaderResult.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// A result set from a call to <see cref="IEventReader.ReadAsync(string, long, long, long)"/>.
    /// </summary>
    public readonly struct EventReaderResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventReaderResult"/> struct.
        /// </summary>
        /// <param name="events">The <see cref="Events"/>.</param>
        /// <param name="continuationToken">The <see cref="ContinuationToken"/>.</param>
        public EventReaderResult(IEnumerable<IEvent> events, string? continuationToken)
        {
            this.Events = events;
            this.ContinuationToken = continuationToken;
        }

        /// <summary>
        /// Gets the list of returned events.
        /// </summary>
        public IEnumerable<IEvent> Events { get; }

        /// <summary>
        /// Gets the continuation token that can be used to retrieve the next block of events.
        /// </summary>
        public string? ContinuationToken { get; }
    }
}
