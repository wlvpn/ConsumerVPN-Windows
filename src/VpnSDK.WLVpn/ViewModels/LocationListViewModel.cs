// <copyright file="LocationListViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System.Windows.Data;
using VpnSDK.Public.Interfaces;
using VpnSDK.WLVpn.Events;
using VpnSDK.WLVpn.Helpers;
using VpnSDK.WLVpn.Views;

namespace VpnSDK.WLVpn.ViewModels
{
    /// <summary>
    /// Class LocationListViewModel. Represents the view model for <see cref="LocationListView"/>.
    /// </summary>
    /// <seealso cref="BindableBase" />
    public class LocationListViewModel : BindableBase
    {
        private EventAggregator _eventAggregator = null;
        private ILocation _selServer = null;
        private RelayCommand _saveCmd = null;
        private RelayCommand _cancelCmd = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationListViewModel"/> class.
        /// </summary>
        /// <param name="sdkMonitor">The SDK monitor.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        public LocationListViewModel(SDKMonitor sdkMonitor, EventAggregator eventAggregator)
        {
            if (IsInDesignMode())
            {
                Init();
                return;
            }

            SDKMonitor = sdkMonitor;
            _eventAggregator = eventAggregator;
            SelectedLocation = SDKMonitor.SelectedLocation;

            Locations = CollectionViewSource.GetDefaultView(SDKMonitor.Locations) as ListCollectionView;
        }

        /// <summary>
        /// Gets the SDK monitor.
        /// </summary>
        /// <value>The SDK monitor.</value>
        public SDKMonitor SDKMonitor { get; private set; }

        /// <summary>
        /// Gets or sets the locations.
        /// </summary>
        /// <value>The locations.</value>
        public ListCollectionView Locations { get; protected set; }

        /// <summary>
        /// Gets or sets the selected location.
        /// </summary>
        /// <value>The selected location.</value>
        public ILocation SelectedLocation
        {
            get
            {
                return _selServer;
            }

            set
            {
                SetProperty(ref _selServer, value);
            }
        }

        /// <summary>
        /// Gets the save command.
        /// </summary>
        /// <value>The save command.</value>
        public RelayCommand SaveCmd
        {
            get
            {
                if (_saveCmd == null)
                {
                    _saveCmd = new RelayCommand(
                        (parm) =>
                        {
                            Save();
                        },
                        (parm) => true);
                }

                return _saveCmd;
            }
        }

        /// <summary>
        /// Gets the cancel command.
        /// </summary>
        /// <value>The cancel command.</value>
        public RelayCommand CancelCmd
        {
            get
            {
                if (_cancelCmd == null)
                {
                    _cancelCmd = new RelayCommand(
                        (parm) =>
                        {
                            Cancel();
                        },
                        (parm) => true);
                }

                return _cancelCmd;
            }
        }

        private void Save()
        {
            SDKMonitor.SelectedLocation = SelectedLocation;
            Return();
        }

        private void Cancel()
        {
            SelectedLocation = null;
            Return();
        }

        private void Return()
        {
            _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.Back });
        }

        private void Init()
        {
        }
    }
}