// <copyright file="App.xaml.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using Microsoft.Shell;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using VpnSDK.WLVpn.Events;
using VpnSDK.WLVpn.Helpers;
using VpnSDK.WLVpn.Interfaces;
using VpnSDK.WLVpn.Properties;
using VpnSDK.WLVpn.ViewModels;

namespace VpnSDK.WLVpn
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private static Logger _logger = LogManager.GetLogger("WL::App");
        private DependencyObject _dummy = new DependencyObject();

        /// <summary>
        /// Gets the DI container instance.
        /// </summary>
        /// <value>The DI container instance.</value>
        public static SimpleInjector.Container ContainerInstance { get; } = new SimpleInjector.Container();

        /// <summary>
        /// Raises and handles the <see cref="E:System.Windows.Application.Exit" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.ExitEventArgs" /> that contains the event data.</param>
        protected override void OnExit(ExitEventArgs e)
        {
            _logger.Info("----------  Exiting Application ");
            base.OnExit(e);
        }

        /// <summary>
        /// Executes an action on the UI thread.
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

        /// <summary>
        /// Determines whether the view is in design mode.
        /// </summary>
        /// <returns><c>true</c> if view is in design mode; otherwise, <c>false</c>.</returns>
        protected bool IsInDesignMode()
        {
            return DesignerProperties.GetIsInDesignMode(_dummy);
        }

        /// <summary>
        /// Raises and handles the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            if (!SingleInstance<App>.InitializeAsFirstInstance($"{Assembly.GetExecutingAssembly().GetName().Name.Replace(" ", "-").ToUpper()}"))
            {
                Environment.Exit(0);
                return;
            }

            ConfigureLogger();
            ConfigureContainer();
            _logger.Info("++++++++++  Starting Application ");
            _logger.Info($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");

            base.OnStartup(e);

            // Configure HockeySDK client
#if !DEBUG
            try
            {
                HockeyClient.Current.Configure(Helpers.Resource.Get<string>("HOCKEYAPP_APP_ID", string.Empty));
                await HockeyClient.Current.SendCrashesAsync(true);
            }
            catch
            {
                // Ignore
            }
#endif

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            var mainWindows = ContainerInstance.GetInstance<MainWindow>();

            SDKMonitor sdkmonitor = ContainerInstance.GetInstance<SDKMonitor>();
            EventAggregator aggregator = ContainerInstance.GetInstance<EventAggregator>();

            if (Settings.Default.CallUpgrade)
            {
                // put any first run logic in here.
                Settings.Default.ShowHideNotification = true;
                Settings.Default.CallUpgrade = false;
                Settings.Default.Save();
            }

            if (sdkmonitor.HideApplicationOnStartup)
            {
                await Task.Run(() =>
                {
                    RunOnDisplayThread(() =>
                    {
                        MainWindow = mainWindows;
                        aggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.MainWindow, Show = false });
                        if (Settings.Default.ShowHideNotification)
                        {
                            aggregator.Publish<ShowNotificationEvent>(new ShowNotificationEvent
                            {
                                Title = WLVpn.Resources.Strings.SETTINGS_APPLICATION_STARTUP,
                                Text = WLVpn.Resources.Strings.SETTINGS_SYSTEM_STARTUP_OPTION2
                            });
                            Settings.Default.ShowHideNotification = false;
                            Settings.Default.Save();
                        }
                    });
                });
            }
            else
            {
                mainWindows.Show();
            }
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            if (Current?.MainWindow != null)
            {
                Current.MainWindow.Activate();
                Current.MainWindow.Visibility = Visibility.Visible;
                Current.MainWindow.Show();
                Current.MainWindow.WindowState = WindowState.Normal;
            }
            return true;
        }

        private static void ConfigureLogger()
        {
            string appLogFile = Helpers.Resource.Get<string>("BRAND_LOGFILE_NAME", "application.log");
            appLogFile = appLogFile.Replace(" ", string.Empty);

            string brandID = Helpers.Resource.Get<string>("BRAND_NAME", "WLSDK");
            brandID = brandID.Replace(" ", string.Empty);

            LoggingConfiguration loggingConfiguration = LogManager.Configuration ?? new LoggingConfiguration();

            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
            string logLocation = Path.Combine(appdata, brandID, "Logs", appLogFile);

            FileTarget fileTarget = new FileTarget
            {
                FileName = logLocation,
                LineEnding = LineEndingMode.CRLF,
                ArchiveEvery = FileArchivePeriod.Day,
                Header = $"================= {((AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute), false)).Title} =================",
                MaxArchiveFiles = 2
            };
            var jsonLayout = new JsonLayout
            {
                Attributes =
                {
                    new JsonAttribute("timestamp", @"${date:format=yyyy\-MM\-dd\THH\:mm\:ss\.fffK}"),
                    new JsonAttribute("level", @"${level:uppercase=true}"),
                    new JsonAttribute("logClass", @"${logger:shortName=True}"),
                    new JsonAttribute("message", @"${message}"),
                    new JsonAttribute("exception", @"${exception:format = toString,Data: maxInnerExceptionLevel = 10}"),
                }
            };

            fileTarget.Layout = jsonLayout;

            loggingConfiguration.AddTarget(brandID + "_filelog", fileTarget);
            loggingConfiguration.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget, "WL::*");

            DebuggerTarget debugTarget = new DebuggerTarget
            {
                Layout = @"${date:format=HH\:mm\:ss\.fff} - [${logger:shortName=True}] - ${message}"
            };

            loggingConfiguration.AddTarget(brandID + "_debuglog", debugTarget);
            loggingConfiguration.AddRule(LogLevel.Debug, LogLevel.Fatal, debugTarget, "WL::*");

            LogManager.Configuration = loggingConfiguration;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Fatal((Exception)e.ExceptionObject, "Unhandled exception bubbled to App.Xaml.cs");
        }

        private void ConfigureContainer()
        {
            ContainerInstance.Register<EventAggregator>(Lifestyle.Singleton);
            /* If you wish to have custom pre-authentication, register a custom authenticator here. */
            ContainerInstance.Register<ICustomAuthenticator, DebugCustomAuthenticator>(Lifestyle.Singleton);
            ContainerInstance.Register<SDKMonitor>(Lifestyle.Singleton);
            ContainerInstance.Verify();
        }
    }
}