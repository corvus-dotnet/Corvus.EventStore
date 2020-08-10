// <copyright file="TableStorageEventMerger.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Azure.TableStorage.Core.Internal
{
    using System;
    using System.Threading.Tasks;
    using Corvus.EventStore.Azure.TableStorage.ContainerFactories;

    /// <summary>
    /// Merges commits from one cloud table factory into an output cloud table factory, across all aggregates.
    /// </summary>
    /// <typeparam name="TInputCloudTableFactory">The type of the input <see cref="IEventCloudTableFactory"/>.</typeparam>
    /// <typeparam name="TOutputCloudTableFactory">The type of the output <see cref="IEventCloudTableFactory"/>.</typeparam>
    internal struct TableStorageEventMerger<TInputCloudTableFactory, TOutputCloudTableFactory>
        where TInputCloudTableFactory : IEventCloudTableFactory
        where TOutputCloudTableFactory : IEventCloudTableFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableStorageEventMerger{TInputCloudTableFactory, TOutputCloudTableFactory}"/> struct.
        /// </summary>
        /// <param name="inputFactory">The <see cref="InputFactory"/>.</param>
        /// <param name="outputFactory">The <see cref="OutputFactory"/>.</param>
        /// <param name="outputAggregateId">The <see cref="OutputAggregateId"/>.</param>
        public TableStorageEventMerger(TInputCloudTableFactory inputFactory, TOutputCloudTableFactory outputFactory, Guid outputAggregateId)
        {
            this.InputFactory = inputFactory;
            this.OutputFactory = outputFactory;
            this.OutputAggregateId = outputAggregateId;
        }

        /// <summary>
        /// Gets the input cloud table factory.
        /// </summary>
        public TInputCloudTableFactory InputFactory { get; }

        /// <summary>
        /// Gets the output cloud table factory.
        /// </summary>
        public TOutputCloudTableFactory OutputFactory { get; }

        /// <summary>
        /// Gets the aggregate Id to use to output the results.
        /// </summary>
        public Guid OutputAggregateId { get; }

        ////public Task StartMerge()
        ////{
        ////    var tables = this.InputFactory.GetTables();
        ////}
    }
}
