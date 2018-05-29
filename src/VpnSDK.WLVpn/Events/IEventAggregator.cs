// <copyright file="IEventAggregator.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VpnSDK.WLVpn.Events
{
    /// <summary>
    /// Interface IEventAggregator. Defines an interface to get instances of an event type.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IEventAggregator : IDisposable
    {
        /// <summary>
        /// Gets the event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <returns>IObservable&lt;TEvent&gt;.</returns>
        IObservable<TEvent> GetEvent<TEvent>();

        /// <summary>
        /// Publishes the specified event.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="sampleEvent">The event.</param>
        void Publish<TEvent>(TEvent sampleEvent);
    }
}