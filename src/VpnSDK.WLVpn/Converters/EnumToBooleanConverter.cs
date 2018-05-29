// <copyright file="EnumToBooleanConverter.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Windows.Data;

namespace VpnSDK.WLVpn.Converters
{
    /// <summary>
    /// Class EnumToBooleanConverter. Allows comparing an <see cref="Enum"/> value to the parameter provided and return a boolean value.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class EnumToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Compares the value to the parameter and returns a <see cref="bool"/> if they match.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns><c>true</c> if the value and the parameter match, <c>false</c> if not.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        /// <summary>
        /// Converts a <see cref="bool"/> back to the parameter.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The parameter if <c>true</c>, <see cref="Binding.DoNothing"/> if not.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}