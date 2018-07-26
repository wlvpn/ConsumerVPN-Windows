// <copyright file="DisconnectedView.xaml.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System.ComponentModel;
using System.Windows.Controls;
using VpnSDK.WLVpn.ViewModels;

namespace VpnSDK.WLVpn.Views
{
    /// <summary>
    /// Interaction logic for DisconnectedView.xaml
    /// </summary>
    public partial class DisconnectedView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectedView"/> class.
        /// </summary>
        public DisconnectedView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DisconnectedViewModel context = DataContext as DisconnectedViewModel;
                context?.Init();
            }
        }
    }
}