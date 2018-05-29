// <copyright file="SettingsAboutViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System.Reflection;
using VpnSDK.WLVpn.Helpers;
using VpnSDK.WLVpn.Views;

namespace VpnSDK.WLVpn.ViewModels
{
    /// <summary>
    /// Class SettingsAboutViewModel. Provides the View Model for the <see cref="SettingsAboutView"/>
    /// </summary>
    /// <seealso cref="BindableBase" />
    /// <seealso cref="ISettingsViewModel" />
    public class SettingsAboutViewModel : BindableBase, ISettingsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsAboutViewModel"/> class.
        /// </summary>
        public SettingsAboutViewModel()
        {
            if (IsInDesignMode())
            {
                Init();
                return;
            }
        }

        /// <summary>
        /// Gets the version of the application
        /// </summary>
        public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <inheritdoc/>
        public bool Cancel()
        {
            return true;
        }

        /// <inheritdoc />
        public bool Save()
        {
            return true;
        }

        /// <summary>
        /// This method is used to initialize any data needed at design time to help the Visual Studio designer display data
        /// </summary>
        private void Init()
        {
        }
    }
}