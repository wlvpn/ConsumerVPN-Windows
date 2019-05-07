using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace WLVPN.Extensions
{
    public static class NetworkInterfaceExtensions
    {
        public static NetworkInterface GetVpnInterface()
        {
            return NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(networkInterface =>
                networkInterface.OperationalStatus == OperationalStatus.Up &&
                (networkInterface.Name.Contains("ConsumerVPN") ||
                 networkInterface.Description.StartsWith("TAP-Win", true, CultureInfo.InvariantCulture)));
        }
    }
}