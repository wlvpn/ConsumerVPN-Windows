using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WLVPN.ValueConverters
{
    public class CountryCodeToFlagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var resourceKey = (string)value;

            if (string.IsNullOrEmpty((string)value))
            {
                return (ImageSource)Application.Current.Resources[$"SpeedometerIcon"];
            }

            if (resourceKey == "UK")
            {
                resourceKey = "GB";
            }

            return (ImageSource)Application.Current.Resources[$"CountryFlag_{resourceKey}"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}