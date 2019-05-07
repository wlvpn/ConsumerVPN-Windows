using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WLVPN.Utils
{
    public class SmartCollection<T> : ObservableCollection<T>
    {
        private bool _suspendCollectionChangeNotification;
        private object _collectionLock = new object();

        #region Constructor
        public SmartCollection()
            : base()
        {
            _suspendCollectionChangeNotification = false;

        }

        public SmartCollection(IEnumerable<T> items)
            : base()
        {
            AddRange(items);
        }
        #endregion

        #region Enable Sync
        public void EnableSynchronization()
        {
            BindingOperations.EnableCollectionSynchronization(this, _collectionLock);
        }
        #endregion

        #region On Collection Changed
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
        #endregion

        #region Suspend Collection Change Notification
        public void SuspendCollectionChangeNotification()
        {
            _suspendCollectionChangeNotification = true;
        }
        #endregion

        #region Resume Collection Change Notification
        public void ResumeCollectionChangeNotification()
        {
            _suspendCollectionChangeNotification = false;
        }
        #endregion

        #region Add range
        public void AddRange(IEnumerable<T> items)
        {
            try
            {
                var inflatedItems = items?.ToList();  // incase this is a linq statement and the defered execution would result in a null.

                if (inflatedItems == null || inflatedItems.Count == 0) return;

                this.SuspendCollectionChangeNotification();

                foreach (var i in inflatedItems)
                {
                    base.Add(i);
                }
            }
            finally
            {
                this.ResumeCollectionChangeNotification();
                var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                this.OnCollectionChanged(arg);
            }
        }
        #endregion

        #region Repopulate
        public void Repopulate(IEnumerable<T> items)
        {

            if (items != null)
            {
                try
                {
                    this.SuspendCollectionChangeNotification();
                    this.Clear();
                    this.AddRange(items);
                }
                finally
                {
                    this.ResumeCollectionChangeNotification();
                }
            }
        }

        #endregion

        #region Reset
        public void Reset()
        {
            var arg = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            this.OnCollectionChanged(arg);
        }
        #endregion
    }
}
