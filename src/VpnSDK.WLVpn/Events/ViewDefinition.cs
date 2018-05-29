// <copyright file="ViewDefinition.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using VpnSDK.WLVpn.Common;

namespace VpnSDK.WLVpn.Events
{
    /// <summary>
    /// class used to define the views used in the application
    /// </summary>
    public class ViewDefinition
    {
        /// <summary>
        /// Gets or sets the value indicating the title to use
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the value indicating the id of the view
        /// </summary>
        public ViewList.Views ID { get; set; }

        /// <summary>
        /// Gets or sets the instance of the view for this id
        /// </summary>
        public object View { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the view is used as an overlay/dialog
        /// </summary>
        public bool IsOverLay { get; set; } = false;
    }
}
