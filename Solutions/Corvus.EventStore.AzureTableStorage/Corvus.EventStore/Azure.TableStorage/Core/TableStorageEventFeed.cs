// <copyright file="TableStorageEventFeed.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core
{
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;

    /// <summary>
    /// Support functions for the <see cref="TableStorageEventFeed{TFactory}"/>.
    /// </summary>
    public static class TableStorageEventFeed
    {
        /// <summary>
        /// Create a feed for the given <see cref="IAllStreamCloudTableFactory"/>.
        /// </summary>
        /// <typeparam name="TFactory">The type of <see cref="IAllStreamCloudTableFactory"/> for which to create a feed.</typeparam>
        /// <param name="factory">The <see cref="IAllStreamCloudTableFactory"/> for which to create the feed.</param>
        /// <returns>The feed for the all stream factory.</returns>
        public static TableStorageEventFeed<TFactory> GetFeedFor<TFactory>(TFactory factory)
            where TFactory : IAllStreamCloudTableFactory
        {
            return new TableStorageEventFeed<TFactory>(factory);
        }
    }
}
