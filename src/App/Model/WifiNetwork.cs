using ManagedNativeWifi;

namespace WLVPN.Model
{
    /// <summary>
    /// Wifi network class.
    /// </summary>
    public class WifiNetwork
    {
        /// <summary>
        /// Gets or sets SSID value.
        /// </summary>
        public string SSID { get; set; }

        /// <summary>
        /// Gets or sets signal quality of network.
        /// </summary>
        public int SignalQuality { get; set; }

        /// <summary>
        /// Gets or sets wireless LAN information on available network.
        /// </summary>
        public AvailableNetworkPack Network { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether WiFi network is connected.
        /// </summary>
        public bool Connected { get; set; }
    }
}
