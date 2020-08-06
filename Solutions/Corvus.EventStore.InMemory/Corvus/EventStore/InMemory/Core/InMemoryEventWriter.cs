// <copyright file="InMemoryEventWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Core
{
    using System.Threading.Tasks;
    using Corvus.EventStore.Core;
    using Corvus.EventStore.InMemory.Core.Internal;

    /// <summary>
    /// In-memory implementation of <see cref="IEventWriter"/>.
    /// </summary>
    public readonly struct InMemoryEventWriter : IEventWriter
    {
        private readonly InMemoryEventStore store;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryEventWriter"/> struct.
        /// </summary>
        /// <param name="store">The underlying store.</param>
        public InMemoryEventWriter(InMemoryEventStore store)
        {
            this.store = store;
        }

        /// <inheritdoc/>
        public Task WriteCommitAsync(Commit commit)
        {
            // TODO: Catch store-specific exception and turn it into an IEventWriter concurrency exception/failure.
            return this.store.WriteCommitAsync(commit);
        }
    }
}
