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
        private readonly DedicatedIpViewModel _dedicatedIp;

        private const string BestAvailable = "bestavailable";
        public IDialogManager Dialog { get; }

        public ConnectionStatus VpnConnectionStatus { get; set; } = ConnectionStatus.Disconnected;

        public IPAddress IPAddress { get; set; }

        public string VisibleLocation { get; set; }

        public string VisibleLocationFlag { get; set; } = null;

        public int SelectedIndex { get; set; }

        public MainViewModel(IEnumerable<IMainScreenTabItem> tabs, ISDK sdk, IDialogManager dialogManager, DedicatedIpViewModel dedicatedIp)
        {
            Items.AddRange(tabs);
            _sdk = sdk;
            _dedicatedIp = dedicatedIp;
            _sdk.VpnConnectionStatusChanged += OnVpnConnectionStatusChanged;
            _sdk.UserLocationStatusChanged += SdkOnUserLocationStatusChanged;
            _sdk.AuthenticationStatusChanged += OnAuthenticationStatusChanged;
            Dialog = dialogManager;
            _sdk.DnsMonitoringUpdate += OnDnsMonitorUpdate;
        }

        /// <summary>
        /// Selects the tab supplied by <typeparamref name="T"/>. Tabs are not at fixed
        /// positions, the Dedicated IP tab only exists for entitled accounts.
        /// </summary>
        public void SelectTab<T>()
            where T : IMainScreenTabItem
        {
            int index = Items.IndexOf(Items.OfType<T>().FirstOrDefault());
            if (index >= 0)
            {
                SelectedIndex = index;
            }
        }

        private async void OnAuthenticationStatusChanged(ISDK sender, AuthenticationStatus status)
        {
            if (status == AuthenticationStatus.Authenticated)
            {
                await UpdateDedicatedIpTab();
            }
            else if (status == AuthenticationStatus.NotAuthenticated)
            {
                Items.Remove(_dedicatedIp);
            }
        }

        /// <summary>
        /// Shows the Dedicated IP tab right after Home, but only for accounts that hold
        /// the entitlement for it.
        /// </summary>
        private async Task UpdateDedicatedIpTab()
        {
            bool entitled;

            try
            {
                var entitlements = await _sdk.GetUserEntitlementsAsync();
                entitled = entitlements.Contains(UserEntitlement.DedicatedIp);
            }
            catch (Exception e)
            {
                Log.Error(e, "Unable to read the user entitlements, hiding the Dedicated IP tab.");
                entitled = false;
            }

            if (!entitled)
            {
                Items.Remove(_dedicatedIp);
                return;
            }

            if (!Items.Contains(_dedicatedIp))
            {
                int home = Items.IndexOf(Items.OfType<HomeViewModel>().FirstOrDefault());
                Items.Insert(home >= 0 ? home + 1 : 0, _dedicatedIp);
            }
        }

        protected override async void OnInitialize()
        {
            base.OnInitialize();

            await UpdateDedicatedIpTab();

            if (Properties.Settings.Default.StartupType != StartupType.NOP)
            {
                if (Properties.Settings.Default.StartupType == StartupType.LastLocation)
                {
                    var lastLocation = _sdk.Locations.Where(i => i.Id != BestAvailable)
                        .FirstOrDefault(location => ((IChildren<IServer>)location).Children
                        .Any(child => child.ToString() == Properties.Settings.Default.LastSelectedServer));

                    await _sdk.InitiateConnection(lastLocation);
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
            SelectTab<SettingsContainerViewModel>();
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