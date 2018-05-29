// <copyright file="SettingsLicensesViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using VpnSDK.WLVpn.Helpers;
using VpnSDK.WLVpn.Views;

namespace VpnSDK.WLVpn.ViewModels
{
    /// <summary>
    /// Class SettingsLicensesViewModel. Provides the View Model for the <see cref="SettingsLicensesView"/>.
    /// </summary>
    /// <seealso cref="BindableBase" />
    /// <seealso cref="ISettingsViewModel" />
    public class SettingsLicensesViewModel : BindableBase, ISettingsViewModel
    {
        /// <inheritdoc/>
        public bool Cancel()
        {
            return true;
        }

        /// <inheritdoc/>
        public bool Save()
        {
            return true;
        }
    }
}