using Caliburn.Micro;
using WLVPN.Extensions;
using WLVPN.Interfaces;
using WLVPN.Utils;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VpnSDK.Enums;
using VpnSDK.Interfaces;
using WLVPN.Helpers;

namespace WLVPN.ViewModels
{
    internal class LoginViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ISDK _sdk;
        private AuthenticationStatus _previousState = AuthenticationStatus.NotAuthenticated;

        public IDialogManager Dialog { get; }

        public AuthenticationStatus State { get; set; } = AuthenticationStatus.NotAuthenticated;

        [AlsoNotifyFor("CanLogin")]
        public string Username { get; set; }

        [AlsoNotifyFor("CanLogin")]
        public string Password { get; set; }

        public string ProgressText { get; set; }

        public LoginViewModel(IEventAggregator eventAggregator, ISDK sdk, IDialogManager dialogManager)
        {
            _eventAggregator = eventAggregator;
            _sdk = sdk;
            State = AuthenticationStatus.NotAuthenticated;
            Dialog = dialogManager;

            _sdk.AuthenticationStatusChanged += OnAuthenticationStatusChanged;
        }

        protected override async void OnInitialize()
        {
            base.OnInitialize();

            if (!string.IsNullOrEmpty(Properties.Settings.Default.Username) && !string.IsNullOrEmpty(Properties.Settings.Default.Password))
            {
                Username = Properties.Settings.Default.Username;
                Password = ProtectData.UnprotectString(Properties.Settings.Default.Password);
                await Login();
            }
        }

        private void OnAuthenticationStatusChanged(ISDK sender, AuthenticationStatus status)
        {
            if (status != AuthenticationStatus.Authenticated)
            {
                State = status;
                if (State == AuthenticationStatus.InProgress) {
                    if (_previousState == AuthenticationStatus.NotAuthenticated)
                    {
                        ProgressText = $"{Properties.Strings.LoggingIn}{Properties.Strings.ProgressSuffix}";
                    }
                    if (_previousState == AuthenticationStatus.Authenticated)
                    {
                        ProgressText = $"{Properties.Strings.LoggingOut}{Properties.Strings.ProgressSuffix}";
                    }
                }
            }
            _previousState = status;
        }

        public async Task Login()
        {
            if (State == AuthenticationStatus.InProgress)
            {
                return;
            }

            try
            {
                var trimmedUsername = Username.Trim();

                await _sdk.Login(trimmedUsername, Password);

                Properties.Settings.Default.Username = trimmedUsername;
                Properties.Settings.Default.Password = ProtectData.ProtectString(Password);
                Properties.Settings.Default.Save();

                Username = "";
                Password = null;

                _sdk.AllowOnlyVPNConnectivity = Properties.Settings.Default.KillSwitch;
                _sdk.AllowLANTraffic = !Properties.Settings.Default.BlockLANTraffic;
                _sdk.DisableDNSLeakProtection = Properties.Settings.Default.DisableDNSLeakProtection;
                _sdk.DisableIPv6LeakProtection = Properties.Settings.Default.DisableIPv6LeakProtection;
                _sdk.AllowLocalAdaptersWhenConnected = Properties.Settings.Default.AllowLanInterfaces;
            }
            catch (Exception e)
            {
                Dialog.ShowMessageBox(e.Message, Properties.Strings.Error);
            }
        }

        public bool CanLogin => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

        /// <summary>
        /// Directs the user to the web page to sign up for an account.
        /// </summary>
        public void SignUp()
        {
            ProcessExtensions.LaunchUrl(Resource.Get<Uri>("SignUpUrl"));
        }
        public bool CanSignUp => true;

        /// <summary>
        /// Directs the user to the web page to reset the password for their account.
        /// </summary>
        public void ForgotPassword()
        {
            ProcessExtensions.LaunchUrl(Resource.Get<Uri>("ForgotPasswordUrl"));
        }
    }
}