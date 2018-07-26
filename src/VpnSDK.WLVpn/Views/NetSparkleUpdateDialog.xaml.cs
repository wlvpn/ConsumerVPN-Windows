// <copyright file="NetSparkleUpdateDialog.xaml.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;
using NetSparkle;
using NetSparkle.Enums;
using NetSparkle.Interfaces;
using VpnSDK.WLVpn.Resources;

namespace VpnSDK.WLVpn.Views
{
    /// <summary>
    /// Interaction logic for NetSparkleUpdateDialog.xaml
    /// </summary>
    public partial class NetSparkleUpdateDialog : MetroWindow, IUpdateAvailable
    {
        public NetSparkleUpdateDialog(AppCastItem appCast)
        {
            InitializeComponent();
            if (Application.Current?.MainWindow != null)
            {
                Owner = Application.Current.MainWindow;
            }

            DataContext = this;
            CurrentItem = appCast;

            if (string.IsNullOrEmpty(appCast.ReleaseNotesLink) == false)
            {
                Browser.Navigate(new Uri(appCast.ReleaseNotesLink));
            }
            else if (string.IsNullOrEmpty(appCast.Description) == false)
            {
                Browser.NavigateToString(appCast.Description);
            }

            Description.Content = string.Format(
                Strings.UPDATE_DIALOG_DESCRIPTION,
                Helpers.Resource.Get<string>("BRAND_NAME"),
                CurrentItem.Version,
                CurrentItem.AppVersionInstalled);

            UpdateHeader.Header = string.Format(Strings.UPDATE_DIALOG_HEADER, Helpers.Resource.Get<string>("BRAND_NAME"));
        }

        public ImageSource Icon => Application.Current.MainWindow.Icon;

        public UpdateAvailableResult Result { get; private set; }

        public AppCastItem CurrentItem { get; }

        public event EventHandler UserResponded;

        private void OnInstallClick(object sender, RoutedEventArgs e)
        {
            Result = UpdateAvailableResult.InstallUpdate;
            Close();
            UserResponded?.Invoke(this, new EventArgs());
        }

        private void OnSkipClick(object sender, RoutedEventArgs e)
        {
            Result = UpdateAvailableResult.SkipUpdate;
            UserResponded?.Invoke(this, new EventArgs());
        }

        private void OnRemindClick(object sender, RoutedEventArgs e)
        {
            Result = UpdateAvailableResult.RemindMeLater;
            UserResponded?.Invoke(this, new EventArgs());
        }

        public void HideReleaseNotes()
        {
            Browser.Visibility = Visibility.Collapsed;
        }

        public void HideRemindMeLaterButton()
        {
            // ignore
        }

        public void HideSkipButton()
        {
            // ignore
        }

        public void BringToFront()
        {
            Activate();
        }

        void IUpdateAvailable.Show()
        {
            try
            {
                ShowDialog();
                Close();
            }
            catch
            {
                // ignore
            }
        }
    }
}
