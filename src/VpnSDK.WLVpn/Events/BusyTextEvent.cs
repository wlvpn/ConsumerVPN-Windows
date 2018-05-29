// <copyright file="BusyTextEvent.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

namespace VpnSDK.WLVpn.Events
{
    /// <summary>
    /// Class BusyTextEvent. Describes an event passed through a <see cref="IEventAggregator"/> that describes an operation is occurring.
    /// </summary>
    public class BusyTextEvent
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; set; }
    }
}