// <copyright file="EnumToVisibilityConverter.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace VpnSDK.WLVpn.Converters
{
    /// <summary>
    /// Class EnumToVisibilityConverter. Converts from a comparison of value to the parameter to a <see cref="Visibility"/>.
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class EnumToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the <see cref="Visibility"/> used when the input value matches the parameter.
        /// </summary>
        [DefaultValue(Visibility.Visible)]
        public Visibility WhenTrue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Visibility"/> used when the input value does not match the parameter.
        /// </summary>
        [DefaultValue(Visibility.Collapsed)]
        public Visibility WhenFalse { get; set; }

        /// <summary>
        /// Converts a <see cref="Enum"/> to a <see cref="Visibility"/> based upon equality between the value and the parameter.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The <see cref="Visibility"/> in <see cref="WhenTrue"/> if the value and the parameter match, else <see cref="WhenFalse"/></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(parameter) == true ? WhenTrue : WhenFalse;
        }

        /// <summary>
        /// Converts the value back to it's parameter if <c>true</c>.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The parameter is <c>true</c> else, a <see cref="Binding.DoNothing"/>.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }
}