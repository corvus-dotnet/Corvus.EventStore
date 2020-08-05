// <copyright file="EventReaderResult.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus2.EventStore.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A result set from a call to <see cref="IEventReader.ReadAsync(string, long, long, int)"/>.
    /// </summary>
    public readonly struct EventReaderResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventReaderResult"/> struct.
        /// </summary>
        /// <param name="events">The events in the result.</param>
        /// <param name="utf8TextContinuationToken">The continuation token for the result as utf8 text.</param>
        public EventReaderResult(IEnumerable<SerializedEvent> events, ReadOnlyMemory<byte>? utf8TextContinuationToken)
        {
            this.Utf8TextContinuationToken = utf8TextContinuationToken;
            this.Events = events;
        }

        /// <summary>
        /// Gets the continuation token that can be used to retrieve the next block of events.
        /// </summary>
        public ReadOnlyMemory<byte>? Utf8TextContinuationToken { get; }

        /// <summary>
        /// Gets the list of returned events.
        /// </summary>
        public IEnumerable<SerializedEvent> Events { get; }
    }
}
