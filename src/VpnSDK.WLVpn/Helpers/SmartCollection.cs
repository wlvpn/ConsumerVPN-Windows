// <copyright file="SmartCollection.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace VpnSDK.WLVpn.Helpers
{
    /// <summary>
    /// Class SmartCollection.A generic observable collection derivative.
    /// </summary>
    /// <typeparam name="T">The type of object to be used for the collection.</typeparam>
    /// <seealso cref="System.Collections.ObjectModel.ObservableCollection{T}" />
    public class SmartCollection<T> : ObservableCollection<T>
    {
        private readonly object _collectionLock = new object();
        private bool _suspendCollectionChangeNotification;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartCollection{T}"/> class.
        /// </summary>
        public SmartCollection()
            : base()
        {
            _suspendCollectionChangeNotification = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartCollection{T}"/> class using a pre-existing <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="items">The <see cref="IEnumerable{T}"/> to derive the new collection from.</param>
        public SmartCollection(IEnumerable<T> items)
            : base()
        {
            AddRange(items);
        }

        /// <summary>
        /// Enables synchronization of the collection to ensure thread-safety if multiple threads are intending to use the collection.
        /// </summary>
        public void EnableSynchronization()
        {
            BindingOperations.EnableCollectionSynchronization(this, _collectionLock);
        }

        /// <summary>
        /// Suspends the collection change notification.
        /// </summary>
        public void SuspendCollectionChangeNotification()
        {
            _suspendCollectionChangeNotification = true;
        }

        /// <summary>
        /// Resumes the collection change notification.
        /// </summary>
        public void ResumeCollectionChangeNotification()
        {
            _suspendCollectionChangeNotification = false;
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="SmartCollection{T}"/>.
        /// </summary>
        /// <param name="items">The collection whose elements should be added to the end of the <see cref="SmartCollection{T}"/>. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
        public void AddRange(IEnumerable<T> items)
        {
            try
            {
                var inflatedItems = items?.ToList();  // incase this is a linq statement and the defered execution would result in a null.

                if (inflatedItems == null || inflatedItems.Count == 0)
                {
                    return;
                }

                SuspendCollectionChangeNotification();

                foreach (var i in inflatedItems)
                {
                    base.Add(i);
                }
            }
            finally
            {
                ResumeCollectionChangeNotification();
                var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                OnCollectionChanged(arg);
            }
        }

        /// <summary>
        /// This method clears and then adds the specified set of items to the collection.
        /// No collection changed events are raised until after the entire set is added.
        /// </summary>
        /// <param name="items">The enumerable set of items to add.</param>
        public void Repopulate(IEnumerable<T> items)
        {
            if (items != null)
            {
                try
                {
                    SuspendCollectionChangeNotification();
                    Clear();
                    AddRange(items);
                }
                finally
                {
                    ResumeCollectionChangeNotification();
                }
            }
        }

        /// <summary>
        /// Causes the collection changed notification to be raised.
        /// </summary>
        public void Reset()
        {
            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(arg);
        }

        /// <summary>
        /// Invokes the collection changed event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suspendCollectionChangeNotification)
            {
                {
                    var dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;

                    if (dispatcher != null && dispatcher.CheckAccess() == false)
                    {
                        dispatcher.BeginInvoke((Action)(() => OnCollectionChanged(e)));
                    }
                    else
                    {
                        base.OnCollectionChanged(e);
                    }
                }
            }
        }
    }
}