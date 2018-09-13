// <copyright file="MainViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using VpnSDK.WLVpn.Common;
using VpnSDK.WLVpn.Events;
using VpnSDK.WLVpn.Helpers;
using VpnSDK.WLVpn.Views;

namespace VpnSDK.WLVpn.ViewModels
{
    /// <summary>
    /// Class MainViewModel. Represents the view model for <see cref="MainWindow"/>.
    /// </summary>
    /// <seealso cref="BindableBase" />
    public class MainViewModel : BindableBase
    {
        private EventAggregator _eventAggregator;
        private readonly IDisposable _registerViewSubscription = null;
        private readonly IDisposable _showViewSubscription = null;
        private readonly IDisposable _showDialogSubscription = null;
        private RelayCommand _exitCmd = null;
        private ViewDefinition _currentView = null;
        private BasicDialogView _basicDialog = null;
        private ViewDefinition _currentDialog = null;
        private bool _showDialog = false;
        private RelayCommand _settingsCmd = null;
        private RelayCommand _logOutCmd = null;
        private RelayCommand _connectCmd = null;
        private RelayCommand _disconnectCmd = null;
        private RelayCommand _showMainViewtCmd = null;
        private RelayCommand _showDestinationsCmd = null;
        private RelayCommand _hideMainViewCmd = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="sdkMonitor">The SDK monitor.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        public MainViewModel(SDKMonitor sdkMonitor, EventAggregator eventAggregator)
        {
            if (IsInDesignMode())
            {
                Init();
                return;
            }

            _eventAggregator = eventAggregator;

            InitViews();

            _showViewSubscription = _eventAggregator.GetEvent<ShowViewEvent>().Subscribe((evt) =>
            {
                RunOnDisplayThread(() =>
                {
                    NavigationLogic(evt);
                });
            });

            _showDialogSubscription = _eventAggregator.GetEvent<ShowDialogEvent>().Subscribe((evt) =>
            {
                RunOnDisplayThread(() =>
                {
                    ShowDialogLogic(evt);
                });
            });

            SDKMonitor = sdkMonitor;

            if (!SDKMonitor.IsLoggedIn)
            {
                _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = ViewList.Views.Login });
            }
            else if (!SDKMonitor.ConnectOnStartup)
            {
                _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.Disconnected });
            }
        }

        /// <summary>
        /// Gets or sets the taskbar icon source.
        /// </summary>
        /// <value>The taskbar icon source.</value>
        public string TaskbarIconSource { get; set; } = Helpers.Resource.Get<string>("BRAND_ICON", @"pack://application:,,,/WLVpn;component/Resources/Branding/Assets/brandIcon.ico");

        /// <summary>
        /// Gets or sets a value indicating whether this instance is shutting down.
        /// </summary>
        /// <value><c>true</c> if this instance is shutting down; otherwise, <c>false</c>.</value>
        public bool IsShuttingDown { get; set; }

        /// <summary>
        /// Gets or sets the SDK monitor.
        /// </summary>
        /// <value>The SDK monitor.</value>
        public SDKMonitor SDKMonitor { get; set; }

        /// <summary>
        /// Gets or sets the current view.
        /// </summary>
        /// <value>The current view.</value>
        public ViewDefinition CurrentView
        {
            get { return _currentView; }
            set { SetProperty(ref _currentView, value); }
        }

        /// <summary>
        /// Gets or sets the current dialog.
        /// </summary>
        /// <value>The current dialog.</value>
        public ViewDefinition CurrentDialog
        {
            get { return _currentDialog; }
            set { SetProperty(ref _currentDialog, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a dialog is showing.
        /// </summary>
        /// <value><c>true</c> if a dialog is showing; otherwise, <c>false</c>.</value>
        public bool ShowDialog
        {
            get { return _showDialog; }
            set { SetProperty(ref _showDialog, value); }
        }

        /// <summary>
        /// Gets the show settings command.
        /// </summary>
        /// <value>The show settings command.</value>
        public RelayCommand SettingsCmd
        {
            get
            {
                if (_settingsCmd == null)
                {
                    _settingsCmd = new RelayCommand(
                        (parm) =>
                        {
                            // if settings is currently shown, hide it.
                            if (CurrentView?.ID == ViewList.Views.Settings)
                            {
                                _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.Back });
                            }
                            else
                            {
                                _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.Settings });
                                _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.MainWindow });
                            }
                        },
                        (parm) => SDKMonitor != null && !SDKMonitor.IsConnected && SDKMonitor.IsLoggedIn);
                }

                return _settingsCmd;
            }
        }

        /// <summary>
        /// Gets the log out command.
        /// </summary>
        /// <value>The log out command.</value>
        public RelayCommand LogOutCmd
        {
            get
            {
                if (_logOutCmd == null)
                {
                    _logOutCmd = new RelayCommand(
                        (parm) =>
                        {
                            DialogAction da = new DialogAction
                            {
                                OKAction = () => LogOut(),
                                OKString = Resources.Strings.DIALOG_ACTION_OK,
                                CancelString = Resources.Strings.DIALOG_ACTION_CANCEL,
                                CancelAction = () => { },
                                Title = Resources.Strings.DIALOG_ACTION_TITLE,
                                Description = Resources.Strings.DIALOG_ACTION_DESCRIPTION
                            };
                            _eventAggregator.Publish<ShowDialogEvent>(new ShowDialogEvent { DialogAction = da, Show = true });
                        },
                        (parm) => SDKMonitor != null && SDKMonitor.IsLoggedIn);
                }

                return _logOutCmd;
            }
        }

        /// <summary>
        /// Gets the connect command.
        /// </summary>
        /// <value>The connect command.</value>
        public RelayCommand ConnectCmd
        {
            get
            {
                if (_connectCmd == null)
                {
                    _connectCmd = new RelayCommand(
                        (parm) =>
                        {
                            SDKMonitor.Connect();
                        },
                        (parm) => SDKMonitor != null && SDKMonitor.CanConnect);
                }

                return _connectCmd;
            }
        }

        /// <summary>
        /// Gets the disconnect command.
        /// </summary>
        /// <value>The disconnect command.</value>
        public RelayCommand DisconnectCmd
        {
            get
            {
                if (_disconnectCmd == null)
                {
                    _disconnectCmd = new RelayCommand(
                        (parm) =>
                        {
                            SDKMonitor.Disconnect();
                        },
                        (parm) => SDKMonitor != null && SDKMonitor.CanDisconnect);
                }

                return _disconnectCmd;
            }
        }

        /// <summary>
        /// Gets the show main view command.
        /// </summary>
        /// <value>The show main view command.</value>
        public RelayCommand ShowMainViewCmd
        {
            get
            {
                if (_showMainViewtCmd == null)
                {
                    _showMainViewtCmd = new RelayCommand(
                        (parm) =>
                        {
                            _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.MainWindow });
                        },
                        (parm) => true);
                }

                return _showMainViewtCmd;
            }
        }

        /// <summary>
        /// Gets the show destinations command.
        /// </summary>
        /// <value>The show destinations command.</value>
        public RelayCommand ShowDestinationsCmd
        {
            get
            {
                if (_showDestinationsCmd == null)
                {
                    _showDestinationsCmd = new RelayCommand(
                        (parm) =>
                        {
                            _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.ServerList });
                            _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.MainWindow });
                        },
                        (parm) =>
                        SDKMonitor != null && !SDKMonitor.IsConnected && SDKMonitor.IsLoggedIn);
                }

                return _showDestinationsCmd;
            }
        }

        /// <summary>
        /// Gets the hide main view command.
        /// </summary>
        /// <value>The hide main view command.</value>
        public RelayCommand HideMainViewCmd
        {
            get
            {
                if (_hideMainViewCmd == null)
                {
                    _hideMainViewCmd = new RelayCommand(
                        (parm) =>
                        {
                            _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.MainWindow, Show = false });
                        },
                        (parm) => Application.Current.MainWindow.Visibility == Visibility.Visible);
                }

                return _hideMainViewCmd;
            }
        }

        /// <summary>
        /// Gets the exit application command.
        /// </summary>
        /// <value>The exit application command.</value>
        public RelayCommand ExitCmd
        {
            get
            {
                if (_exitCmd == null)
                {
                    _exitCmd = new RelayCommand(
                        (parm) =>
                        {
                            // need to disconnect, etc.  so more to figure out what to do here.
                            IsShuttingDown = true;
                            SDKMonitor.ShutItAllDown();
                            Application.Current.Shutdown();
                        },
                        (parm) => true);
                }

                return _exitCmd;
            }
        }

        private Stack<ViewDefinition> PreviousView { get; set; } = new Stack<ViewDefinition>();

        private Dictionary<ViewList.Views, ViewDefinition> AvalableViews { get; set; } = new Dictionary<ViewList.Views, ViewDefinition>();

        private void Init()
        {
            ShowDialog = false;
        }

        private void InitViews()
        {
            Container ioc = App.ContainerInstance;
            RunOnDisplayThread(() =>
            {
                AvalableViews.Add(ViewList.Views.Disconnected, new ViewDefinition { ID = ViewList.Views.Disconnected, Title = "Disconnected", View = ioc.GetInstance<DisconnectedView>() });
                AvalableViews.Add(ViewList.Views.Connected, new ViewDefinition { ID = ViewList.Views.Connected, Title = "Connected", View = ioc.GetInstance<ConnectedView>() });
                AvalableViews.Add(ViewList.Views.Login, new ViewDefinition { ID = ViewList.Views.Login, Title = "Login", View = ioc.GetInstance<LoginView>() });
                AvalableViews.Add(ViewList.Views.ServerList, new ViewDefinition { ID = ViewList.Views.ServerList, Title = "ServerList", View = ioc.GetInstance<LocationListView>(), IsOverLay = true });
                AvalableViews.Add(ViewList.Views.Settings, new ViewDefinition { ID = ViewList.Views.Settings, Title = "Settings", View = ioc.GetInstance<SettingsView>(), IsOverLay = true });
            });
        }

        private void NavigationLogic(ShowViewEvent evt)
        {
            if (CurrentView != null && CurrentView.ID == evt.ID)
            {
                return;
            }

            if (evt.ID == ViewList.Views.Back && PreviousView != null)
            {
                if (PreviousView.Count() > 0)
                {
                    CurrentView = PreviousView.Pop();
                    if (CurrentView != null && !CurrentView.IsOverLay)
                    {
                        PreviousView.Clear();
                    }
                }

                return;
            }

            if (AvalableViews.ContainsKey(evt.ID))
            {
                PreviousView.Push(CurrentView);
                CurrentView = AvalableViews[evt.ID];
                if (!CurrentView.IsOverLay)
                {
                    PreviousView.Clear();
                }
            }

            FocusManager.SetIsFocusScope((DependencyObject)CurrentView.View, true);

            CommandManager.InvalidateRequerySuggested();
        }

        private void ShowDialogLogic(ShowDialogEvent evt)
        {
            // init the dialog on first use.
            if (_basicDialog == null)
            {
                _basicDialog = App.ContainerInstance.GetInstance<BasicDialogView>();
                CurrentDialog = new ViewDefinition { ID = ViewList.Views.ShowDialog, IsOverLay = true, View = _basicDialog };
            }

            if (evt.Show)
            {
                _basicDialog.SetDialogAction(evt.DialogAction);
                ShowDialog = true;
            }
            else
            {
                _basicDialog.SetDialogAction(null);
                ShowDialog = false;
            }

            _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.MainWindow });
        }

        private void LogOut()
        {
            SDKMonitor.LogOut();

            RunOnDisplayThread(
                () =>
                {
                    _eventAggregator.Publish<LogOutEvent>(new LogOutEvent());
                    CommandManager.InvalidateRequerySuggested();
                });
        }
    }
}