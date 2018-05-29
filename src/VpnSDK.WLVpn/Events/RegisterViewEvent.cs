// <copyright file="RegisterViewEvent.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

namespace VpnSDK.WLVpn.Events
{
    /// <summary>
    /// Class RegisterViewEvent. Describes an event passed through a <see cref="IEventAggregator"/> that describes a view being registered.
    /// </summary>
    public class RegisterViewEvent
    {
        /// <summary>
        /// Gets or sets a value indicating the view to define.
        /// </summary>
        public ViewDefinition View { get; set; }
    }
}