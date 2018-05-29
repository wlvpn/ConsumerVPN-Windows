// <copyright file="ShowNotificationEvent.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using Hardcodet.Wpf.TaskbarNotification;

namespace VpnSDK.WLVpn.Events
{
    /// <summary>
    /// ShowNotificationEvent
    /// </summary>
    public class ShowNotificationEvent
    {
        /// <summary>
        /// Gets or sets the title to use on the notification popup
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the text to use on the notification popup
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the icon to use on the notification popup
        /// </summary>
        public BalloonIcon Icon { get; set; } = BalloonIcon.Info;
    }
}
