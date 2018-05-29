// <copyright file="NullToCollapsedConverter.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VpnSDK.WLVpn.Converters
{
    /// <summary>
    /// Class NullToCollapsedConverter. Converts an object to a <see cref="Visibility.Collapsed"/> if null, otherwise <see cref="Visibility.Visible"/>
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class NullToCollapsedConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value to a <see cref="Visibility.Collapsed"/> based upon equality between the value and null.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns><see cref="Visibility.Collapsed"/> if null, else <see cref="Visibility.Visible"/></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Converts a visibility object back to it's original object if not <c>null</c>. Not in use and will always throw an exception.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <exception cref="NotImplementedException">Always is thrown.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}