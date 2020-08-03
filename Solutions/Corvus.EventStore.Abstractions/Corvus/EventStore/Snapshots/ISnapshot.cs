// <copyright file="ISnapshot.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Snapshots
{
    /// <summary>
    /// Represents a snapshot of an aggregate at a point in its history.
    /// </summary>
    /// <typeparam name="TMemento">The type of the memento produced by the source aggregate.</typeparam>
    public interface ISnapshot<TMemento>
    {
        /// <summary>
        /// Gets the Id of the aggregate from which this snapshot was generated.
        /// </summary>
        string AggregateId { get; }

        /// <summary>
        /// Gets the sequence number for the snapshot.
        /// </summary>
        long SequenceNumber { get; }

        /// <summary>
        /// Gets the memoized version of the aggregate for the snapshot.
        /// </summary>
        /// <returns>The payload.</returns>
        TMemento GetPayload();
    }
}