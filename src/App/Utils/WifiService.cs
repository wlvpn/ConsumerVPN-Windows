using ManagedNativeWifi;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using WLVPN.Interfaces;
using WLVPN.Model;

namespace WLVPN.Utils
{
    /// <summary>
    /// Wifi network related class.
    /// </summary>
    public class WifiService : IWifiService, IDisposable
    {
        private NativeWifiPlayer _nativeWifiPlayer;
        private const int _wifiScanTimeout = 5;
        private string _currentSsid;
        public event EventHandler NetworkChanged;

        /// <summary>
        ///  Gets the collection for Wi-Fi networks.
        /// </summary>
        public IReadOnlyList<WifiNetwork> Networks => _wifiNetworks;

        private SmartCollection<WifiNetwork> _wifiNetworks;
        /// <summary>
        /// Wifiservice constructor.
        /// </summary>
        public WifiService()
        {
            _nativeWifiPlayer = new NativeWifiPlayer();            
            NetworkChange.NetworkAddressChanged += OnNetworkChanged;
            _wifiNetworks = new SmartCollection<WifiNetwork>();
        }

        /// <summary>
        /// Refresh wifi network in the current machine.
        /// </summary>
        /// <returns>retuns list of WifiNetwork from the system.</returns>
        public async Task Refresh(CancellationToken cancellationTokenSource)
        {
            try
            {
                await Task.Run(() => _nativeWifiPlayer.ScanNetworksAsync(TimeSpan.FromSeconds(_wifiScanTimeout), cancellationTokenSource)).ConfigureAwait(false);

                // Get connected and available networks
                var connectedWiFiNetworks = await Task.Run(() => NativeWifi.EnumerateInterfaceConnections()).ConfigureAwait(false);
                var availableWiFiNetworks = await Task.Run(() => NativeWifi.EnumerateAvailableNetworks()).ConfigureAwait(false);

                var extendedWiFiInformation =
                    from network in availableWiFiNetworks
                    select new WifiNetwork()
                    {
                        Connected = connectedWiFiNetworks.Any(n => n.State == InterfaceState.Connected &&
                                                                   n.Id.Equals(network.Interface.Id) &&
                                                                   (n.ProfileName?.Equals(network.ProfileName, StringComparison.Ordinal) ?? false)),
                        Network = network,
                    };

                var wifiNetworks = (from network in extendedWiFiInformation
                                    where !string.IsNullOrWhiteSpace(network?.Network?.Ssid?.ToString())
                                    group network by network.Network.Ssid.ToString() into g
                                    let firstNetwork = g.First().Network
                                    orderby firstNetwork.Ssid.ToString()
                                    select new WifiNetwork()
                                    {
                                        Connected = g.Any(n => n.Connected),
                                        SSID = firstNetwork.Ssid.ToString(),
                                        SignalQuality = (int)g.Average(n => n.Network.SignalQuality),
                                    }).OrderByDescending(network => network.SignalQuality)
               .ToList();

                _wifiNetworks.Repopulate(wifiNetworks);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception while loading available wifi list.");
            }
        }

        private void OnNetworkChanged(object sender, EventArgs e)
        {
            var newSsid = GetCurrentSsid();
            if (!string.IsNullOrWhiteSpace(newSsid) && _currentSsid != newSsid)
            {
                SetConnectionStatus(newSsid, true);
                SetConnectionStatus(_currentSsid, false);                
                NetworkChanged?.Invoke(this, new EventArgs());                
            }
            _currentSsid = newSsid;
        }
        private string GetCurrentSsid()
        {
            return NativeWifi.EnumerateInterfaceConnections().FirstOrDefault(n => n.State == InterfaceState.Connected)?.ProfileName;
        }

        private void SetConnectionStatus(string ssid, bool status)
        {
            var wifiNetwork = _wifiNetworks.FirstOrDefault(network => network.SSID == ssid);
            if (wifiNetwork != null)
            {
                wifiNetwork.Connected = status;
            }
        }

        public void Dispose()
        {
            NetworkChange.NetworkAddressChanged -= OnNetworkChanged;
        }
    }
}
