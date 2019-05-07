using Caliburn.Micro;
using WLVPN.Interfaces;
using WLVPN.Utils;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Display;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WLVPN.Helpers;

namespace WLVPN.ViewModels
{
    public class DiagnosticsViewModel : Screen, IInformationTabItem
    {
        private const int LOG_ITEMS_MAX = 500;

        public static Style Icon => Resource.Get<Style>("DiagnosticsIcon");

        public string TabHeaderTitle => Properties.Strings.TabSettingsDiagnostics;

        public List<LogEvent> LogCollection { get; } = new List<LogEvent>();

        public string LogText { get; set; }

        public DiagnosticsViewModel()
        {
            AppBootstrapper.LogSubject.Subscribe(OnLogEvent);
        }

        //
        // Summary:
        //     Called when activating.
        protected override void OnActivate()
        {

        }

        //
        // Summary:
        //     Called when deactivating.
        //
        // Parameters:
        //   close:
        //     Inidicates whether this instance will be closed.
        protected override void OnDeactivate(bool close)
        {

        }

        private void OnLogEvent(LogEvent e)
        {
            if (LogCollection.Count >= LOG_ITEMS_MAX)
            {
                LogCollection.RemoveAt(0);
            }

            LogCollection.Add(e);

            MessageTemplateTextFormatter formatter =
                new MessageTemplateTextFormatter("{Timestamp:HH:mm} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}", CultureInfo.InvariantCulture);

            LogText = string.Join("", LogCollection.Select(x =>
            {
                var buffer = new StringWriter(new StringBuilder(256));
                formatter.Format(x, buffer);
                return buffer.ToString();
            }
            ));
        }

        public static void OpenDiagnosticsFolder()
        {
            if (Directory.Exists(AppBootstrapper.LogFileFolder))
            {
                Process.Start("explorer.exe", AppBootstrapper.LogFileFolder);
            }
        }

        /// <summary>
        /// Copies the log text to the clip board.
        /// </summary>
        public void CopyLogs()
        {
            string lockedProcess = null;
            try
            {
                ClipboardHelper.Set(LogText, out lockedProcess);
            }
            catch (Exception)
            {
                if (!string.IsNullOrEmpty(lockedProcess))
                {
                    Log.Warning($"Unable to copy to clipboard, clipboard was locked by process: {lockedProcess}");
                }
                else
                {
                    Log.Warning("Unable to copy to clipboard");
                }
            }
        }
    }
}