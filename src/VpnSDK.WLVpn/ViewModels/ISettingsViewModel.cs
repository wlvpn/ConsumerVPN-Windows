// <copyright file="ISettingsViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

namespace VpnSDK.WLVpn.ViewModels
{
    /// <summary>
    /// Interface ISettingsViewModel. Provides mandatory methods for view models in the settings view.
    /// </summary>
    public interface ISettingsViewModel
    {
        /// <summary>
        /// Saves all associated application settings related with the view model.
        /// </summary>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        bool Save();

        /// <summary>
        /// Cancels all changes made to application settings related to the view model.
        /// </summary>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        bool Cancel();
    }
}