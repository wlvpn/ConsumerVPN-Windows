using Caliburn.Micro;
using WLVPN.Enums;
using WLVPN.Extensions;
using WLVPN.Interfaces;
using WLVPN.Utils;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VpnSDK.DTO;
using VpnSDK.Enums;
using VpnSDK.Interfaces;
using static WLVPN.AppBootstrapper;
using System.Reflection;
using NetSparkle;
using WLVPN.Helpers;
using NetSparkle.Enums;
using Hardcodet.Wpf.TaskbarNotification;
using System.Drawing;
using System.Globalization;

namespace WLVPN.ViewModels
{
    public sealed class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IDisposable
    {
        private ILocation _location = null;
        private int _reconnectAttempts = 0;
        private bool _isTerminating = false;
        private bool _connectionInProcess = false;
        private SAPIWrapper Voice { get; } = null;
        private Sparkle _sparkleUpdater;

        public ShellViewModel(ISDK sdk, IDialogManager dialogManager, IBusyManager busyManager, SAPIWrapper speech)
        {
            SDK = sdk;
            Voice = speech;
            SDK.UserLocationStatusChanged += OnUserLocationStatusChanged;
            SDK.AuthenticationStatusChanged += OnAuthenticationStatusChanged;
            SDK.VpnConnectionStatusChanged += OnVpnConnectionStatusChanged;
            SDK.VpnConnectionStatusChanged += ReconnectOnVpnConnectionStatusChanged;
            SDK.TapDeviceInstallationStatusChanged += OnTapDeviceInstallationStatusChanged;
            Dialog = dialogManager;
            BusyManager = busyManager;
        }

        public string Version { get; set; } = $"v{Assembly.GetExecutingAssembly().GetName().Version}";

        public IDialogManager Dialog { get; }

        public bool IsLoggedIn { get; set; } = false;

        public IBusyManager BusyManager { get; }

        public ResizeMode ResizeMode { get; set; } = ResizeMode.NoResize;

        public ISDK SDK { get; set; }

        public NetworkGeolocation UserLocation { get; set; }

        public void ShowWindow()
        {
            Window window = GetView() as Window;
            if (window != null)
            {
                if (window.WindowState != WindowState.Normal)
                {
                    window.WindowState = WindowState.Normal;
                }
                window.Show();
                window.Activate();
            }
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            if (_sparkleUpdater == null)
            {
                _sparkleUpdater = new Sparkle(
                    Properties.Settings.Default.BetaOptIn ? Resource.Get<string>("BetaAppcastUrl") : Resource.Get<string>("AppcastUrl"),
                    Application.Current?.MainWindow?.Icon?.ToIcon() ?? Icon.ExtractAssociatedIcon(new Uri(typeof(ShellViewModel).Assembly.CodeBase).LocalPath),
                    SecurityMode.Unsafe)
                    {
                        ShowsUIOnMainThread = true,
                        UIFactory = new SparkleUIFactory(),
                    };
#if DEBUG               
                    _sparkleUpdater.LogWriter.PrintDiagnosticToConsole = true;
#endif
                    _sparkleUpdater.StartLoop(true, true, TimeSpan.FromHours(2));
            }

        }



        private void OnTapDeviceInstallationStatusChanged(ISDK sender, OperationStatus status)
        {
            if (status == OperationStatus.InProgress)
            {
                BusyManager.Activate($"{Properties.Strings.Installing}{Properties.Strings.ProgressSuffix}");
            }
            else
            {
                BusyManager.Dismiss();
            }
        }

        private void ReconnectOnVpnConnectionStatusChanged(ISDK sender, ConnectionStatus previous, ConnectionStatus current)
        {
            if (_reconnectAttempts == 0)
            {
                return;
            }
        }

