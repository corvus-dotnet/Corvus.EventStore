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
    using Corvus.Extensions;

    /// <summary>
    /// Underlying store used by <see cref="InMemoryEventReader"/> and <see cref="InMemoryEventWriter"/>.
    /// </summary>
    public class InMemoryEventStore
    {
        private readonly ConcurrentDictionary<string, InMemoryEventList> store =
            new ConcurrentDictionary<string, InMemoryEventList>();

        /// <summary>
        /// Reads events from the store for an aggregate.
        /// </summary>
        /// <param name="aggregateId">The Id of the aggregate to retrieve events for.</param>
        /// <param name="fromSequenceNumber">The minimum <see cref="IEvent.SequenceNumber"/> to retrieve.</param>
        /// <param name="toSequenceNumber">The maximum <see cref="IEvent.SequenceNumber"/> to retreive.</param>
        /// <param name="maxItems">The maximum number of items to return.</param>
        /// <returns>The results, contained in an <see cref="IEventReaderResult"/>.</returns>
        public ValueTask<IEventReaderResult> ReadAsync(string aggregateId, long fromSequenceNumber, long toSequenceNumber, int maxItems)
        {
            if (!this.store.TryGetValue(aggregateId, out InMemoryEventList list))
            {
                return new ValueTask<IEventReaderResult>(new InMemoryEventReaderResult(Enumerable.Empty<InMemoryEvent>(), null));
            }

            InMemoryEvent[] events = list.Events
                .Where(item => item.Key >= fromSequenceNumber && item.Key <= toSequenceNumber)
                .Take(maxItems)
                .Select(item => item.Value)
                .ToArray();

            string? encodedContinuationToken = null;

            if (events.Length == maxItems)
            {
                var continuationToken = new ContinuationToken(aggregateId, fromSequenceNumber + maxItems, toSequenceNumber, maxItems);

                encodedContinuationToken = JsonSerializer.Serialize(continuationToken).Base64UrlEncode();
            }

            return new ValueTask<IEventReaderResult>(new InMemoryEventReaderResult(events, encodedContinuationToken));
        }

        /// <summary>
        /// Reads the next block in a result set initially acquired by calling <see cref="ReadAsync(string, long, long, int)"/>.
        /// </summary>
        /// <param name="encodedContinuationToken">A continuation token returned from a previous call that can be used to
        /// obtain the next set of results.</param>
        /// <returns>The results, contained in an <see cref="IEventReaderResult"/>.</returns>
        public ValueTask<IEventReaderResult> ReadAsync(string encodedContinuationToken)
        {
            ContinuationToken continuationToken = JsonSerializer.Deserialize<ContinuationToken>(encodedContinuationToken.Base64UrlDecode());

            return this.ReadAsync(continuationToken.AggregateId, continuationToken.FromSequenceNumber, continuationToken.ToSequenceNumber, continuationToken.MaxItems);
        }

        /// <summary>
        /// Writes the supplied events to the store as a single transaction.
        /// </summary>
        /// <param name="eventWrites">The write instructions.</param>
        /// <returns>A task that completes when the events have been written to the store.</returns>
        public ValueTask WriteBatchAsync(IEnumerable<Action<IEventBatchWriter>> eventWrites)
        {
            InMemoryStoreEventBatchWriter batch = default;

            foreach (Action<IEventBatchWriter> writer in eventWrites)
            {
                writer(batch);
            }

            ImmutableArray<InMemoryEvent> events = batch.Events;

            if (events.Length == 0)
            {
                return new ValueTask(Task.CompletedTask);
            }

            string aggregateId = events[0].AggregateId;

            events.ForEachAtIndex((ev, idx) =>
            {
                if (ev.AggregateId != aggregateId)
                {
                    throw new ArgumentException("All supplied events must have the same aggregate Id.", nameof(events));
                }

                if (idx > 0 && ev.SequenceNumber != events[idx - 1].SequenceNumber + 1)
                {
                    throw new ArgumentException("Event sequence numbers must be consecutive.", nameof(events));
                }
            });

            this.store.AddOrUpdate(
                aggregateId,
                seq =>
                {
                    if (events[0].SequenceNumber != 0)
                    {
                        throw new InMemoryEventStoreEventOutOfSequenceException();
                    }

                    return new InMemoryEventList(0, ImmutableDictionary<long, InMemoryEvent>.Empty.AddRange(events.Select(ev => KeyValuePair.Create(ev.SequenceNumber, ev))));
                },
                (aggregateId, list) =>
                {
                    if (list.LastSequenceNumber >= events[0].SequenceNumber)
                    {
                        throw new InMemoryEventStoreConcurrencyException();
                    }

                    if (list.LastSequenceNumber != events[0].SequenceNumber - 1)
                    {
                        throw new InMemoryEventStoreEventOutOfSequenceException();
                    }

                    return list.AddEvents(events);
                });

            return new ValueTask(Task.CompletedTask);
        }

        /// <summary>
        /// Writes the supplied event to the store as a single transaction.
        /// </summary>
        /// <param name="event">The event to write.</param>
        /// <returns>A task that completes when the events have been written to the store.</returns>
        public ValueTask WriteAsync(InMemoryEvent @event)
        {
            this.store.AddOrUpdate(
                @event.AggregateId,
                seq =>
                {
                    if (@event.SequenceNumber != 0)
                    {
                        throw new InMemoryEventStoreEventOutOfSequenceException();
                    }

                    return new InMemoryEventList(0, ImmutableDictionary<long, InMemoryEvent>.Empty.Add(0, @event));
                },
                (aggregateId, list) =>
                {
                    if (list.LastSequenceNumber >= @event.SequenceNumber)
                    {
                        throw new InMemoryEventStoreConcurrencyException();
                    }

                    if (list.LastSequenceNumber != @event.SequenceNumber - 1)
                    {
                        throw new InMemoryEventStoreEventOutOfSequenceException();
                    }

                    return list.AddEvents(ImmutableArray<InMemoryEvent>.Empty.Add(@event));
                });

            return new ValueTask(Task.CompletedTask);
        }

        private readonly struct ContinuationToken
        {
            public ContinuationToken(string aggregateId, long fromSequenceNumber, long toSequenceNumber, int maxItems)
            {
                this.FromSequenceNumber = fromSequenceNumber;
                this.ToSequenceNumber = toSequenceNumber;
                this.MaxItems = maxItems;
                this.AggregateId = aggregateId;
            }

            public long FromSequenceNumber { get; }

            public long ToSequenceNumber { get; }

            public int MaxItems { get; }

            public string AggregateId { get; }
        }

        private readonly struct InMemoryEventList
        {
            public InMemoryEventList(long eTag, ImmutableDictionary<long, InMemoryEvent> events)
            {
                this.Events = events;
                this.LastSequenceNumber = eTag;
            }

            public long LastSequenceNumber { get; }

            public ImmutableDictionary<long, InMemoryEvent> Events { get; }

            public InMemoryEventList AddEvents(ImmutableArray<InMemoryEvent> events)
            {
                return new InMemoryEventList(
                    this.LastSequenceNumber + 1,
                    this.Events.AddRange(events.Select(ev => KeyValuePair.Create(ev.SequenceNumber, ev))));
            }
        }

        private struct InMemoryStoreEventBatchWriter : IEventBatchWriter
        {
            private List<InMemoryEvent>? events;

            public ImmutableArray<InMemoryEvent> Events => ImmutableArray<InMemoryEvent>.Empty.AddRange(this.events);

            public void WriteBatchItemAsync<TEvent, TPayload>(in TEvent @event)
                where TEvent : IEvent
            {
                if (this.events is null)
                {
                    this.events = new List<InMemoryEvent>();
                }

                this.events.Add(InMemoryEvent.CreateFrom<TEvent, TPayload>(@event));
            }
        }
    }
}
