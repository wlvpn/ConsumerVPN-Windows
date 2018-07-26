// <copyright file="DisconnectedViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using VpnSDK.WLVpn.Events;
using VpnSDK.WLVpn.Helpers;
using VpnSDK.WLVpn.Views;

namespace VpnSDK.WLVpn.ViewModels
{
    /// <summary>
    /// Class DisconnectedViewModel. Represents the view model associated with <see cref="DisconnectedView"/>.
    /// </summary>
    /// <seealso cref="BindableBase" />
    public class DisconnectedViewModel : BindableBase
    {
        private EventAggregator _eventAggregator = null;
        private RelayCommand _connectCmd = null;
        private RelayCommand _serverListCmd = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectedViewModel"/> class.
        /// </summary>
        /// <param name="sdkMonitor">The SDK monitor.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        public DisconnectedViewModel(SDKMonitor sdkMonitor, EventAggregator eventAggregator)
        {
            if (IsInDesignMode())
            {
                return;
            }

            SDKMonitor = sdkMonitor;
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        /// Gets the SDK monitor.
        /// </summary>
        /// <value>The SDK monitor.</value>
        public SDKMonitor SDKMonitor { get; private set; }

        /// <summary>
        /// Gets the connect command.
        /// </summary>
        /// <value>The connect command.</value>
        public RelayCommand ConnectCmd
        {
            get
            {
                if (_connectCmd == null)
                {
                    _connectCmd = new RelayCommand(
                        (parm) =>
                        {
                            Connect();
                        },
                        (parm) => SDKMonitor != null && SDKMonitor.CanConnect);
                }

                return _connectCmd;
            }
        }

        /// <summary>
        /// Gets the server list command.
        /// </summary>
        /// <value>The server list command.</value>
        public RelayCommand ServerListCmd
        {
            get
            {
                if (_serverListCmd == null)
                {
                    _serverListCmd = new RelayCommand(
                        (parm) =>
                        {
                            _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.ServerList });
                        },
                        (parm) => true);
                }

                return _serverListCmd;
            }
        }

        private void Connect()
        {
            SDKMonitor.Connect();
        }

        public void Init()
        {
            // If connect on startup, now is the time.
            if (SDKMonitor.ConnectOnStartup)
            {
                Connect();
            }
        }
    }
}