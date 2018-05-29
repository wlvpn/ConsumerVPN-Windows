// <copyright file="ShowDialogEvent.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

namespace VpnSDK.WLVpn.Events
{
    /// <summary>
    /// ShowDialogEvent
    /// </summary>
    public class ShowDialogEvent
    {
        /// <summary>
        /// Gets or sets the dialog actions to use in the popup
        /// </summary>
        public DialogAction DialogAction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show or hide the dialog
        /// </summary>
        public bool Show { get; set; } = true;
    }
}
