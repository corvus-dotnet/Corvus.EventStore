// <copyright file="EventFeedObservable.cs" company="Endjin Limited">
// Copyright (c) Endjin Limited. All rights reserved.
// </copyright>

namespace Corvus.EventStore.Core
{
    using System;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An observable for an aggregate.
    /// </summary>
    public class EventFeedObservable : IObservable<Commit>, IAsyncDisposable
    {
        private readonly IObservable<Commit> internalObserver;
        private readonly Task observerTask;
        private readonly Subject<Commit> subject;
        private readonly CancellationTokenSource cts;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventFeedObservable"/> class.
        /// </summary>
        /// <param name="subject">The subject running the observable.</param>
        /// <param name="observerTask">The task running inside the observable.</param>
        /// <param name="cts">The <see cref="CancellationTokenSource"/> for the task running inside the observable.</param>
        internal EventFeedObservable(Subject<Commit> subject, Task observerTask, CancellationTokenSource cts)
        {
            this.internalObserver = subject.AsObservable();
            this.observerTask = observerTask;
            this.subject = subject;
            this.cts = cts;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            try
            {
                this.cts.Cancel();
                await this.observerTask;
            }
            finally
            {
                this.cts.Dispose();
                this.observerTask.Dispose();
                this.subject.Dispose();
            }
        }

        /// <inheritdoc/>
        public IDisposable Subscribe(IObserver<Commit> observer)
        {
            return this.internalObserver.Subscribe(observer);
        }
    }
}
