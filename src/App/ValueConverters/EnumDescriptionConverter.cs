using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WLVPN.ValueConverters
{
    public class EnumDescriptionConverter : IValueConverter
    {
        public static string GetDescription(object value)
        {
            if (value == null)
            {
                return String.Empty;
            }
            FieldInfo field = value.GetType().GetField(value.ToString());
            return field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .Cast<DescriptionAttribute>()
                        .Select(x => x.Description)
                        .DefaultIfEmpty(value.ToString())
                        .FirstOrDefault();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetDescription(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
