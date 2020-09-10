// <copyright file="CosmosEventFeed.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.Cosmos.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.Cosmos.ContainerFactories;
    using Corvus.EventStore.Azure.Cosmos.Core.Internal;
    using Corvus.EventStore.Core;
    using global::Azure.Cosmos;

    /// <summary>
    /// A cosmos Change feed.
    /// </summary>
    public readonly struct CosmosEventFeed : IEventFeed
    {
        private readonly IEventContainerFactory containerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosEventFeed"/> struct.
        /// </summary>
        /// <param name="containerFactory">The event container factory.</param>
        public CosmosEventFeed(IEventContainerFactory containerFactory)
        {
            this.containerFactory = containerFactory;
        }

        /// <inheritdoc/>
        public ValueTask<EventFeedResult> Get(EventFeedFilter filter, int maxItems)
        {
            CosmosContainer container = this.containerFactory.GetContainer();
            container.GetChangeFeedIterator<CommitDocument>();
        }

        /// <inheritdoc/>
        public ValueTask<EventFeedResult> Get(ReadOnlyMemory<byte> fromCheckpoint)
        {
            throw new NotImplementedException();
        }
    }
}
