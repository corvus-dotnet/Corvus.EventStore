// <copyright file="IAggregateImplementationWithMemento{TImplementation,TMemento}.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Aggregates
{
    using Corvus.EventStore.Core;

    /// <summary>
    /// Implemented by types which wish to use the standard <see cref="AggregateWithMemento{TAggregate, TMemento}"/> pattern.
    /// </summary>
    /// <typeparam name="TImplementation">The type implementating this interface.</typeparam>
    /// <typeparam name="TMemento">The type of the memento.</typeparam>
    public interface IAggregateImplementationWithMemento<TImplementation, TMemento>
        where TImplementation : IAggregateImplementationWithMemento<TImplementation, TMemento>, new()
        where TMemento : new()
    {
        /// <summary>
        /// Applies the given event to a memento.
        /// </summary>
        /// <typeparam name="TPayload">The type of the event payload.</typeparam>
        /// <param name="memento">The memento to which to apply the event.</param>
        /// <param name="event">The event to apply.</param>
        /// <returns>An instance of the memento with the event applied.</returns>
        TMemento ApplyEvent<TPayload>(in TMemento memento, in Event<TPayload> @event);

        /// <summary>
        /// Applies a serialized event to the aggregate implementation.
        /// </summary>
        /// <param name="memento">The memento which to apply the serialized event.</param>
        /// <param name="event">The serialized event to apply.</param>
        /// <returns>An instance of the memento with the event applied.</returns>
        TMemento ApplySerializedEvent(in TMemento memento, in SerializedEvent @event);
    }
}