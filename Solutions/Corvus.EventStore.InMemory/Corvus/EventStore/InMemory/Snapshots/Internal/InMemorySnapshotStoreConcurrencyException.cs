// <copyright file="InMemorySnapshotStoreConcurrencyException.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Snapshots.Internal
{
    using System;

    /// <summary>
    /// Exception thrown from the <see cref="InMemorySnapshotStore"/> when trying to add snapshots with an invalid sequence
    /// number.
    /// </summary>
    [Serializable]
    public class InMemorySnapshotStoreConcurrencyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMemorySnapshotStoreConcurrencyException"/> class.
        /// </summary>
        public InMemorySnapshotStoreConcurrencyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemorySnapshotStoreConcurrencyException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public InMemorySnapshotStoreConcurrencyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemorySnapshotStoreConcurrencyException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner exception.</param>
        public InMemorySnapshotStoreConcurrencyException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemorySnapshotStoreConcurrencyException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected InMemorySnapshotStoreConcurrencyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
