// <copyright file="InMemoryEventStoreEventOutOfSequenceException.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus2.EventStore.InMemory.Core.Internal
{
    using System;

    /// <summary>
    /// Exception thrown from the <see cref="InMemoryEventStore"/> when trying to add events with an invalid sequence
    /// number.
    /// </summary>
    [Serializable]
    public class InMemoryEventStoreEventOutOfSequenceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventStoreEventOutOfSequenceException"/> class.
        /// </summary>
        public InMemoryEventStoreEventOutOfSequenceException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventStoreEventOutOfSequenceException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public InMemoryEventStoreEventOutOfSequenceException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventStoreEventOutOfSequenceException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner exception.</param>
        public InMemoryEventStoreEventOutOfSequenceException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventStoreEventOutOfSequenceException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected InMemoryEventStoreEventOutOfSequenceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
