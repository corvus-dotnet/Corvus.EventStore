﻿// <copyright file="EventReaderResult.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
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
        public EventReaderResult(in IEnumerable<SerializedEvent> events, in ReadOnlyMemory<byte>? utf8TextContinuationToken)
        {
            this.ContinuationToken = utf8TextContinuationToken;
            this.Events = events;
        }

        /// <summary>
        /// Gets the continuation token that can be used to retrieve the next block of events.
        /// </summary>
        public ReadOnlyMemory<byte>? ContinuationToken { get; }

        /// <summary>
        /// Gets the list of returned events.
        /// </summary>
        public IEnumerable<SerializedEvent> Events { get; }
    }
}
