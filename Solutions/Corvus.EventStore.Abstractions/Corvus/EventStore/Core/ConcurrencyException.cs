// <copyright file="ConcurrencyException.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;

    /// <summary>
    /// Exception thrown when a concurrency error occurs during a commit.
    /// </summary>
    [Serializable]
    public class ConcurrencyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        public ConcurrencyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public ConcurrencyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner exception.</param>
        public ConcurrencyException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected ConcurrencyException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
