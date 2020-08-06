// <copyright file="Aggregate.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Aggregates
{
    using System.Threading.Tasks;
    using Corvus.EventStore.Core;

    /// <summary>
    /// An instance of an aggregate.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the aggregate that this wraps.</typeparam>
    public readonly struct Aggregate<TAggregate>
        where TAggregate : IAggregateRoot<TAggregate>
    {
        private readonly TAggregate aggregate;

        /// <summary>
        /// Initializes a new instance of the <see cref="Aggregate{TAggregate}"/> struct.
        /// </summary>
        /// <param name="aggregate">The aggregate.</param>
        public Aggregate(TAggregate aggregate)
        {
            this.aggregate = aggregate;
        }

        /// <summary>
        /// Applies the given event to the aggregate.
        /// </summary>
        /// <typeparam name="TPayload">The payload of the event to apply.</typeparam>
        /// <param name="event">The event to apply.</param>
        /// <remarks>
        /// This will be called when a new event has been created.
        /// </remarks>
        /// <returns>The aggregate with the event applied.</returns>
        public Aggregate<TAggregate> ApplyEvent<TPayload>(in Event<TPayload> @event)
        {
            return new Aggregate<TAggregate>(this.aggregate.ApplyEvent(@event));
        }

        /// <summary>
        /// Stores uncommitted events using the specified event writer.
        /// </summary>
        /// <typeparam name="TEventWriter">The type of event writer to use.</typeparam>
        /// <param name="writer">The event writer to use to store new events.</param>
        /// <returns>The aggregate with all new events committed.</returns>
        public async ValueTask<Aggregate<TAggregate>> CommitAsync<TEventWriter>(TEventWriter writer)
            where TEventWriter : IEventWriter
        {
            return new Aggregate<TAggregate>(await this.aggregate.CommitAsync(writer).ConfigureAwait(false));
        }
    }
}
