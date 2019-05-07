using WLVPN.ViewModels;
using System.Windows;
using VpnSDK.Interfaces;
using WPFLocalizeExtension.Providers;
using System.Windows.Input;
using System;
using System.Globalization;

namespace WLVPN.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        private ILocation _location;
        private bool _isLoaded = false;

        public ShellView()
        {
            ResxLocalizationProvider.SetDefaultAssembly(this, AppBootstrapper.AssemblyName);
            ResxLocalizationProvider.SetDefaultDictionary(this, "Strings");
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded == true)
            {
                return;
            }
            if (DataContext is ShellViewModel vm)
            {
                vm.SDK.VpnConnectionStatusChanged += OnVpnConnectionStatusChanged;
            }
            _isLoaded = true;
        }

        private void OnVpnConnectionStatusChanged(ISDK sender, VpnSDK.Enums.ConnectionStatus previous, VpnSDK.Enums.ConnectionStatus current)
        {
            if (current == VpnSDK.Enums.ConnectionStatus.Disconnected && (previous != VpnSDK.Enums.ConnectionStatus.Disconnected && previous != VpnSDK.Enums.ConnectionStatus.Connecting))
            {
                if (_location != null)
                {
                    TaskbarIconControl.ShowBalloonTip(
                        Properties.Strings.BalloonNotificationTitle, 
                        string.Format(CultureInfo.InvariantCulture, Properties.Strings.DisconnectedBalloon, $"{_location.City}, {_location.Country}"),
                        Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                }
            }
            else if (current == VpnSDK.Enums.ConnectionStatus.Connected)
            {
                _location = sender.ActiveConnectionInformation?.Location;
                if (_location != null)
                {
                    TaskbarIconControl.ShowBalloonTip(
                        Properties.Strings.BalloonNotificationTitle, 
                        string.Format(CultureInfo.InvariantCulture, Properties.Strings.ConnectedBalloon, $"{_location.City}, {_location.Country}"),
                        Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                }
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_isLoaded == false)
            {
                return;
            }
            if (DataContext is ShellViewModel vm)
            {
                vm.SDK.VpnConnectionStatusChanged -= OnVpnConnectionStatusChanged;
            }

            _isLoaded = false;
        }
    }
}