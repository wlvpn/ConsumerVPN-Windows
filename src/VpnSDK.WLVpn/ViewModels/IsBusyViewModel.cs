// <copyright file="IsBusyViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using VpnSDK.WLVpn.Events;
using VpnSDK.WLVpn.Helpers;
using VpnSDK.WLVpn.Views;

namespace VpnSDK.WLVpn.ViewModels
{
    /// <summary>
    /// Class IsBusyViewModel. Represents the view model for <see cref="IsBusyView"/>.
    /// </summary>
    /// <seealso cref="BindableBase" />
    public class IsBusyViewModel : BindableBase
    {
        private string _busyText = Resources.Branding.Strings.AUTHENTICATING;
        private RelayCommand _cancelCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="IsBusyViewModel"/> class.
        /// </summary>
        /// <param name="sdkMonitor">The SDK monitor.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        public IsBusyViewModel(SDKMonitor sdkMonitor, EventAggregator eventAggregator)
        {
            Aggregator = eventAggregator;
            SDKMonitor = sdkMonitor;
            Aggregator.GetEvent<BusyTextEvent>().Subscribe((evt) =>
            {
                RunOnDisplayThread(() =>
                {
                    BusyText = evt.Text;
                });
            });
        }

        /// <summary>
        /// Gets the SDK monitor.
        /// </summary>
        /// <value>The SDK monitor.</value>
        public SDKMonitor SDKMonitor { get; private set; }

        /// <summary>
        /// Gets or sets the busy text.
        /// </summary>
        /// <value>The busy text.</value>
        public string BusyText
        {
            get { return _busyText; }
            set { SetProperty(ref _busyText, value); }
        }

        /// <summary>
        /// Gets the Cancel Connecting command.
        /// </summary>
        /// <value>The hide main view command.</value>
        public RelayCommand CancelConnectingCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(
                                         (parm) =>
                                         {
                                             SDKMonitor.CancelConnection();
                                         },
                                         (parm) => SDKMonitor.IsConnecting);
                }

                return _cancelCommand;
            }
        }

        private EventAggregator Aggregator { get; set; }
    }
}