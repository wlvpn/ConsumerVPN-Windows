// <copyright file="MainWindow.xaml.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using MahApps.Metro.Controls;
using NetSparkle;
using NetSparkle.Enums;
using VpnSDK.WLVpn.Events;
using VpnSDK.WLVpn.Utilities;
using VpnSDK.WLVpn.ViewModels;

namespace VpnSDK.WLVpn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private IDisposable _notificationSubscription = null;
        private IDisposable _showMainViewSubscription = null;
        private Sparkle _sparkleUpdater;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="sdk">The SDK.</param>
        public MainWindow(EventAggregator eventAggregator, SDKMonitor sdk)
        {
            SdkMonitor = sdk;
            Aggregator = eventAggregator;
            InitializeComponent();

            // the following line is to allow ElementName bindings in the Context menu to resolve properly
            // Context menu's are not in the Visual Tree.
            NameScope.SetNameScope(contextMenu, NameScope.GetNameScope(this));

            _notificationSubscription = eventAggregator.GetEvent<ShowNotificationEvent>().Subscribe((msg) =>
            {
                RunOnDisplayThread(() =>
                {
                    MainViewModel mvm = DataContext as MainViewModel;
                    if (mvm != null && mvm.IsShuttingDown)
                    {
                        return;
                    }
                });

                TaskbarIconControl.ShowBalloonTip(msg.Title, msg.Text, msg.Icon);
            });

            _showMainViewSubscription = eventAggregator.GetEvent<ShowViewEvent>().Subscribe((msg) =>
            {
                if (msg.ID == Common.ViewList.Views.MainWindow)
                {
                    RunOnDisplayThread(() =>
                    {
                        if (msg.Show)
                        {
                            Visibility = Visibility.Visible;
                            WindowState = WindowState.Normal;
                            BringIntoView();
                            Activate();
                        }
                        else
                        {
                            Visibility = Visibility.Hidden;
                            WindowState = WindowState.Normal;
                        }
                    });
                }
            });

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _sparkleUpdater = new Sparkle(
                Helpers.Resource.Get<string>("BRAND_SPARKLE_URL"),
                System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location),
                SecurityMode.Unsafe)
            {
                ShowsUIOnMainThread = true,
                RelaunchAfterUpdate = true,
                UIFactory = new SparkleUIFactory()
            };

            _sparkleUpdater.StartLoop(true, true, TimeSpan.FromHours(2));
            Loaded -= OnLoaded;
        }

        /// <summary>
        /// Gets or sets the SDK monitor.
        /// </summary>
        /// <value>The SDK monitor.</value>
        public SDKMonitor SdkMonitor { get; set; }

        private EventAggregator Aggregator { get; set; }

        /// <summary>
        /// Handles the <see cref="E:Closing" /> event.
        /// </summary>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (SdkMonitor.CloseButtonHidesApplication)
            {
                e.Cancel = true;
                Visibility = Visibility.Hidden;
                WindowState = WindowState.Normal;
                if (SdkMonitor.ShowCloseNotification)
                {
                    Aggregator.Publish<ShowNotificationEvent>(new ShowNotificationEvent { Title = WLVpn.Resources.Strings.SETTINGS_CLOSING_OPTION2, Text = WLVpn.Resources.Strings.SETTINGS_SYSTEM_STARTUP_OPTION2 });
                    SdkMonitor.ShowCloseNotification = false;
                }
            }

            base.OnClosing(e);
        }

        /// <summary>
        /// Runs the provided action on the UI thread.
        /// </summary>
        /// <param name="actionToExecute">The action to execute.</param>
        protected void RunOnDisplayThread(Action actionToExecute)
        {
            var dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;

            if (dispatcher != null && dispatcher.CheckAccess() == false)
            {
                dispatcher.BeginInvoke(actionToExecute);
            }
            else
            {
                actionToExecute();
            }
        }
    }
}