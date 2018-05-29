// <copyright file="BasicDialogView.xaml.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System.Windows;
using System.Windows.Controls;
using VpnSDK.WLVpn.Events;
using VpnSDK.WLVpn.ViewModels;

namespace VpnSDK.WLVpn.Views
{
    /// <summary>
    /// Interaction logic for BasicDialogView.xaml
    /// </summary>
    public partial class BasicDialogView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicDialogView"/> class.
        /// </summary>
        public BasicDialogView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sets the dialog action.
        /// </summary>
        /// <param name="action">The <see cref="DialogAction"/> to perform.</param>
        public void SetDialogAction(DialogAction action)
        {
            if (DataContext is BasicDialogViewModel viewModel)
            {
                viewModel.WhatToDo = action;
            }
        }

        private void BasicDialogView_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Focus();
            }
        }
    }
}