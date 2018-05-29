// <copyright file="EventAggregator.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace VpnSDK.WLVpn.Events
{
    // based on http://joseoncode.com/2010/04/29/event-aggregator-with-reactive-extensions/
    // and http://machadogj.com/2011/3/yet-another-event-aggregator-using-rx.html

    /// <summary>
    /// Class EventAggregator. Implements an in-process event aggregator for objects to publish events globally.
    /// </summary>
    /// <seealso cref="IEventAggregator" />
    public class EventAggregator : IEventAggregator
    {
        private readonly Subject<object> _subject = new Subject<object>();
        private bool _disposed;

        /// <inheritdoc/>
        public IObservable<TEvent> GetEvent<TEvent>()
        {
            return _subject.OfType<TEvent>().AsObservable();
        }

        /// <inheritdoc/>
        public void Publish<TEvent>(TEvent sampleEvent)
        {
            _subject.OnNext(sampleEvent);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _subject.Dispose();

            _disposed = true;
        }
    }
}