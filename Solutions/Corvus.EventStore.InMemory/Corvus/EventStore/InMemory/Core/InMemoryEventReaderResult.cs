// <copyright file="InMemoryEventReaderResult.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Core
{
    using System.Collections.Generic;
    using Corvus.EventStore.Core;

    /// <summary>
    /// Results from the <see cref="InMemoryEventReader"/>.
    /// </summary>
    public readonly struct InMemoryEventReaderResult : IEventReaderResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventReaderResult"/> struct.
        /// </summary>
        /// <param name="events">The results.</param>
        /// <param name="continuationToken">The continuation token, if there may be more results.</param>
        public InMemoryEventReaderResult(IEnumerable<IEvent> events, string? continuationToken)
        {
            this.Events = events;
            this.ContinuationToken = continuationToken;
        }

        /// <inheritdoc/>
        public string? ContinuationToken { get; }

        /// <inheritdoc/>
        public IEnumerable<IEvent> Events { get; }
    }
}
