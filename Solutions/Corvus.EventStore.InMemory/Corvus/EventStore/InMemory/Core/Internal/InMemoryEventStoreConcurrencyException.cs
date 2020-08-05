// <copyright file="InMemoryEventStoreConcurrencyException.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Core.Internal
{
    using System;

    /// <summary>
    /// Exception thrown from the <see cref="InMemoryEventStore"/> when trying to add events with an invalid sequence
    /// number.
    /// </summary>
    [Serializable]
    public class InMemoryEventStoreConcurrencyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventStoreConcurrencyException"/> class.
        /// </summary>
        public InMemoryEventStoreConcurrencyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventStoreConcurrencyException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public InMemoryEventStoreConcurrencyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventStoreConcurrencyException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner exception.</param>
        public InMemoryEventStoreConcurrencyException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventStoreConcurrencyException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected InMemoryEventStoreConcurrencyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
