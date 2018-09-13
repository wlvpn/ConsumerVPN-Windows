// <copyright file="SDKMonitor.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using DynamicData;
using DynamicData.Binding;
using Microsoft.Win32.TaskScheduler;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using VpnSDK.Public;
using VpnSDK.Public.Enums;
using VpnSDK.Public.Exceptions;
using VpnSDK.Public.Interfaces;
using VpnSDK.Public.Messages;
using VpnSDK.WLVpn.Events;
using VpnSDK.WLVpn.Helpers;
using Task = System.Threading.Tasks.Task;

namespace VpnSDK.WLVpn.ViewModels
{
    /// <summary>
    /// Class SDKMonitor. This class cannot be inherited. Provides bindable monitoring of the VpnSDK.
    /// </summary>
    /// <seealso cref="BindableBase" />
    public sealed class SDKMonitor : BindableBase
    {
        private static Logger _logger = LogManager.GetLogger("WL::SDKMonitor");

        private ISDK _manager;

        private bool _isConnected = false;
        private bool _isLoggedIn = false;
        private bool _isConnecting = false;
        private bool _killSwitch = false;
        private bool _isBusy = false;
        private string _externIP = string.Empty;
        private DateTime _startTime;
        private DateTime _endTime;
        private DispatcherTimer _timer = new DispatcherTimer();
        private string _currentLocationName = "Over the hill and far away";
        private ILocation _selServer = null;
        private bool _startOnStartup = false;
        private bool _checkStartupTaskOnInit = false;
        private EventAggregator _eventAggregator;
        private IDisposable _serverListLoader;
        private string _search = string.Empty;
        private IDisposable _serverListRefresh;
        private string _connectionState = string.Empty;
        private bool _wasConnected = false;
        private string _username;
        private string _password;

        /// <summary>
        /// Initializes a new instance of the <see cref="SDKMonitor"/> class.
        /// </summary>
        /// <param name="eventAggregator">The application-wide event aggregator.</param>
        public SDKMonitor(EventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            if (Properties.Settings.Default.CallUpgrade)
            {
                _logger.Info("Upgrading user settings.");
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.CallUpgrade = false;
                _logger.Info("Settings were upgraded.");
            }

            // do whatever is needed to create/get the SDk instance
            ConfigureSDK();

            // save and restore local settings
            ReadStoredSettings();

            IsBusy = false;
        }

        /// <summary>
        /// Gets a value that represents the list of <see cref="ILocation"/> available to the client.
        /// </summary>
        /// <value>Currently available <see cref="ILocation"/></value>
        public IObservableCollection<ILocation> Locations { get; } = new ObservableCollectionExtended<ILocation>();

        /// <summary>
        /// Gets or sets a value indicating whether the VPN connection is connected.
        /// </summary>
        /// <value><c>true</c> if this instance is connected; otherwise, <c>false</c>.</value>
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }

