// <copyright file="SettingsGeneralViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using VpnSDK.WLVpn.Helpers;
using VpnSDK.WLVpn.Views;

namespace VpnSDK.WLVpn.ViewModels
{
    /// <summary>
    /// SettingsGeneralViewModel. Provides the View Model for the <see cref="SettingsGeneralView"/>.
    /// </summary>
    public class SettingsGeneralViewModel : BindableBase, ISettingsViewModel
    {
        private bool _hideOnStartup = false;
        private bool _autoReconnect = false;
        private bool _connectOnStartup = false;
        private bool _killSwitch = false;
        private bool _startOnStartup = false;
        private bool _closeIsHide = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsGeneralViewModel"/> class.
        /// </summary>
        /// <param name="sdk">The instance of the <see cref="SDKMonitor"/> to use.</param>
        public SettingsGeneralViewModel(SDKMonitor sdk)
        {
            if (IsInDesignMode())
            {
                Init();
                return;
            }

            SdkMonitor = sdk;

            // get the right values for the fields below from the sdkMonitor class.
            HideApplicationOnStartup = SdkMonitor.HideApplicationOnStartup;
            StartOnStartup = SdkMonitor.StartOnStartup;
            AutoReconnect = SdkMonitor.AutoReconnect;
            ConnectOnStartup = SdkMonitor.ConnectOnStartup;
            KillSwitch = SdkMonitor.KillSwitch;
            CloseIsHide = SdkMonitor.CloseButtonHidesApplication;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application should be hidden to the notification area on start up.
        /// </summary>
        public bool HideApplicationOnStartup
        {
            get { return _hideOnStartup; }
            set
            {
                SetProperty(ref _hideOnStartup, value);
                SdkMonitor.HideApplicationOnStartup = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application should automatically attempt to reconnect the VPN connection if the connection is lost.
        /// </summary>
        public bool AutoReconnect
        {
            get { return _autoReconnect; }
            set
            {
                SetProperty(ref _autoReconnect, value);
                SdkMonitor.AutoReconnect = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application should be automatically reconnect on start up.
        /// </summary>
        public bool ConnectOnStartup
        {
            get { return _connectOnStartup; }
            set
            {
                SetProperty(ref _connectOnStartup, value);
                SdkMonitor.ConnectOnStartup = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application should disable all networking if the VPN is disconnected.
        /// </summary>
        public bool KillSwitch
        {
            get { return _killSwitch; }
            set
            {
                SetProperty(ref _killSwitch, value);
                SdkMonitor.KillSwitch = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application should be started when the user logs into the system.
        /// </summary>
        public bool StartOnStartup
        {
            get { return _startOnStartup; }
            set
            {
                SetProperty(ref _startOnStartup, value);
                SdkMonitor.StartOnStartup = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application should be hidden instead of closed when the user clicks the close button.
        /// </summary>
        public bool CloseIsHide
        {
            get { return _closeIsHide; }
            set
            {
                SetProperty(ref _closeIsHide, value);
                SdkMonitor.CloseButtonHidesApplication = value;
            }
        }

        private SDKMonitor SdkMonitor { get; }



        /// <summary>
        /// This method is used to initialize any data needed at design time to help the Visual Studio designer display data
        /// </summary>
        private void Init()
        {
        }
    }
}