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
        private const int LOG_ITEMS_MAX = 300;
        private const int LOG_ITEMS_MAX_SIZE = 150000;   // 300 lines * 500 bytes per line


        public static Style Icon => Resource.Get<Style>("DiagnosticsIcon");

        public string TabHeaderTitle => Properties.Strings.TabSettingsDiagnostics;

        public List<LogEvent> LogCollection { get; } = new List<LogEvent>();

        public string LogText { get; set; }

        private MessageTemplateTextFormatter formatter =
        new MessageTemplateTextFormatter("{Timestamp:HH:mm:ss} [{Level}] ({SourceContext}) {Message}{NewLine}{Exception}", CultureInfo.InvariantCulture);

        private StringBuilder sb = new StringBuilder();

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

            sb.Clear();

            using (StringWriter buffer = new StringWriter(sb))
            {
                foreach (LogEvent le in LogCollection)
                {
                    if (sb.Length >= LOG_ITEMS_MAX_SIZE) break;
                    formatter.Format(le, buffer);
                    buffer.Flush();
                }
                buffer.Flush();
            }

            LogText = sb.ToString();
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