// <copyright file="IPayloadReader.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore
{
    /// <summary>
    /// Used by <see cref="IEventHandler{TMemento}"/> implementations to
    /// read a payload from an event of a particular type.
    /// </summary>
    public interface IPayloadReader
    {
        /// <summary>
        /// Reads a payload from an event.
        /// </summary>
        /// <typeparam name="TPayload">The type of the payload to read.</typeparam>
        /// <returns>An instance of the payload, read from the event.</returns>
        TPayload Read<TPayload>();
    }
}
