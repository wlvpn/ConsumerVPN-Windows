using Caliburn.Micro;
using DynamicData.Binding;
using WLVPN.Enums;
using WLVPN.Extensions;
using WLVPN.Interfaces;
using WLVPN.Properties;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VpnSDK;
using VpnSDK.DTO;
using VpnSDK.Enums;
using VpnSDK.Extensions;
using VpnSDK.Interfaces;
using WPFLocalizeExtension.Providers;
using WLVPN.Helpers;

namespace WLVPN.ViewModels
{
    internal class MainViewModel : Conductor<IMainScreenTabItem>.Collection.OneActive
    {
        private readonly ISDK _sdk;

        public IDialogManager Dialog { get; }

        public ConnectionStatus VpnConnectionStatus { get; set; } = ConnectionStatus.Disconnected;

        public IPAddress IPAddress { get; set; }

        public string VisibleLocation { get; set; }

        public string VisibleLocationFlag { get; set; } = null;

        public int SelectedIndex { get; set; }

        public MainViewModel(IEnumerable<IMainScreenTabItem> tabs, ISDK sdk, IDialogManager dialogManager)
        {
            Items.AddRange(tabs);
            _sdk = sdk;
            _sdk.VpnConnectionStatusChanged += OnVpnConnectionStatusChanged;
            _sdk.UserLocationStatusChanged += SdkOnUserLocationStatusChanged;
            Dialog = dialogManager;
        }

        protected override async void OnInitialize()
        {
            base.OnInitialize();
            if (Properties.Settings.Default.StartupType != StartupType.NOP)
            {
                if (Properties.Settings.Default.StartupType == StartupType.LastLocation)
                {
                    await _sdk.InitiateConnection();
                }
                else
                {
                    await _sdk.InitiateConnection(_sdk.Locations.First());
                }
            }
            await _sdk.Locations.Ping();
        }

        private void SdkOnUserLocationStatusChanged(ISDK sender, OperationStatus status, NetworkGeolocation args)
        {
            IPAddress = args?.IPAddress;

            if (args?.City != null && args?.Country != null)
            {
                VisibleLocation = $"{args.City}, {args.Country}";
                VisibleLocationFlag = args.CountryCode;
            }
            else if (args?.City == null && args?.Country != null)
            {
                VisibleLocation = args.Country;
                VisibleLocationFlag = args.CountryCode;
            }
            else
            {
                VisibleLocation = Properties.Strings.Updating + Properties.Strings.ProgressSuffix;
                VisibleLocationFlag = null;
                IPAddress = null;
            }
        }

        private void OnVpnConnectionStatusChanged(ISDK sender, ConnectionStatus previous, ConnectionStatus current)
        {
            VpnConnectionStatus = current;
        }

        public void OpenSettingsTab()
        {
            SelectedIndex = (int)MainScreenTabs.Settings;
        }

        public async Task InstallOrRepairDrivers()
        {
            await _sdk.TapInstallOrRepair();
        }

        /// <summary>
        /// Invalidates the logged in user object and publishes the logout 
        /// notification on the UI thread. 
        /// </summary>
        public async Task Logout()
        {
            Settings.Default.Username = null;
            Settings.Default.Password = null;
            Settings.Default.Save();

            await _sdk.Logout();
            _sdk.AllowOnlyVPNConnectivity = false;
            _sdk.AllowLANTraffic = true;
        }

        public void Website()
        {
            ProcessExtensions.LaunchUrl(Resource.Get<Uri>("WebsiteUrl"));
        }

        public void GoToHelp()
        {
            ActiveItem = AppBootstrapper.ContainerInstance.GetInstance<InformationContainerViewModel>();
        }

        public void GoToSettings()
        {
            ActiveItem = AppBootstrapper.ContainerInstance.GetInstance<SettingsContainerViewModel>();
        }

    }
}