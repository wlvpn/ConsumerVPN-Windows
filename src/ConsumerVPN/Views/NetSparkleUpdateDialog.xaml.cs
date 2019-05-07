using NetSparkle;
using NetSparkle.Enums;
using NetSparkle.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WLVPN.Helpers;
using WLVPN.Properties;

namespace WLVPN.Views
{
    /// <summary>
    /// Interaction logic for NetSparkleUpdateDialog.xaml
    /// </summary>
    public partial class NetSparkleUpdateDialog : Window, IUpdateAvailable
    {
        public NetSparkleUpdateDialog(AppCastItem appCast)
        {
            InitializeComponent();
            if (Application.Current?.MainWindow != null)
            {
                Owner = Application.Current.MainWindow;
                Icon = Application.Current.MainWindow.Icon;
            }

            DataContext = this;
            CurrentItem = appCast;

            if (string.IsNullOrEmpty(appCast.ReleaseNotesLink) == false)
            {
                Browser.Navigate(new Uri(appCast.ReleaseNotesLink));
            }
            else if (string.IsNullOrEmpty(appCast.Description) == false)
            {
                Browser.NavigateToString(appCast.Description);
            }

            Description.Content = string.Format(
                CultureInfo.InvariantCulture,
                Strings.UpdaterWindowDescription,
                Resource.Get<string>("ApplicationName"),
                CurrentItem.Version,
                CurrentItem.AppVersionInstalled);
        }

        public UpdateAvailableResult Result { get; private set; }

        public AppCastItem CurrentItem { get; }

        public event EventHandler UserResponded;

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnInstallClick(object sender, RoutedEventArgs e)
        {
            Result = UpdateAvailableResult.InstallUpdate;
            Close();
            UserResponded?.Invoke(this, new EventArgs());
        }

        private void OnSkipClick(object sender, RoutedEventArgs e)
        {
            Result = UpdateAvailableResult.SkipUpdate;
            UserResponded?.Invoke(this, new EventArgs());
        }

        private void OnRemindClick(object sender, RoutedEventArgs e)
        {
            Result = UpdateAvailableResult.RemindMeLater;
            UserResponded?.Invoke(this, new EventArgs());
        }

        public void HideReleaseNotes()
        {
            Browser.Visibility = Visibility.Collapsed;
        }

        public void HideRemindMeLaterButton()
        {
            // ignore
        }

        public void HideSkipButton()
        {
            // ignore
        }

        public void BringToFront()
        {
            Activate();
        }

        void IUpdateAvailable.Show()
        {
            try
            {
                ShowDialog();
                Close();
            }
            catch
            {
                // ignore
            }
        }
    }
}
