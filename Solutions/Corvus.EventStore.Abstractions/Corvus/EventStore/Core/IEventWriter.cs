// <copyright file="IEventWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for classes that can write events for an aggregate to the store.
    /// </summary>
    public interface IEventWriter
    {
        /// <summary>
        /// Performs the supplied list of events to the store as a single transaction.
        /// </summary>
        /// <param name="commit">The batch of events to commit.</param>
        /// <returns>A task that completes when the events have been written to the store.</returns>
        Task WriteCommitAsync(Commit commit);
    }
}