        private void OnVpnConnectionStatusChanged(ISDK sender, ConnectionStatus previous, ConnectionStatus current)
        {
            _connectionInProcess = current == ConnectionStatus.Connecting || current == ConnectionStatus.Disconnecting;

            if (current == ConnectionStatus.Connecting && _reconnectAttempts == 0)
            {
                // First attempt
                BusyManager.Activate($"{Properties.Strings.Connecting}{Properties.Strings.ProgressSuffix}", CancelConnection);
                Voice.Speak(Properties.Strings.Connecting);
            }
            else if (current == ConnectionStatus.Connecting && _reconnectAttempts > 0)
            {
                // Part of reconnection process
                BusyManager.Activate($"{Properties.Strings.Reconnecting}{Properties.Strings.ProgressSuffix}", CancelConnection);
                Voice.Speak(Properties.Strings.Reconnecting);
            }
            else if (current == ConnectionStatus.Disconnecting)
            {
                BusyManager.Activate($"{Properties.Strings.Disconnecting}{Properties.Strings.ProgressSuffix}");
                Voice.Speak(Properties.Strings.Disconnecting);
            }
            else if (current == VpnSDK.Enums.ConnectionStatus.Disconnected && (previous != VpnSDK.Enums.ConnectionStatus.Disconnected && previous != VpnSDK.Enums.ConnectionStatus.Connecting))
            {
                if (_location != null)
                {
                    Voice.Speak(string.Format(CultureInfo.InvariantCulture, Properties.Strings.DisconnectedBalloon, $"{_location.City}, {_location.Country}"));
                }
                BusyManager.Dismiss();
            }
            else if (current == VpnSDK.Enums.ConnectionStatus.Connected)
            {
                _location = sender.ActiveConnectionInformation?.Location;
                if (_location != null) // not needed but who knows.
                {
                    Voice.Speak(string.Format(CultureInfo.InvariantCulture, Properties.Strings.ConnectedBalloon, $"{_location.City}, {_location.Country}"));
                }
                BusyManager.Dismiss();
                _reconnectAttempts = 0;
            }
            else
            {
                BusyManager.Dismiss();
            }

            // Failed to connect || Unexpected disconnect
            if ((current == ConnectionStatus.Disconnected && previous == ConnectionStatus.Connecting && SDK.IsConnectionCancelled == false)
                || (current == ConnectionStatus.Disconnected && previous == ConnectionStatus.Connected))
            {
                SynchronizationContext.Current.Post(async (state) => { await Reconnect().ConfigureAwait(false); }, this);
            }

            NotifyOfPropertyChange(nameof(IsConnectMenuEnabled));
            
        }

        private async Task Reconnect()
        {
            if (Properties.Settings.Default.AutoReconnect && _reconnectAttempts < Properties.Settings.Default.ReconnectTries)
            {
                Log.Information($"Will attempt to reconnect. Attempts made: {_reconnectAttempts}, Max attempts {Properties.Settings.Default.ReconnectTries}");
                bool lastTry = _reconnectAttempts == Properties.Settings.Default.ReconnectTries - 1;
                _reconnectAttempts++;

                var delayPeriod = CalculateReconnectDelay(_reconnectAttempts);
                var cancelled = false;
                await BusyManager.ActivateWithDelay(string.Format(CultureInfo.InvariantCulture, Properties.Strings.ReconnectAfter, delayPeriod.TotalSeconds),
                    delayPeriod,
                    TimeSpan.FromSeconds(1),
                    () => { BusyManager.ChangeBusyText(string.Format(CultureInfo.InvariantCulture, Properties.Strings.ReconnectAfter, BusyManager.DelayRemained.TotalSeconds)); },
                    () =>
                    {
                        _reconnectAttempts = Properties.Settings.Default.ReconnectTries;
                        BusyManager.Dismiss();
                        _reconnectAttempts = 0;
                        cancelled = true;
                        Log.Information("Reconnection canceled.");
                    });
                if (!cancelled)
                {
                    await SDK.Reconnect(lastTry);
                }
            }
            else
            {
                Log.Information("Reconnect attempts exceeded.");
                _reconnectAttempts = 0;
            }
        }

        private static TimeSpan CalculateReconnectDelay(int _reconnectAttempts)
        {
            TimeSpan delayPeriod;
            if (_reconnectAttempts > 5)
            {
                delayPeriod = TimeSpan.FromSeconds(60);
            }
            else
            {
                delayPeriod = TimeSpan.FromSeconds(Math.Pow(2, _reconnectAttempts));
            }
            return delayPeriod;
        }

        private void OnUserLocationStatusChanged(ISDK sender, OperationStatus status, NetworkGeolocation args)
        {
            UserLocation = args;
        }

