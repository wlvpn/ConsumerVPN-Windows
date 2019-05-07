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

        public App()
        {
            // Forces all WPF related modules to load to ensure library localizations don't cause a recursive cycle.
            // This ensures HockeyApp and other libraries are able to access the resource manager when IL merging is used.
            ClrBugUtility.InvokeExceptionAndReturnTrue();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            if (IsAnotherInstance())
            {
                BringOtherInstanceToForeground();
                Log.Information("Another instance of ConsumerVPN is running on this machine, exiting this instance");
                Environment.Exit(1152);
            }

            base.OnStartup(e);

            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            this.ShutdownMode = ShutdownMode.OnLastWindowClose;
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
            Log.Information("=== Exiting ConsumerVPN ===");
            ClearMutext();
            base.OnExit(e);
        }

        private bool IsAnotherInstance()
        {
            _appMutex = new Mutex(true, "Local\\" + "ConsumerVPN", out bool created);
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