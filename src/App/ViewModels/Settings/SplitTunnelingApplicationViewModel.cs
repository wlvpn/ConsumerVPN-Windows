using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Forms;
using VpnSDK.Common.Settings;
using VpnSDK.Interfaces;
using WLVPN.Interfaces;
using WLVPN.Properties;

namespace WLVPN.ViewModels
{
    public class SplitTunnelingApplicationViewModel : Caliburn.Micro.Screen, ISplitTunnelingTabItem
    {
        private const string SupportedExtension = ".exe";
        private const string UnsupportedApp = "ConsumerVPN";

        public string TabHeaderTitle => Properties.Strings.Applications;
        public bool ShowList => Applications.Any();
        public ObservableCollection<SplitTunnelApp> Applications { get; set; }
        public ISDK SDK { get; }

        public SplitTunnelingApplicationViewModel(ISDK sdk)
        {
            SDK = sdk;
            Applications = new ObservableCollection<SplitTunnelApp>(SDK.SplitTunnelAllowedApps);
        }

        public void PickManually()
        {
            var filter = string.Join("|", SupportedExtension
                                  .Select(_ => $"Executable (*{SupportedExtension})|*{SupportedExtension}"));

            // Create OpenFileDialog
            OpenFileDialog openFileDlg = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                DefaultExt = SupportedExtension,
                Filter = filter,
                CheckFileExists = true,
                Title = Strings.AddApplications,
                CheckPathExists = true,
                Multiselect = true,
            };
            openFileDlg.FileOk += OpenFileDlg_FileOk;

            openFileDlg.ShowDialog();

        }

        private void OpenFileDlg_FileOk(object sender, CancelEventArgs e)
        {
            if (sender is OpenFileDialog openFileDlg)
            {
                foreach (var filePath in openFileDlg.FileNames)
                {
                    var extension = Path.GetExtension(filePath);
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    if (!SupportedExtension.Equals(extension, System.StringComparison.OrdinalIgnoreCase) ||
                        UnsupportedApp.Equals(fileName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        var message = string.Format(CultureInfo.CurrentCulture,Strings.UnsupportedFileChosen, fileName);
                        System.Windows.MessageBox.Show(message, Strings.InvalidApplication, MessageBoxButton.OK, MessageBoxImage.Error);
                        e.Cancel = true;
                        continue;
                    }

                    if (Applications.Any(app => app.Path == filePath))
                    {
                        var message = string.Format(Strings.ApplicationAlreadyAdded, fileName);
                        System.Windows.MessageBox.Show(message, Strings.DuplicateApplication, MessageBoxButton.OK, MessageBoxImage.Error);
                        e.Cancel = true;
                        continue;
                    }

                    SplitTunnelApp splitTunnelApp = new SplitTunnelApp(fileName, filePath);
                    Applications.Add(splitTunnelApp);
                    SDK.SplitTunnelAllowedApps.Add(splitTunnelApp);
                    NotifyOfPropertyChange(nameof(ShowList));
                }
            }
        }

        public void DeleteApplication(SplitTunnelApp application)
        {
            try
            {
                Applications.Remove(application);
                SDK.SplitTunnelAllowedApps.Remove(application);
            }
            catch (Exception)
            {
                //ignore.
            }
        }
    }
}
