using Microsoft.Shell;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using WLVPN.Utils;

namespace WLVPN
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        // Forces all WPF related modules to load to ensure library localizations don't cause a recursive cycle.
        // This ensures HockeyApp and other libraries are able to access the resource manager when IL merging is used.
        private static bool ForceLoadedModule = ClrBugUtility.InvokeExceptionAndReturnTrue();

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(AppBootstrapper.AssemblyName))
            {
                App application = new App();
                application.InitializeComponent();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;

            // NOTE:  This code is run after the AppBootstrapper.cs code.  It is NOT the first thing run!
            // Caliburn ....
            base.OnStartup(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is DynamicData.SortException || e.Exception is DynamicData.UnspecifiedIndexException)
            {
                Log.Warning(e.Exception, "Failed to handle server list sorting.");
                e.Handled = true;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information($"=== Exiting {AppBootstrapper.AssemblyName} ===");
            base.OnExit(e);
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            if (Current != null)
            {
                if (Current.MainWindow != null)
                {
                    Current.MainWindow.Activate();
                    Current.MainWindow.Visibility = Visibility.Visible;
                    Current.MainWindow.Show();
                    Current.MainWindow.WindowState = WindowState.Normal;
                }
            }
            return true;
        }
    }
}