        private void OnAuthenticationStatusChanged(ISDK sender, AuthenticationStatus status)
        {
            if (status == AuthenticationStatus.Authenticated)
            {
                ActivateItem((Screen)ContainerInstance.GetInstance(typeof(MainViewModel)));
                IsLoggedIn = true;
            }
            else if (status == AuthenticationStatus.NotAuthenticated)
            {
                ActivateItem((Screen)ContainerInstance.GetInstance(typeof(LoginViewModel)));
                IsLoggedIn = false;
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            ActivateItem(ContainerInstance.GetInstance<LoginViewModel>());
        }

        public static void DragWindow()
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.DragMove();
            }
        }

        public static void MinimizeWindow()
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
            }
        }

        public static void MaximizeWindow()
        {
            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.WindowState =
                    Application.Current.MainWindow.WindowState == WindowState.Normal
                        ? WindowState.Maximized
                        : WindowState.Normal;
            }
        }

        public async Task Connect()
        {
            await SDK.InitiateConnection();
        }

        public bool CanConnect()
        {
            return !_connectionInProcess && !SDK.IsConnected && !SDK.IsConnecting && IsLoggedIn;
        }

        public bool IsConnectMenuEnabled
        {
            get
            {
                return CanConnect();
            }
        }

        public async Task Disconnect()
        {
            await SDK.Disconnect();
        }

        public bool CanDisconnect()
        {
            return !_connectionInProcess && SDK.IsConnected;
        }

        private void CancelConnection()
        {
            try
            {
                _reconnectAttempts = 0;
                SDK.GetCancellationTokenSource()?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Ignore
            }
        }

        public void CloseAction()
        {
            // this aint working for some reason.  ?????  TODO
            if (!(ActiveItem is LoginViewModel))
            {
                switch (Properties.Settings.Default.CloseStyle)
                {
                    case ApplicationCloseType.Dialog:
                        Dialog.ShowDialog(new CloseWindowDialogViewModel(), screenResult =>
                        {
                            CloseWindowDialogViewModel result = (CloseWindowDialogViewModel)screenResult;

                            if (result.WasSelected(CloseDialogOptions.Hide))
                            {
                                Properties.Settings.Default.CloseStyle = ApplicationCloseType.Hide;
                                Properties.Settings.Default.Save();
                                HideWindow();
                            }
                            else
                            {
                                Properties.Settings.Default.CloseStyle = ApplicationCloseType.Exit;
                                Properties.Settings.Default.Save();
                                ExitApplication();
                            }
                        });
                        break;

                    case ApplicationCloseType.Hide:
                        HideWindow();
                        break;

                    default:
                        ExitApplication();
                        break;
                }
            }
            else
            {
                ExitApplication();
            }
        }

        public void HideWindow()
        {
            Window window = GetView() as Window;
            window?.Hide();
        }

        public async void ShortcutKey(string key)
        {
            if (Properties.Settings.Default.EnableShortcuts)
            {
                ActivateItem((Screen)ContainerInstance.GetInstance(typeof(MainViewModel)));
                MainViewModel mvm = (MainViewModel)ActiveItem;

                switch (key)
                {

                    case "H":
                        mvm.SelectedIndex = (int)MainScreenTabs.Home;
                        break;

                    case "S":
                        mvm.SelectedIndex = (int)MainScreenTabs.Settings;
                        break;

                    case "F1":
                    case "I":
                        mvm.SelectedIndex = (int)MainScreenTabs.Information;
                        break;

                    case "C":
                        if (CanConnect() == true)
                            await Connect();
                        break;

                    case "D":
                        if (CanDisconnect() == true)
                            await Disconnect();
                        break;
                }
            }
        }

        public void ExitApplication()
        {
            if (_isTerminating)
            {
                return;
            }
            _isTerminating = true;

            try
            {
                SDK.Dispose();
            }
            catch (Exception)
            {
                // Suppress any errors in regards to disconnection failure
            }

            try
            {
                Application.Current?.Shutdown();
            }
            catch (Exception)
            {
                // Suppress any errors in regards to how we choose to exit the application
            }
            _isTerminating = false;
        }

        public void Dispose()
        {
            _sparkleUpdater.Dispose();
        }
    }
}