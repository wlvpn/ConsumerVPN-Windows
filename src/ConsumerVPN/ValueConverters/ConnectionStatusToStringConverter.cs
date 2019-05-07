using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using VpnSDK.Enums;

namespace WLVPN.ValueConverters
{
    public class ConnectionStatusToStringStateConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is ConnectionStatus status)
            {
                switch (status)
                {
                    case ConnectionStatus.Disconnected:
                        return Properties.Strings.Connect.ToUpperInvariant();
                    case ConnectionStatus.Connecting:
                        return Properties.Strings.Connecting.ToUpperInvariant();
                    case ConnectionStatus.Connected:
                        return Properties.Strings.Disconnect.ToUpperInvariant();
                    case ConnectionStatus.Disconnecting:
                        return Properties.Strings.Disconnecting.ToUpperInvariant();
                }

            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
