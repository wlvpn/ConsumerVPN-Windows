using Caliburn.Micro;
using WLVPN.Enums;
using WLVPN.Extensions;
using WLVPN.Interfaces;
using WLVPN.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;
using VpnSDK.DTO;
using VpnSDK.Enums;
using VpnSDK.Extensions;
using VpnSDK.Interfaces;
using WLVPN.Helpers;
using VpnSDK.DnsMonitor.DTO;
using Serilog;

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
            _sdk.DnsMonitoringUpdate += OnDnsMonitorUpdate;
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

        private void OnDnsMonitorUpdate(DnsMonitoringArgs dnsMonitoringArgs)
        {
            if (dnsMonitoringArgs == null)
            {
                return;
            }

            Log.Information($"Received DnsMonitoringEvent url count: {dnsMonitoringArgs.DomainNames.Count} {string.Join(",", dnsMonitoringArgs.DomainNames)}");
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
    }
}