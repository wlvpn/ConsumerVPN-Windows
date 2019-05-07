using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WLVPN.ValueConverters
{
    public class IPAddressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                if (!string.IsNullOrEmpty(str) && IPAddress.TryParse(str, out var newAddress))
                {
                    value = newAddress;
                }
            }

            if (value is IPAddress address)
            {
                if (address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    string ip = address.ToString();
                    var removedExtraZeros = ip.Replace("0000", "*");
                    var regex = new Regex(":0+");
                    removedExtraZeros = regex.Replace(removedExtraZeros, ":");
                    var regex2 = new Regex(":\\*:\\*(:\\*)+:");
                    removedExtraZeros = regex2.Replace(removedExtraZeros, "::");
                    return removedExtraZeros.Replace("*", "0");
                }

                return address.ToString();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                if (!string.IsNullOrEmpty(str) && IPAddress.TryParse(str, out var address))
                {
                    return address;
                }
            }

            if (value is IPAddress ip)
            {
                return ip;
            }

            return null;
        }
    }
}