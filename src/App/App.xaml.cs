using Serilog;
using System;
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
    public partial class App : Application
    {
        private static Mutex _appMutex = null;

        // Forces all WPF related modules to load to ensure library localizations don't cause a recursive cycle.
        // This ensures HockeyApp and other libraries are able to access the resource manager when IL merging is used.
        private static bool ForceLoadedModule = ClrBugUtility.InvokeExceptionAndReturnTrue();

        public App()
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            if (IsAnotherInstance())
            {
                BringOtherInstanceToForeground();
                Environment.Exit(1152);
            }
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            this.ShutdownMode = ShutdownMode.OnLastWindowClose;
        }

        protected override void OnStartup(StartupEventArgs e)
        {

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
            ClearMutext();
            base.OnExit(e);
        }

        private bool IsAnotherInstance()
        {
            _appMutex = new Mutex(true, $"Local\\{AppBootstrapper.AssemblyName}", out bool created);
            return !created;
        }

        private void ClearMutext()
        {
            if (_appMutex == null) return;
            _appMutex.ReleaseMutex();
            _appMutex.Dispose();
            _appMutex = null;
        }

        private const int SW_RESTORE = 9;

        private void BringOtherInstanceToForeground()
        {
            using (Process currentProcess = Process.GetCurrentProcess())
            {
                string appName = currentProcess.ProcessName;
                Process[] otherInstances = Process.GetProcessesByName(appName);
                if (otherInstances != null && otherInstances.Length > 1)
                {
                    foreach (var process in otherInstances)
                    {
                        if (process.Id != currentProcess.Id)
                        {
                            var handle = process.MainWindowHandle;
                            if (NativeMethods.IsIconic(handle))
                            {
                                NativeMethods.ShowWindow(handle, SW_RESTORE);
                            }
                            NativeMethods.SetForegroundWindow(handle);
                            process.Dispose();
                            return;
                        }
                    }
                }
            }
        }
    }
}