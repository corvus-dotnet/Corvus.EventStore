// <copyright file="IAggregateImplementationWithMementoHost.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Aggregates
{
    using Corvus.EventStore.Core;

    /// <summary>
    /// Implemented by types which wish to use the standard <see cref="AggregateImplementationWithMemento{TAggregate, TMemento}"/> pattern.
    /// </summary>
    /// <typeparam name="TAggregate">The type of the hosting aggregate.</typeparam>
    /// <typeparam name="TMemento">The type of the memento.</typeparam>
    public interface IAggregateImplementationWithMementoHost<TAggregate, TMemento> : IAggregateRoot<TAggregate>
        where TAggregate : IAggregateRoot<TAggregate>, IAggregateImplementationWithMementoHost<TAggregate, TMemento>
        where TMemento : new()
    {
        /// <summary>
        /// Applies the given event to a memento.
        /// </summary>
        /// <typeparam name="TPayload">The type of the event payload.</typeparam>
        /// <param name="memento">The memento to which to apply the event.</param>
        /// <param name="event">The event to apply.</param>
        /// <returns>An instance of the memento with the event applied.</returns>
        TMemento ApplyEventToMemento<TPayload>(in TMemento memento, in Event<TPayload> @event);

        /// <summary>
        /// Applies a serialized event to the aggregate implementation.
        /// </summary>
        /// <param name="implementation">The aggregate implemnetation to which to apply the serialized event.</param>
        /// <param name="event">The serialized event to apply.</param>
        /// <returns>An instance of the implementation type with the event applied.</returns>
        AggregateImplementationWithMemento<TAggregate, TMemento> ApplySerializedEvent(in AggregateImplementationWithMemento<TAggregate, TMemento> implementation, in SerializedEvent @event);
    }
}