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
        public async Task WriteCommitAsync(Commit commit)
        {
            try
            {
                await this.store.WriteCommitAsync(commit).ConfigureAwait(false);
            }
            catch (InMemoryEventStoreConcurrencyException ex)
            {
                throw new ConcurrencyException($"Unable to write the commit for aggregateID {commit.AggregateId} with sequence number {commit.SequenceNumber}.", ex);
            }
        }
    }
}
