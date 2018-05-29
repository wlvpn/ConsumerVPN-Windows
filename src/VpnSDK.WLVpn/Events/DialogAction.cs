// <copyright file="DialogAction.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;

namespace VpnSDK.WLVpn.Events
{
    /// <summary>
    /// Class DialogAction. Represents a dialog boxes actions.
    /// </summary>
    public class DialogAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogAction"/> class.
        /// </summary>
        public DialogAction()
        {
            OKString = Resources.Branding.Strings.OK;
            CancelString = Resources.Branding.Strings.CANCEL;
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the OK string.
        /// </summary>
        /// <value>The ok string.</value>
        public string OKString { get; set; } = "OK";

        /// <summary>
        /// Gets or sets the action to execute on OK being pressed.
        /// </summary>
        /// <value>The OK action.</value>
        public Action OKAction { get; set; }

        /// <summary>
        /// Gets or sets the cancel string.
        /// </summary>
        /// <value>The cancel string.</value>
        public string CancelString { get; set; } = "Cancel";  // last resort.

        /// <summary>
        /// Gets or sets the action to execute on Cancel being pressed.
        /// </summary>
        /// <value>The cancel action.</value>
        public Action CancelAction { get; set; }

        /// <summary>
        /// Gets or sets the string displayed for more information when applicable.
        /// </summary>
        /// <value>The other string.</value>
        public string OtherString { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the action when the <see cref="OtherString"/> button is pressed.
        /// </summary>
        /// <value>The other action.</value>
        public Action OtherAction { get; set; }
    }
}