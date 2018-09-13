// <copyright file="LoginView.xaml.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using MahApps.Metro.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using VpnSDK.WLVpn.Helpers;
using VpnSDK.WLVpn.Resources;
using VpnSDK.WLVpn.ViewModels;

namespace VpnSDK.WLVpn.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        private string _clearPropertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginView"/> class.
        /// </summary>
        public LoginView()
        {
            InitializeComponent();
            LoginViewModel lvm = (LoginViewModel)DataContext;
            lvm.PropertyChanged += Lvm_PropertyChanged;

            _clearPropertyName = nameof(lvm.Clear);

            if (Resource.Get<bool>("BRAND_USES_EMAIL", false))
            {
                TextBoxHelper.SetWatermark(Username, Strings.LOGIN_EMAILBOX_HINT);
                ForgotButton.Content = Strings.LOGIN_FORGOT_EMAIL_PASSWORD;
            }
        }

        private void Lvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(_clearPropertyName))
            {
                Password.Clear();
            }
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            LoginViewModel lvm = (LoginViewModel)DataContext;
            lvm.Password = Password.Password;
        }
    }
}