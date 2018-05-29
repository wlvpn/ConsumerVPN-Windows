// <copyright file="CountryCodeToFlagConverter.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace VpnSDK.WLVpn.Converters
{
    /// <summary>
    /// Class CountryCodeToFlagConverter. Converts an ISO 3166-1 alpha-2 country code <see cref="string"/> into a flag <see cref="ImageSource"/> as defined in the Resources/Flags/Flags.xaml.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    internal class CountryCodeToFlagConverter : IValueConverter
    {
        /// <summary>
        /// Converts an ISO 3166-1 alpha-2 country code <see cref="string"/> into a flag <see cref="ImageSource"/>.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>An <see cref="ImageSource"/> representing the flag. If not found or invalid, a flag representing an unknown location is provided.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string upCase = value.ToString().ToUpper();
                ImageSource flag = (ImageSource)Application.Current.Resources[$"CountryFlag_{upCase}"];
                return flag ?? (ImageSource)Application.Current.Resources[$"CountryFlag_UNKNOWN"];
            }
            catch
            {
                return (ImageSource)Application.Current.Resources[$"CountryFlag_UNKNOWN"];
            }
        }

        /// <summary>
        /// Converts an <see cref="ImageSource"/> back to an ISO 3166-1 alpha-2 country code <see cref="string"/>. This will always thrown an exception and is not implemented.
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