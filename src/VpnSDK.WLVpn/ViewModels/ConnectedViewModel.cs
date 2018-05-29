// <copyright file="ConnectedViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using VpnSDK.WLVpn.Events;
using VpnSDK.WLVpn.Helpers;
using VpnSDK.WLVpn.Views;

namespace VpnSDK.WLVpn.ViewModels
{
    /// <summary>
    /// Class ConnectedViewModel. Represents the view model for <see cref="ConnectedView"/>.
    /// </summary>
    /// <seealso cref="BindableBase" />
    public class ConnectedViewModel : BindableBase
    {
        private readonly EventAggregator _eventAggregator;
        private RelayCommand _disconnectCmd;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedViewModel"/> class.
        /// </summary>
        /// <param name="sdkMonitor">The SDK monitor.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        public ConnectedViewModel(SDKMonitor sdkMonitor, EventAggregator eventAggregator)
        {
            if (IsInDesignMode())
            {
                Init();
                return;
            }

            SDKMonitor = sdkMonitor;
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        /// Gets the disconnect command.
        /// </summary>
        /// <value>The disconnect command.</value>
        public RelayCommand DisconnectCmd
        {
            get
            {
                if (_disconnectCmd == null)
                {
                    _disconnectCmd = new RelayCommand(
                        (parm) =>
                        {
                            Disconnect();
                        },
                        (parm) => SDKMonitor != null && SDKMonitor.CanDisconnect);
                }

                return _disconnectCmd;
            }
        }

        /// <summary>
        /// Gets or sets the SDK monitor.
        /// </summary>
        /// <value>The SDK monitor.</value>
        public SDKMonitor SDKMonitor { get; set; }

        private void Disconnect()
        {
            SDKMonitor.Disconnect();
        }

        private void Init()
        {
        }
    }
}