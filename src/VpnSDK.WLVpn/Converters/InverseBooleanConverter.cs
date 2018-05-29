// <copyright file="InverseBooleanConverter.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Windows.Data;

namespace VpnSDK.WLVpn.Converters
{
    /// <summary>
    /// Class InverseBooleanConverter. Converts boolean values in to their inverse.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class InverseBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value in to their inverse value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <exception cref="InvalidOperationException">The target must be a boolean.</exception>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool booleanResult)
            {
                return !(bool?)booleanResult;
            }

            return value != null;
        }

        /// <summary>
        /// Converts a inverted boolean value in their original value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool booleanResult)
            {
                return !(bool?)booleanResult;
            }

            return value != null;
        }
    }
}