// <copyright file="ShowViewEvent.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using VpnSDK.WLVpn.Common;

namespace VpnSDK.WLVpn.Events
{
    /// <summary>
    /// ShowViewEvent
    /// </summary>
    public class ShowViewEvent
    {
        /// <summary>
        /// Gets or sets the id of the view to show
        /// </summary>
        public ViewList.Views ID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show or hide the view
        /// </summary>
        public bool Show { get; set; } = true;
    }
}