            set
            {
                SetProperty(ref _isConnected, value);
                if (value)
                {
                    StartTiming();
                    _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.Connected });
                }
                else
                {
                    StopTimer();
                    _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.Disconnected });
                }

                _logger.Debug("IsConnected == " + value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is logged in.
        /// </summary>
        /// <value><c>true</c> if this instance is logged in; otherwise, <c>false</c>.</value>
        public bool IsLoggedIn
        {
            get
            {
                return _isLoggedIn;
            }

            set
            {
                _logger.Debug("IsLoggedIn == " + value);
                if (value && _isLoggedIn != true)
                {
                    _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.Disconnected });
                }
                else if (!value && _isLoggedIn)
                {
                    // Unsubscribing from the observable that is responsible for broadcasting server list refresh events.
                    // That stops the server list refresh on the SDK side
                    _serverListRefresh?.Dispose();
                    _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.Login });
                }

                SetProperty(ref _isLoggedIn, value);
                RunOnDisplayThread(
                    () =>
                    {
                        CommandManager.InvalidateRequerySuggested();
                    });
            }
        }

        /// <summary>
        /// Gets or sets a value indicating what connection protocol the application should use.
        /// </summary>
        public NetworkConnectionType ConnectionProtocol
        {
            get
            {
                return Properties.Settings.Default.NetworkConnectionType;
            }

            set
            {
                Properties.Settings.Default.NetworkConnectionType = value;
                if (value != NetworkConnectionType.OpenVPN)
                {
                    OpenVpnScramble = false;
                }

                Properties.Settings.Default.Save();
                OnPropertyChanged("ConnectionProtocol");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating what OpenVPN protocol the application should use.
        /// </summary>
        public NetworkProtocolType SelectedOpenVpnProtocol
        {
            get
            {
                return Properties.Settings.Default.NetworkProtocolType;
            }

            set
            {
                Properties.Settings.Default.NetworkProtocolType = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application should use OpenVPN scrambling.
        /// </summary>
        /// <value>true or false</value>
        public bool OpenVpnScramble
        {
            get
            {
                return Properties.Settings.Default.OpenVpnScramble;
            }

            set
            {
                Properties.Settings.Default.OpenVpnScramble = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets the OpenVPN cipher type.
        /// </summary>
        public OpenVpnCipherType CipherType
        {
            get
            {
                return Properties.Settings.Default.CipherType;
            }

            set
            {
                Properties.Settings.Default.CipherType = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged("CipherType");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is connecting to a VPN server.
        /// </summary>
        /// <value><c>true</c> if this instance is connecting; otherwise, <c>false</c>.</value>
        public bool IsConnecting
        {
            get
            {
                return _isConnecting;
            }

            set
            {
                SetProperty(ref _isConnecting, value);
                _logger.Debug("IsConnecting == " + value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is busy.
        /// </summary>
        /// <value><c>true</c> if this instance is busy; otherwise, <c>false</c>.</value>
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }

            set
            {
                SetProperty(ref _isBusy, value);

                _logger.Debug("IsBusy == " + value);
            }
        }

        /// <summary>
        /// Gets or sets a value that represents the if the current user connection state
        /// </summary>
        /// <value>string</value>
        public string ConnectionState
        {
            get { return _connectionState; }
            set { SetProperty(ref _connectionState, value); }
        }

        /// <summary>
        /// Gets or sets a value that represents the current filter value for the search of the servers.
        /// </summary>
        /// <value>the search string</value>
        public string Search
        {
            get { return _search; }
            set { SetProperty(ref _search, value); }
        }

        /// <summary>
        /// Gets the IP addresses associated with the system.
        /// </summary>
        /// <value>The IP addresses associated with the system.</value>
        public SmartCollection<IPAddress> IPAddresses { get; private set; } = new SmartCollection<IPAddress>();

        /// <summary>
        /// Gets or sets the external IP address.
        /// </summary>
        /// <value>The external IP address.</value>
        public string ExternalIPAddress
        {
            get { return _externIP; }
            set { SetProperty(ref _externIP, value); }
        }

        /// <summary>
        /// Gets or sets the start time of the VPN connection lifetime.
        /// </summary>
        /// <value>The start time of the VPN connection.</value>
        public DateTime StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, value); }
        }

        /// <summary>
        /// Gets or sets the end time of the VPN connection lifetime.
        /// </summary>
        /// <value>The end time of the VPN connection.</value>
        public DateTime EndTime
        {
            get
            {
                return _endTime;
            }

            set
            {
                SetProperty(ref _endTime, value);
                OnPropertyChanged("TimeElapsed");
            }
        }

        /// <summary>
        /// Gets the time elapsed of the active VPN connection.
        /// </summary>
        /// <value>The time elapsed of the active VPN connection.</value>
        public TimeSpan TimeElapsed
        {
            get { return EndTime.Subtract(StartTime); }
        }

        /// <summary>
        /// Gets or sets the name of the current location.
        /// </summary>
        /// <value>The name of the current location.</value>
        public string CurrentLocationName
        {
            get
            {
                return _currentLocationName;
            }

            set
            {
                SetProperty(ref _currentLocationName, value);
                _logger.Debug("Currently connected to Location -> " + value);
            }
        }

        /// <summary>
        /// Gets or sets the selected location.
        /// </summary>
        /// <value>The selected location.</value>
        public ILocation SelectedLocation
        {
            get
            {
                return _selServer;
            }

            set
            {
                SetProperty(ref _selServer, value);
                if (value != null)
                {
                    Properties.Settings.Default.LastSelectedServer = value.Id;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged("SelectedLocation");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application is hidden on startup.
        /// </summary>
        /// <value><c>true</c> if the application hides on startup otherwise, <c>false</c>.</value>
        public bool HideApplicationOnStartup
        {
            get
            {
                return Properties.Settings.Default.HideApplicationOnStartup;
            }

            set
            {
                if (Properties.Settings.Default.HideApplicationOnStartup == value)
                {
                    return;
                }

                Properties.Settings.Default.HideApplicationOnStartup = value;
                Properties.Settings.Default.ShowHideNotification = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application should show a close notification.
        /// </summary>
        /// <value><c>true</c> if the application shows a close notification; otherwise, <c>false</c>.</value>
        public bool ShowCloseNotification
        {
            get
            {
                return Properties.Settings.Default.ShowCloseNotification;
            }

            set
            {
                if (Properties.Settings.Default.ShowCloseNotification == value)
                {
                    return;
                }

                Properties.Settings.Default.ShowCloseNotification = value;
                Properties.Settings.Default.ShowCloseNotification = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the close button hides the application.
        /// </summary>
        /// <value><c>true</c> if the close button hides the application; otherwise, <c>false</c>.</value>
        public bool CloseButtonHidesApplication
        {
            get
            {
                return Properties.Settings.Default.CloseButtonHidesApplication;
            }

            set
            {
                // if the user is setting the Hide the application we make sure they get a notification.
                // From then on, it will not be shown until the turn this off and back on
                if (Properties.Settings.Default.CloseButtonHidesApplication != value)
                {
                    ShowCloseNotification = value;
                }

                Properties.Settings.Default.CloseButtonHidesApplication = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application launches on OS startup.
        /// </summary>
        /// <value><c>true</c> if application launches on startup; otherwise, <c>false</c>.</value>
        public bool StartOnStartup
        {
            get
            {
                return _startOnStartup;
            }

            set
            {
                if (value == _startOnStartup)
                {
                    return;
                }

                SetProperty(ref _startOnStartup, value);

                // if this is the set during startup, dont create or delete the task
                if (_checkStartupTaskOnInit)
                {
                    _checkStartupTaskOnInit = false;
                    return;
                }

                if (value)
                {
                    AddStartupTask();
                }
                else
                {
                    RemoveStartupTask();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether automatic VPN reconnect is enabled.
        /// </summary>
        /// <value><c>true</c> if automatic reconnect is enabled; otherwise, <c>false</c>.</value>
        public bool AutoReconnect
        {
            get
            {
                return Properties.Settings.Default.ReconnectOnDisconnect;
            }

            set
            {
                Properties.Settings.Default.ReconnectOnDisconnect = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether kill switch (no internet connectivity if VPN isn't connected) is enabled.
        /// </summary>
        /// <value><c>true</c> if kill switch is enabled; otherwise, <c>false</c>.</value>
        public bool KillSwitch
        {
            get
            {
                return _killSwitch;
            }

            set
            {
                if (_manager != null)
                {
                    _manager.AllowOnlyVPNConnectivity = value;
                    _manager.AllowLANTraffic = value;
                }

                Properties.Settings.Default.KillSwitch = value;
                Properties.Settings.Default.Save();
                SetProperty(ref _killSwitch, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to connect on startup.
        /// </summary>
        /// <value><c>true</c> if connect on startup is set; otherwise, <c>false</c>.</value>
        public bool ConnectOnStartup
        {
            get
            {
                return Properties.Settings.Default.ConnectOnStartup;
            }

            set
            {
                Properties.Settings.Default.ConnectOnStartup = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can connect.
        /// </summary>
        /// <value><c>true</c> if this instance can connect; otherwise, <c>false</c>.</value>
        public bool CanConnect
        {
            get
            {
                return SelectedLocation != null && !IsConnected && IsLoggedIn && !IsConnecting;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can disconnect.
        /// </summary>
        /// <value><c>true</c> if this instance can disconnect; otherwise, <c>false</c>.</value>
        public bool CanDisconnect
        {
            get
            {
                return IsConnected == true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether OpenVPN protocol is available to use.
        /// </summary>
        public bool IsOpenVpnAvailable
        {
            get
            {
                return _manager.GetAvailableVpnTypes().Any(x => x.Key == NetworkConnectionType.OpenVPN && x.Value == true);
            }
        }

        /// <summary>
        /// Gets the name of the application brand.
        /// </summary>
        /// <value>The name of the application brand.</value>
        public string BrandName => Resource.Get<string>("BRAND_NAME");

        /// <summary>
        /// Disconnects the VPN connection and prepares the instance for shut down.
        /// </summary>
        // TODO: Rename this.
        public void ShutItAllDown()
        {
            if (IsConnected)
            {
                Disconnect();
            }
        }

        /// <summary>
        /// OnLocationListRefresh, indicates an error occurred refreshing the list of destinations/locations
        /// </summary>
        /// <param name="message"><see cref="RefreshLocationListMessage"/></param>
        public void OnLocationListRefresh(RefreshLocationListMessage message)
        {
            if (message.Status == RefreshLocationListStatus.Refreshed && IsBusy == true && IsLoggedIn == false)
            {
                SelectedLocation = FindTheLocationByID(Properties.Settings.Default.LastSelectedServer);
                IsLoggedIn = true;
                IsBusy = false;

                // If connect on startup, now is the time.
                if (ConnectOnStartup)
                {
                    Connect();
                }
            }

            if (message.Status == RefreshLocationListStatus.Error)
            {
                if (message.Exception is VpnSDKOAuthException || message.Exception is VpnSDKNotAuthorizedException)
                {
                    ShowErrorDialog(Resources.Strings.ERROR_OCCURED, message.Exception.Message);
                    LogOut();
                }
            }
        }

        /// <summary>
        /// Logs the user in to the WLVPN API using VpnSDK.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns><c>true</c> always.</returns>
        public async Task<bool> LogIn(string username, string password)
        {
            _logger.Debug("LogIn Called");
            IsBusy = true;
            _username = username;
            _password = password;

            _eventAggregator.Publish<BusyTextEvent>(new BusyTextEvent { Text = Resources.Strings.AUTHENTICATING });

            _manager.Login(username, password)
                .Do(e => Debug.WriteLine($"Authentication process: {e}"))
                .StartWith(AuthenticationStatus.Authenticating)
                .SubscribeOn(ThreadPoolScheduler.Instance)
                .Subscribe(
                    o =>
                    {
                        if (o == AuthenticationStatus.Authenticated)
                        {
                            SelectedLocation = FindTheLocationByID(Properties.Settings.Default.LastSelectedServer);

                            // Subscribing to the observable that is responsible for broadcasting server list refresh events.
                            // This tells to SDK to start refreshing server list periodically.
                            _serverListRefresh = _manager.WhenLocationListChanged
                                .SubscribeOn(ThreadPoolScheduler.Instance)
                                .Subscribe(OnLocationListRefresh);
                        }
                    },
                    exception =>
                    {
                        IsBusy = false;
                        IsLoggedIn = false;
                        ShowErrorDialog(Resources.Strings.ERROR_OCCURED, exception.Message);
                    });

            return true;
        }

        /// <summary>
        /// Logs the user out of the API using VpnSDK.
        /// </summary>
        public void LogOut()
        {
            _logger.Debug("LogOut Called");
            _eventAggregator.Publish<BusyTextEvent>(new BusyTextEvent { Text = Resources.Strings.LOGGING_OUT });

            IsBusy = true;

            _manager.Logout()
                .SubscribeOn(ThreadPoolScheduler.Instance)
                .Subscribe(o =>
                {
                    CurrentLocationName = string.Empty;
                    SelectedLocation = null;

                    // we probably want to null out everything else too.
                    Properties.Settings.Default.Username = null;
                    Properties.Settings.Default.Password = null;
                    Properties.Settings.Default.AccessExpiry = DateTime.MinValue;   // TODO: Research correct initial value.
                    Properties.Settings.Default.Save();

                    IsBusy = false;
                    IsConnected = false;
                    IsLoggedIn = false;
                });
        }

        /// <summary>
        /// Connects the VPN connection using the parameters set within the instance.
        /// </summary>
        /// <returns><c>true</c> always.</returns>
        public async Task<bool> Connect()
        {
            _wasConnected = false;

            if (SelectedLocation is IRegion)
            {
                IRegion region = SelectedLocation as IRegion;
                _logger.Info("Connect to " + region.City + ", " + region.Country);
            }
            else if (SelectedLocation is IBestAvailable)
            {
                IBestAvailable bestAvailable = SelectedLocation as IBestAvailable;
                _logger.Info("Connect to " + bestAvailable.SearchName);
            }

            IConnectionConfiguration configuration = null;

            if (ConnectionProtocol == NetworkConnectionType.OpenVPN)
            {
                if (!_manager.TapDriverInstalled)
                {
                    IsBusy = true;
                    _eventAggregator.Publish<BusyTextEvent>(new BusyTextEvent { Text = Resources.Strings.INSTALLING_TAP_DRIVER });
                    if (!await _manager.InstallTapDriver().FirstAsync())
                    {
                        ShowErrorDialog("Tap Driver Installation", "Couldn't install TAP driver.");
                    }

                    IsBusy = false;
                    return false;
                }

                configuration = new OpenVpnConnectionConfigurationBuilder()
                    .SetScramble(OpenVpnScramble)
                    .SetNetworkProtocol(SelectedOpenVpnProtocol)
                    .SetCipher(CipherType)
                    .Build();
            }
            else
            {
                configuration = new RasConnectionConfigurationBuilder()
                                    .SetConnectionType(NetworkConnectionType.IKEv2)
                                    .Build();
            }

            IsConnecting = true;
            IsConnected = false;

            if (SelectedLocation != null)
            {
                _manager.Connect(SelectedLocation, configuration)?
                    .Do(e => Debug.WriteLine($"Connection Process: {e}"))
                    .Subscribe(
                        status =>
                        {
                            switch (status)
                            {
                                case ConnectionStatus.Connecting:
                                    IsConnecting = true;
                                    IsConnected = false;
                                    IsBusy = true;
                                    _eventAggregator.Publish<BusyTextEvent>(new BusyTextEvent { Text = Resources.Strings.CONNECTING });
                                    break;

                                case ConnectionStatus.Connected:
                                    StartTiming();
                                    IsConnecting = false;
                                    IsConnected = true;
                                    IsBusy = false;
                                    _wasConnected = true;
                                    break;

                                default:
                                    IsConnecting = false;
                                    IsConnected = false;
                                    IsBusy = false;
                                    break;
                            }
                        },
                        exception =>
                        {
                            // exceptions will show up here.
                            IsConnecting = false;
                            IsConnected = false;
                            ConnectionState = ConnectionStatus.Failed.ToString();
                            IsBusy = false;

                            if (AutoReconnect && _wasConnected)
                            {
                                RunOnDisplayThread(() =>
                                {
                                    IsConnecting = true;
                                    _eventAggregator.Publish<BusyTextEvent>(new BusyTextEvent { Text = Resources.Strings.CONNECTING });
                                });

                                Task.Factory.StartNew(async () =>
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(Properties.Settings.Default.ReconnectDelay));

                                    RunOnDisplayThread(() =>
                                    {
                                        Connect();
                                    });
                                });
                            }
                            else
                            {
                                ShowErrorDialog(Resources.Strings.ERROR_OCCURED, exception.Message);
                            }
                        });
            }

            return true;
        }

        /// <summary>
        /// Install OpenVPN TAP driver.
        /// </summary>
        public void InstallTapDriver()
        {
            IsBusy = true;
            _manager.InstallTapDriver()
                .Finally(() => { IsBusy = false; })
                .Subscribe(result =>
                {
                    if (result)
                    {
                        ShowErrorDialog("Tap Driver Installation", "TAP driver has been successfully installed.");
                    }
                    else
                    {
                        ShowErrorDialog("Tap Driver Installation", "Couldn't install TAP driver.");
                    }
                });
        }

        /// <summary>
        /// cancel the connection that is currently being made.
        /// </summary>
        public void CancelConnection()
        {
            _manager.CancelConnectionProcess();
        }

        /// <summary>
        /// Disconnects the current VPN connection.
        /// </summary>
        /// <returns><c>true</c> always.</returns>
        public bool Disconnect()
        {
            _wasConnected = false;

            _logger.Debug("Disconnect Called");
            _manager.Disconnect()
                .Do(e => Debug.WriteLine($"Connection Process: {e}"))
                .Subscribe(status =>
                {
                    switch (status)
                    {
                        case ConnectionStatus.Disconnecting:
                            _eventAggregator.Publish<BusyTextEvent>(new BusyTextEvent { Text = Resources.Strings.DISCONNECTING });
                            IsBusy = true;
                            break;

                        case ConnectionStatus.Disconnected:
                            StopTimer();
                            IsBusy = false;
                            IsConnected = false;
                            break;
                    }
                });

            IsConnected = false;
            return true;
        }

        private void ReadStoredSettings()
        {
            var tmp = BrandName;

            HideApplicationOnStartup = Properties.Settings.Default.HideApplicationOnStartup;
            AutoReconnect = Properties.Settings.Default.ReconnectOnDisconnect;
            ConnectOnStartup = Properties.Settings.Default.ConnectOnStartup;
            KillSwitch = Properties.Settings.Default.KillSwitch;
            ShowCloseNotification = Properties.Settings.Default.ShowCloseNotification;

            if (!IsOpenVpnAvailable && ConnectionProtocol != NetworkConnectionType.IKEv2)
            {
                ConnectionProtocol = NetworkConnectionType.IKEv2;
            }

            // if the task exists, that means we were started by it.
            Task.Factory.StartNew(() =>
            {
                using (Microsoft.Win32.TaskScheduler.TaskService tastService = new Microsoft.Win32.TaskScheduler.TaskService())
                {
                    Microsoft.Win32.TaskScheduler.Task serviceTask =
                        tastService.RootFolder.Tasks.FirstOrDefault(x => x.Name == BrandName);

                    if (serviceTask != null && serviceTask.Enabled)
                    {
                        _checkStartupTaskOnInit = true;
                        StartOnStartup = true;
                    }
                }
            });
        }

        private ILocation FindTheLocationByID(string id)
        {
            ILocation location = Locations.FirstOrDefault((loc) => string.Equals(loc.Id, id));
            return location;
        }

        private void RemoveStartupTask()
        {
            try
            {
                using (Microsoft.Win32.TaskScheduler.TaskService tastService = new Microsoft.Win32.TaskScheduler.TaskService())
                {
                    tastService.RootFolder.DeleteTask(BrandName, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Unable to remove the startup task ");
            }
        }

        private void AddStartupTask()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    using (Microsoft.Win32.TaskScheduler.TaskService tastService = new Microsoft.Win32.TaskScheduler.TaskService())
                    {
                        Microsoft.Win32.TaskScheduler.TaskDefinition task = tastService.NewTask();
                        Microsoft.Win32.TaskScheduler.LogonTrigger trigger = new Microsoft.Win32.TaskScheduler.LogonTrigger()
                        {
                            Delay = TimeSpan.FromSeconds(10)
                        };

                        task.Principal.RunLevel = Microsoft.Win32.TaskScheduler.TaskRunLevel.Highest;
                        task.Settings.DisallowStartIfOnBatteries = false;
                        task.Settings.StopIfGoingOnBatteries = false;
                        task.Settings.RunOnlyIfNetworkAvailable = false;  // Always start up regardless of network availability as the application can handle itself when there is no network connection.
                        task.Settings.RunOnlyIfIdle = false;
                        task.Settings.ExecutionTimeLimit = TimeSpan.Zero;
                        task.Triggers.Add(trigger);
                        task.Actions.Add(
                            new ExecAction($"\"{Assembly.GetExecutingAssembly().Location}\"", null, $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}"));
                        task.RegistrationInfo.Description = $"{BrandName} automatic startup.";
                        tastService.RootFolder.RegisterTaskDefinition(BrandName, task);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Unable to add the startup task ");
                }
            });
        }

        private void UpdateTime()
        {
            EndTime = DateTime.Now;
        }

        private void StartTiming()
        {
            StartTime = DateTime.Now;
            EndTime = DateTime.Now;
            _timer.Tick += Timer_Tick;
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTime();
        }

        private void StopTimer()
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
        }

        private void ShowErrorDialog(string title, string errorDescription)
        {
            DialogAction da = new DialogAction
            {
                OKAction = () => { },
                OKString = Resources.Strings.DIALOG_ACTION_OK,
                Title = title,
                Description = errorDescription
            };
            _eventAggregator.Publish<ShowDialogEvent>(new ShowDialogEvent { DialogAction = da, Show = true });
        }

        /// <summary>
        /// The Region Filter is used to filter the set of <see cref="ILocation"/> shown in the destinations list.
        /// </summary>
        /// <param name="searchText">The search query.</param>
        /// <returns>A function that represents the filter.</returns>
        private Func<ILocation, bool> RegionFilter(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return location => true;
            }

            return location =>
            {
                if (location is IBestAvailable bestAvailable)
                {
                    return bestAvailable.SearchName.ToLowerInvariant().Contains(searchText.ToLowerInvariant())
                           || bestAvailable.SearchName.ToLowerInvariant().Contains(searchText.ToLowerInvariant());
                }

                return ((IRegion)location).Country.ToLowerInvariant().Contains(searchText.ToLowerInvariant())
                       || ((IRegion)location).City.ToLowerInvariant().Contains(searchText.ToLowerInvariant());
            };
        }

        private void ConfigureSDK()
        {
            string brandID = Helpers.Resource.Get<string>("BRAND_NAME", "WLSDK");
            brandID = brandID.Replace(" ", string.Empty);

            string logFilesPath = Path.Combine(
                                    Environment.GetFolderPath(
                                    Environment.SpecialFolder.LocalApplicationData,
                                    Environment.SpecialFolderOption.Create),
                                    brandID,
                                    "Logs",
                                    "SDK.log");

            if (string.IsNullOrEmpty(Helpers.Resource.Get<string>("API_KEY")) ||
                string.IsNullOrEmpty(Helpers.Resource.Get<string>("AUTHORIZATION_TOKEN")))
            {
                throw new VpnSDKInvalidConfigurationException("API key or Authorization token was not set.");
            }

            /*
             The values for the API_KEY and AUTHORIZATION_TOKEN for this example app are stored in Resources\Branding\apiaccess.xaml.
             You must replace those placeholder values with a real key and authorization token
             To obtain these values you need to be a registered WLVPN reseller.
             If you have not done so already, please visit https://wlvpn.com/#contact to get started.
             */
            _manager = new SDKBuilder()
                            .SetApiKey(Helpers.Resource.Get<string>("API_KEY"))
                            .SetApplicationName(Helpers.Resource.Get<string>("BRAND_NAME"))
                            .SetAuthenticationToken(Helpers.Resource.Get<string>("AUTHORIZATION_TOKEN"))
                            .SetLogFilesPath(logFilesPath)
                            .SetServerListCache(TimeSpan.FromDays(1))
                            .SetOpenVpnConfiguration(new OpenVpnConfiguration
                            {
                                OpenVpnCertificateFileName = "ca.crt",
                                OpenVpnDirectory = "OpenVPN",
                                OpenVpnExecutableFileName = "openvpn.exe",
                                OpenVpnConfigDirectory = "OpenVPN",
                                OpenVpnConfigFileName = "config.ovpn",
                                OpenVpnLogFileName = "openvpn.log",
                                TapDeviceDescription = "TAP-Windows Adapter V9",
                                TapDeviceName = "tap0901"
                            })
                            .SetRasConfiguration(new RasConfiguration
                            {
                                RasDeviceDescription = Resource.Get<string>("BRAND_NAME")
                            })
                            .Create();

            var filter = this.WhenValueChanged(t => t.Search)
                                .Throttle(TimeSpan.FromMilliseconds(250))
                                .Select(RegionFilter);

            _serverListLoader = _manager.RegionsList.Connect()
                .Filter(x => x is IRegion || x is IBestAvailable)
                .Filter(filter)
                .ObserveOnDispatcher()
                .Bind(Locations)
                .Subscribe();

            _manager.WhenUserLocationChanged.Subscribe(info =>
            {
                switch (info.Status)
                {
                    case PositionInfoStatus.Updating:
                        CurrentLocationName = "Retrieving...";
                        ExternalIPAddress = "Retrieving...";
                        break;

                    case PositionInfoStatus.Updated:
                        RunOnDisplayThread(() =>
                        {
                            CurrentLocationName = $"{info.City}, {info.Country}";
                            ExternalIPAddress = info.IPAddress.ToString();
                            if (IsConnected)
                            {
                                _eventAggregator.Publish<ShowNotificationEvent>(new ShowNotificationEvent
                                {
                                    Title = Resources.Strings.CONNECTED_VISIBLE_LOCATION_HEADER_CHANGED,
                                    Text = string.Format(Resources.Strings.CURRENT_VISIBLE_LOCATION, CurrentLocationName, string.Empty),
                                });
                            }
                        });

                        break;
                }
            });
        }
    }
}