// <copyright file="TableStorageEventWriter.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.InMemory.Core
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;
    using Corvus.EventStore.Core;

    /// <summary>
    /// In-memory implementation of <see cref="IEventWriter"/>.
    /// </summary>
    public readonly struct TableStorageEventWriter : IEventWriter
    {
        private readonly IEventCloudTableFactory cloudTableFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageEventWriter"/> struct.
        /// </summary>
        /// <param name="cloudTableFactory">The factory for the underlying cloud table.</param>
        public TableStorageEventWriter(IEventCloudTableFactory cloudTableFactory)
        {
            this.cloudTableFactory = cloudTableFactory;
        }

        /// <inheritdoc/>
        public Task WriteCommitAsync(Commit commit)
        {
            throw new NotImplementedException();
        }
    }
}
