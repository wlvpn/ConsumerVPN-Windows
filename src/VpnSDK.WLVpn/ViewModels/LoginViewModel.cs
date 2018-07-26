// <copyright file="LoginViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using VpnSDK.WLVpn.Events;
using VpnSDK.WLVpn.Extensions;
using VpnSDK.WLVpn.Helpers;
using VpnSDK.WLVpn.Views;

namespace VpnSDK.WLVpn.ViewModels
{
    /// <summary>
    /// Class LoginViewModel. Represents the view model for <see cref="LoginView"/>.
    /// </summary>
    /// <seealso cref="BindableBase" />
    public class LoginViewModel : BindableBase
    {
        private EventAggregator _eventAggregator = null;
        private RelayCommand _loginCmd = null;
        private RelayCommand _signUpCmd = null;
        private RelayCommand _forgotPasswordCmd = null;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _clear = false;
        private IDisposable _logoutSubscription = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
        /// </summary>
        /// <param name="sdkMonitor">The SDK monitor.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        public LoginViewModel(SDKMonitor sdkMonitor, EventAggregator eventAggregator)
        {
            if (IsInDesignMode())
            {
                return;
            }

            SDKMonitor = sdkMonitor;
            _eventAggregator = eventAggregator;

            _logoutSubscription = _eventAggregator.GetEvent<LogOutEvent>().Subscribe(
                (msg) =>
                {
                    RunOnDisplayThread(() =>
                    {
                        Username = string.Empty;
                        Password = string.Empty;
                        Clear = true; // this is to trigger the password field to clear itself.});
                    });
                });
        }

        /// <summary>
        /// Gets the login command.
        /// </summary>
        /// <value>The login command.</value>
        public RelayCommand LoginCmd
        {
            get
            {
                if (_loginCmd == null)
                {
                    _loginCmd = new RelayCommand(
                        (parm) =>
                        {
                            Login();
                        },
                        (parm) => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password));
                }

                return _loginCmd;
            }
        }

        /// <summary>
        /// Gets the Sign up command
        /// </summary>
        /// <value>The Signup command.</value>
        public RelayCommand SignUpCmd
        {
            get
            {
                if (_signUpCmd == null)
                {
                    _signUpCmd = new RelayCommand(
                        (parm) =>
                        {
                            LinkTo(Resource.Get<string>("BRAND_REGISTER_URL", "https://wlvpn.com/#contact"));
                        },
                        (parm) => true);
                }

                return _signUpCmd;
            }
        }

        /// <summary>
        /// Gets the Forgot Password Command
        /// </summary>
        /// <value>The Signup command.</value>
        public RelayCommand ForgotPasswordCmd
        {
            get
            {
                if (_forgotPasswordCmd == null)
                {
                    _forgotPasswordCmd = new RelayCommand(
                        (parm) =>
                        {
                            LinkTo(Resource.Get<string>("BRAND_FORGOT_PASSWORD_URL", "Stackpath.com"));
                        },
                        (parm) => true);
                }

                return _forgotPasswordCmd;
            }
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LoginViewModel"/> is clear.
        /// </summary>
        /// <value><c>true</c> if clear; otherwise, <c>false</c>.</value>
        public bool Clear
        {
            get { return _clear; }
            set { OnPropertyChanged("Clear"); }
        }

        private SDKMonitor SDKMonitor { get; set; }

        private void Login()
        {
            SDKMonitor.LogIn(Username, Password);

            Properties.Settings.Default.Username = Username.Protect();
            Properties.Settings.Default.Password = Password.Protect();
            Properties.Settings.Default.Save();

            Password = string.Empty;
            Username = string.Empty;

            Clear = true;
        }

        public void Init()
        {
            try
            {
                Username = Properties.Settings.Default.Username.Unprotect();
                Password = Properties.Settings.Default.Password.Unprotect();
                if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
                {
                    Login();
                }
            }
            catch
            {
                // Probably no user/pass. Move on.
            }
        }

        private Process LinkTo(string uri)
        {
            try
            {
                return Process.Start(new ProcessStartInfo("explorer.exe", $"\"{uri}\""));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}