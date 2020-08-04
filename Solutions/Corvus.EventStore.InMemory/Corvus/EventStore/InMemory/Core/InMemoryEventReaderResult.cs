// <copyright file="InMemoryEventReaderResult.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using Corvus.EventStore.Core;

    /// <summary>
    /// Results from the <see cref="InMemoryEventReader"/>.
    /// </summary>
    public readonly struct InMemoryEventReaderResult : IEventReaderResult
    {
        private readonly ImmutableArray<InMemoryEvent> events;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventReaderResult"/> struct.
        /// </summary>
        /// <param name="events">The results.</param>
        /// <param name="continuationToken">The continuation token, if there may be more results.</param>
        internal InMemoryEventReaderResult(IEnumerable<InMemoryEvent> events, string? continuationToken)
        {
            this.events = ImmutableArray<InMemoryEvent>.Empty.AddRange(events);
            this.ContinuationToken = continuationToken;
        }

        /// <inheritdoc/>
        public string? ContinuationToken { get; }

        /// <inheritdoc/>
        public IEventEnumerator Events => new InMemoryEventEnumerator(this);

        /// <summary>
        /// Represents an enumerator for the properties of the resource.
        /// </summary>
        [DebuggerDisplay("{Current,nq}")]
        public struct InMemoryEventEnumerator : IEventEnumerator
        {
            private readonly InMemoryEventReaderResult target;
            private ImmutableArray<InMemoryEvent>.Enumerator enumerator;

            /// <summary>
            /// Initializes a new instance of the <see cref="InMemoryEventEnumerator"/> struct.
            /// </summary>
            /// <param name="target">The target <see cref="InMemoryEventReaderResult"/>.</param>
            internal InMemoryEventEnumerator(InMemoryEventReaderResult target)
            {
                this.target = target;
                this.enumerator = this.target.events.GetEnumerator();
            }

            /// <inheritdoc />
            public IEvent Current
            {
                get
                {
                    return this.enumerator.Current;
                }
            }

            /// <inheritdoc/>
            object IEnumerator.Current => this.Current;

            /// <inheritdoc/>
            public string CurrentEventType => this.enumerator.Current.EventType;

            /// <inheritdoc/>
            public string CurrentAggregateId => this.enumerator.Current.AggregateId;

            /// <inheritdoc/>
            public string CurrentPartitionKey => this.enumerator.Current.PartitionKey;

            /// <inheritdoc/>
            public long CurrentTimestamp => this.enumerator.Current.Timestamp;

            /// <inheritdoc/>
            public long CurrentSequenceNumber => this.enumerator.Current.SequenceNumber;

            /// <summary>
            /// Returns an enumerator that iterates the links on the document for a particular relation.
            /// </summary>
            /// <returns>An enumerator that can be used to iterate through the links on the document for the relation.</returns>
            public InMemoryEventEnumerator GetEnumerator()
            {
                InMemoryEventEnumerator result = this;
                result.Reset();
                return result;
            }

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            /// <inheritdoc/>
            IEnumerator<IEvent> IEnumerable<IEvent>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            /// <inheritdoc/>
            public void Reset()
            {
                this.enumerator = this.target.events.GetEnumerator();
            }

            /// <inheritdoc/>
            public bool MoveNext()
            {
                return this.enumerator.MoveNext();
            }

            /// <inheritdoc/>
            public T GetCurrentPayload<T>()
            {
                return this.enumerator.Current.GetPayload<T>();
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                // NOP
            }
        }
    }
}
