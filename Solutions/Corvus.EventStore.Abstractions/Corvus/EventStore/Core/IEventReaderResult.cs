// <copyright file="IEventReaderResult.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// A result set from a call to <see cref="IEventReader.ReadAsync(string, long, long, int)"/>.
    /// </summary>
    public interface IEventReaderResult
    {
        /// <summary>
        /// Gets the continuation token that can be used to retrieve the next block of events.
        /// </summary>
        string? ContinuationToken { get; }

        /// <summary>
        /// Gets the list of returned events.
        /// </summary>
        public IEnumerable<IEvent> Events { get; }
    }
}