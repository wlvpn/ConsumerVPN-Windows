// <copyright file="ViewList.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

namespace VpnSDK.WLVpn.Common
{
    /// <summary>
    /// Class ViewList. A container that holds enumeration types representing sets of views available for navigation.
    /// </summary>
    public class ViewList
    {
        /// <summary>
        /// Enum Views. Represents a navigational view in the application.
        /// </summary>
        public enum Views
        {
            /// <summary>
            /// The login view.
            /// </summary>
            Login,

            /// <summary>
            /// The VPN connected view.
            /// </summary>
            Connected,

            /// <summary>
            /// The VPN disconnected view.
            /// </summary>
            Disconnected,

            /// <summary>
            /// The server list view.
            /// </summary>
            ServerList,

            /// <summary>
            /// The settings view.
            /// </summary>
            Settings,

            /// <summary>
            /// The view used to represent navigating back.
            /// </summary>
            Back,

            /// <summary>
            /// The pop up messagebox 'Show Dialog' view.
            /// </summary>
            ShowDialog,

            /// <summary>
            /// The main window view.
            /// </summary>
            MainWindow
        }
    }
}