// <copyright file="SettingsLogViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using VpnSDK.WLVpn.Helpers;
using VpnSDK.WLVpn.Views;

namespace VpnSDK.WLVpn.ViewModels
{
    /// <summary>
    /// Class SettingsLogViewModel. Provides the View Model for <see cref="SettingsLogView"/>.
    /// </summary>
    /// <seealso cref="BindableBase" />
    /// <seealso cref="ISettingsViewModel" />
    public class SettingsLogViewModel : BindableBase, ISettingsViewModel
    {
        private string _logLines = string.Empty;
        private RelayCommand _logUpdateCmd;
        private string _vpnSDKLogLines = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsLogViewModel"/> class.
        /// </summary>
        public SettingsLogViewModel()
        {
            if (IsInDesignMode())
            {
                Init();
                return;
            }
        }

        /// <summary>
        /// Gets or sets the logs shown to the user from the application log.
        /// </summary>
        public string LogFileLines
        {
            get { return _logLines; }
            set { SetProperty(ref _logLines, value); }
        }

        /// <summary>
        /// Gets the command reponsible for updating logs.
        /// </summary>
        /// <value>The update logs command.</value>
        public RelayCommand UpdateLogsCmd
        {
            get
            {
                if (_logUpdateCmd == null)
                {
                    _logUpdateCmd = new RelayCommand(
                        (parm) =>
                        {
                            UpdateLogs();
                        },
                        (parm) => true);
                }

                return _logUpdateCmd;
            }
        }

        /// <summary>
        /// Gets or sets the logs shown to the user from the VpnSDK log.
        /// </summary>
        public string VpnSDKLogFileLines
        {
            get { return _vpnSDKLogLines; }
            set { SetProperty(ref _vpnSDKLogLines, value); }
        }

        /// <summary>
        /// Cancels the current dialog.
        /// </summary>
        /// <returns>true</returns>
        public bool Cancel()
        {
            return true;
        }

        /// <summary>
        /// Saves the current dialog.
        /// </summary>
        /// <returns>true</returns>
        public bool Save()
        {
            return true;
        }

        /// <summary>
        /// Updates the logs currently displayed to the user based off the SDK log file as well as the internal log file.
        /// </summary>
        public void UpdateLogs()
        {
            string appLogFile = Helpers.Resource.Get<string>("BRAND_LOGFILE_NAME", "application.log");
            appLogFile = appLogFile.Replace(" ", string.Empty);

            string vpnSDKLogFile = "SDK.log";

            appLogFile = appLogFile.Replace(" ", string.Empty);

            string brandID = Helpers.Resource.Get<string>("BRAND_NAME", "WLSDK");
            brandID = brandID.Replace(" ", string.Empty);

            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
            string logFile = Path.Combine(appdata, brandID, "Logs", appLogFile);
            string vpnSDKlogFile = Path.Combine(appdata, brandID, "Logs", vpnSDKLogFile);

            try
            {
                string lines = string.Join(Environment.NewLine, File.ReadLines(logFile).Reverse().Take(1000).Reverse().ToList());

                // filter out the json for the screen
                lines = lines.Replace("}", string.Empty);
                lines = lines.Replace("{", string.Empty);
                lines = lines.Replace("\"", string.Empty);
                lines = lines.Replace("timestamp:", string.Empty);
                lines = lines.Replace("level:", string.Empty);
                lines = lines.Replace("logClass:", string.Empty);
                lines = lines.Replace("message:", string.Empty);

                LogFileLines = lines;
            }
            catch (Exception ex)
            {
                // no log file defined......
                System.Console.WriteLine(ex);
            }

            try
            {
                string lines = string.Join(Environment.NewLine, File.ReadLines(vpnSDKlogFile).Reverse().Take(1000).Reverse().ToList());

                // filter out the json for the screen
                lines = lines.Replace("}", string.Empty);
                lines = lines.Replace("{", string.Empty);
                lines = lines.Replace("\"", string.Empty);
                lines = lines.Replace("timestamp:", string.Empty);
                lines = lines.Replace("level:", string.Empty);
                lines = lines.Replace("logClass:", string.Empty);
                lines = lines.Replace("message:", string.Empty);

                VpnSDKLogFileLines = lines;
            }
            catch (Exception ex)
            {
                // no log file defined......
                System.Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// Initialize any data needed at design time to help the Visual Studio designer display data.
        /// </summary>
        private void Init()
        {
            LogFileLines = "The Log Data will go here.";
        }
    }
}