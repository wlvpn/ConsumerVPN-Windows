// <copyright file="DescriptionAttributeConverter.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace VpnSDK.WLVpn.Converters
{
    /// <summary>
    /// Class DescriptionAttributeConverter. Converts objects, fields or properties with <see cref="DescriptionAttribute"/> implemented to their description string.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class DescriptionAttributeConverter : IValueConverter
    {
        /// <summary>
        /// Gets the description string on anything that uses the <see cref="DescriptionAttribute"/>.
        /// </summary>
        /// <param name="value">The object to extract the description value from.</param>
        /// <returns>The description string if <see cref="DescriptionAttribute"/> is used, an empty string if not.</returns>
        public static string GetDescription(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            try
            {
                FieldInfo field = value.GetType().GetField(value.ToString());
                return field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .Cast<DescriptionAttribute>()
                            .Select(x => x.Description)
                            .DefaultIfEmpty(value.ToString())
                            .FirstOrDefault();
            }
            catch
            {
            }

            return string.Empty;
        }

        /// <summary>
        /// Converts a value that uses a <see cref="DescriptionAttribute"/> to the description string.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetDescription(value);
        }

        /// <summary>
        /// Converts a value back from a <see cref="DescriptionAttribute"/> to an object. This is not implemented and will always throw an exception.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <exception cref="NotImplementedException">Always thrown.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}