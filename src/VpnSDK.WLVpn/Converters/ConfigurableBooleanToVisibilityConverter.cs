// <copyright file="ConfigurableBooleanToVisibilityConverter.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace VpnSDK.WLVpn.Converters
{
    /// <summary>
    /// Class ConfigurableBooleanToVisibilityConverter. Converts from a <see cref="bool"/> value to a <see cref="Visibility"/> and vice versa.
    /// </summary>
    /// <seealso cref="System.Windows.Markup.MarkupExtension" />
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public class ConfigurableBooleanToVisibilityConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// Gets or sets the <see cref="Visibility"/> when the value is <c>true</c>.
        /// </summary>
        /// <value>The <see cref="Visibility"/> value to return when <c>true</c>.</value>
        [DefaultValue(Visibility.Visible)]
        public Visibility WhenTrue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Visibility"/> when the value is <c>false</c>.
        /// </summary>
        /// <value>The <see cref="Visibility"/> value to return when <c>false</c>.</value>
        [DefaultValue(Visibility.Collapsed)]
        public Visibility WhenFalse { get; set; }

        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the
        /// value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>The object value to set on the property where the extension is applied.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        /// <summary>
        /// Converts a <see cref="bool"/> value to a <see cref="Visibility"/>.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        /// <exception cref="InvalidOperationException">Value provided is not a <see cref="bool"/>.</exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value ? WhenTrue : WhenFalse;
            }

            throw new NotSupportedException($"Value provided is a {value?.GetType()} when a boolean was expected.");
        }

        /// <summary>
        /// Converts a <see cref="Visibility"/> back to a <see cref="bool"/>.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == WhenTrue;
            }

            return false;
        }
    }
}