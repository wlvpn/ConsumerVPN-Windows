// <copyright file="SettingsLicensesView.xaml.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Resources;

namespace VpnSDK.WLVpn.Views
{
    /// <summary>
    /// Interaction logic for SettingsLicensesView.xaml
    /// </summary>
    public partial class SettingsLicensesView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsLicensesView"/> class.
        /// </summary>
        public SettingsLicensesView()
        {
            InitializeComponent();

            // this resource location should come from the branding xaml.
            StreamResourceInfo streamResourceInfo =
               Application.GetResourceStream(new Uri("pack://application:,,,/WLVpn;component/Resources/Branding/Assets/licenses.rtf", UriKind.RelativeOrAbsolute));
            if (streamResourceInfo != null)
            {
                using (var stream = streamResourceInfo.Stream)
                {
                    TextRange textRange = new TextRange(LicenseRichBoxFlowDocument.ContentStart, LicenseRichBoxFlowDocument.ContentEnd);
                    textRange.Load(stream, System.Windows.DataFormats.Rtf);
                }
            }
        }
    }
}
