using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WLVPN.Interfaces;
using VpnSDK.Enums;
using VpnSDK.Interfaces;
using System.Windows;
using WLVPN.Helpers;
using VpnSDK.DnsMonitor.DTO;
using System.Windows.Forms;
using Screen = Caliburn.Micro.Screen;
using WLVPN.Properties;
using System.ComponentModel;
using System.IO;
using VpnSDK.TrafficOptimizer.DTO;
using System.Collections.ObjectModel;
using WLVPN.Model;

namespace WLVPN.ViewModels
{
    public class ConnectionSettingsViewModel : Screen, ISettingsTabItem
    {
        private bool _openVpnScramble = Properties.Settings.Default.Scramble;
        private const string SupportedExtension = ".exe";
        private const string UnsupportedApp = "ConsumerVPN";
        private static IDialogManager _dialog = AppBootstrapper.ContainerInstance.GetInstance<IDialogManager>();

        private TrafficOptimizerConfig _trafficOptimizerConfig;

        public ConnectionSettingsViewModel(ISDK sdk)
        {
            SDK = sdk;

            ConnectionTypes = SDK.AvailableProtocols.Where(x => x.Value == true).Select(x => x.Key).ToList();

            if (!SDK.AvailableProtocols.Where(x => x.Value == true).Select(x => x.Key.ToString().Equals(Properties.Settings.Default.ConnectionProtocol)).Any())
            {
                Properties.Settings.Default.ConnectionProtocol = ConnectionTypes.First();
                Properties.Settings.Default.Save();
            }
            _trafficOptimizerConfig = new TrafficOptimizerConfig();
            IsDoubleHopEnabled = false;
            Properties.Settings.Default.PropertyChanged += OnSettingsPropertyChanged;
        }

        public string TabHeaderTitle => Properties.Strings.TabSettingsConnection;
        public bool ShowList => Applications.Any();
        public static Style Icon => Resource.Get<Style>("SignalIcon");

        public ISDK SDK { get; }

        public static string ReconnectTries
        {
            get => Properties.Settings.Default.ReconnectTries.ToString(CultureInfo.InvariantCulture);
            set
            {
                if (int.TryParse(value, out int tries))
                {
                    if (tries > 99999)
                    {
                        tries = 99999;
                    }
                    if (tries < 1)
                    {
                        tries = 1;
                    }
                }
                else
                {
                    tries = 5;
                }

                Properties.Settings.Default.ReconnectTries = tries;
                Properties.Settings.Default.Save();
            }
        }

        public List<NetworkConnectionType> ConnectionTypes { get; }

        public List<NetworkProtocolType> OpenVpnProtocols { get; } = new List<NetworkProtocolType> { NetworkProtocolType.UDP, NetworkProtocolType.TCP };

        public ObservableCollection<ApplicationDetails> Applications { get; set; } = new ObservableCollection<ApplicationDetails>();

        private void OnSettingsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Properties.Settings.Default.KillSwitch))
            {
                SDK.AllowOnlyVPNConnectivity = Properties.Settings.Default.KillSwitch;
                if (Properties.Settings.Default.KillSwitch == false)
                {
                    Properties.Settings.Default.BlockLANTraffic = false;
                    Properties.Settings.Default.Save();
                }
            }

            if (e.PropertyName == nameof(Properties.Settings.Default.BlockLANTraffic))
            {
                SDK.AllowLANTraffic = !Properties.Settings.Default.BlockLANTraffic;
                Properties.Settings.Default.Save();
            }

            if (e.PropertyName == nameof(Properties.Settings.Default.DisableDNSLeakProtection))
            {
                SDK.DisableDNSLeakProtection = Properties.Settings.Default.DisableDNSLeakProtection;
                Properties.Settings.Default.Save();
            }

            if (e.PropertyName == nameof(Properties.Settings.Default.DisableIPv6LeakProtection))
            {
                SDK.DisableIPv6LeakProtection = Properties.Settings.Default.DisableIPv6LeakProtection;
                Properties.Settings.Default.Save();
            }

            if (e.PropertyName == nameof(Properties.Settings.Default.AllowLanInterfaces))
            {
                SDK.AllowLocalAdaptersWhenConnected = Properties.Settings.Default.AllowLanInterfaces;
                Properties.Settings.Default.Save();
            }

            if (e.PropertyName == nameof(Properties.Settings.Default.IsThreatProtectionEnabled))
            {
                SDK.DnsFilterMode = IsThreatProtectionEnabled ? VpnSDK.Enums.DnsFilteringMode.WithWLVPNDns : VpnSDK.Enums.DnsFilteringMode.Disabled;
            }

            if (e.PropertyName == nameof(Properties.Settings.Default.DnsMonitoring))
            {
                SDK.SetDnsMonitorConfig(new DnsMonitoringConfig()
                {
                    IsEnabled = Properties.Settings.Default.DnsMonitoring,
                });
            }
        }

        /// <summary>
        /// Gets a list of the available protocols for OpenVPN
        /// </summary>
        public List<NetworkProtocolType> AvailableOpenVpnProtocols => Enum.GetValues(typeof(NetworkProtocolType)).Cast<NetworkProtocolType>().ToList();

        /// <summary>
        /// Gets or sets a value indicating what connection protocol the application should use.
        /// </summary>
        public NetworkConnectionType ConnectionProtocol
        {
            get { return Properties.Settings.Default.ConnectionProtocol; }
            set
            {
                Properties.Settings.Default.ConnectionProtocol = value;
                NotifyOfPropertyChange(nameof(IsCipherEnabled));
            }
        }

        /// <summary>
        /// Gets the list of available OpenVPN Cipher types
        /// </summary>
        public List<OpenVpnCipherType> CipherComboBox => Enum.GetValues(typeof(OpenVpnCipherType)).Cast<OpenVpnCipherType>().ToList();

        /// <summary>
        /// Gets or sets the Cipher used for the OpenVPN protocol   128 or 256
        /// </summary>
        public OpenVpnCipherType Cipher
        {
            get => Properties.Settings.Default.CipherType;
            set
            {
                Properties.Settings.Default.CipherType = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Cipher Combo Box is enabled
        /// The Cipher must be set to 128 when Scramble is enabled and we do not let the user change it.
        /// </summary>
        public bool IsCipherEnabled
        {
            get
            {
                if (ConnectionProtocol == NetworkConnectionType.OpenVPN && OpenVpnScramble == false)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application should enable Scramble
        /// </summary>
        /// <value>true or false</value>
        public bool OpenVpnScramble
        {
            get { return _openVpnScramble; }
            set
            {
                _openVpnScramble = value;
                if (value)
                {
                    Cipher = OpenVpnCipherType.AES_128_CBC;
                }
                else
                {
                    Cipher = OpenVpnCipherType.AES_256_CBC;
                }
                NotifyOfPropertyChange(nameof(IsCipherEnabled));
                Properties.Settings.Default.Scramble = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets OpenVPN protocol.
        /// </summary>
        /// <value><see cref="NetworkProtocolType"/></value>
        public NetworkProtocolType OpenVpnProtocol
        {
            get
            {
                return Properties.Settings.Default.OpenVpnProtocol;
            }
            set
            {
                Properties.Settings.Default.OpenVpnProtocol = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application reconnect a user on connection lost or failure.
        /// </summary>
        /// <value>true or false</value>
        public bool AutoReconnect
        {
            get
            {
                return Properties.Settings.Default.AutoReconnect;
            }
            set
            {
                Properties.Settings.Default.AutoReconnect = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Kill Switch feature is enabled.
        /// </summary>
        /// <value>true or false</value>
        public bool KillSwitch
        {
            get
            {
                return Properties.Settings.Default.KillSwitch;
            }
            set
            {
                Properties.Settings.Default.KillSwitch = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Block LAN Traffic feature is enabled.
        /// </summary>
        /// <value>true or false</value>
        public bool BlockLANTraffic
        {
            get
            {
                return Properties.Settings.Default.BlockLANTraffic;
            }
            set
            {
                Properties.Settings.Default.BlockLANTraffic = value;
                Properties.Settings.Default.Save();
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether the local adapters are allowed when connected to a VPN server.
        /// </summary>
        /// <value>true or false</value>
        public bool AllowLanInterfaces
        {
            get => Properties.Settings.Default.AllowLanInterfaces;

            set
            {
                Properties.Settings.Default.AllowLanInterfaces = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the VPN connection should allow DNS to leak.
        /// </summary>
        /// <value>true or false</value>
        public bool DisableDNSLeakProtection
        {
            get => Properties.Settings.Default.DisableDNSLeakProtection;

            set
            {
                Properties.Settings.Default.DisableDNSLeakProtection = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the VPN connection should allow IPv6 to leak.
        /// </summary>
        /// <value>true or false</value>
        public bool DisableIPv6LeakProtection
        {
            get => Properties.Settings.Default.DisableIPv6LeakProtection;

            set
            {
                Properties.Settings.Default.DisableIPv6LeakProtection = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Threat protection feature is enabled.
        /// Enable threat protection with the default WLVPN DNS.Optionally, partners can choose to use their own DNS server for threat protection.
        /// To do this, configure the DNS server at the backend and, after enabling threat protection, set SDK.DnsFilterMode to DnsFilteringMode.WithPartnerDns.
        /// </summary>
        /// <value>true or false</value>
        public bool IsThreatProtectionEnabled
        {
            get
            {
                return Properties.Settings.Default.IsThreatProtectionEnabled;
            }
            set
            {
                Properties.Settings.Default.IsThreatProtectionEnabled = value;
                Properties.Settings.Default.Save();
            }
        }
        public bool IsOpenVpnAvailable
        {
            get => SDK.AvailableProtocols.Any(x => x.Key == NetworkConnectionType.OpenVPN && x.Value == true);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the DNS Monitoring is enabled or not.
        /// </summary>
        /// <value>true or false</value>
        public bool DnsMonitoring
        {
            get => Properties.Settings.Default.DnsMonitoring;

            set
            {
                Properties.Settings.Default.DnsMonitoring = value;
                Properties.Settings.Default.Save();
            }
        }

        ///// <summary>
        ///// Sets a value indicating whether double hop is enabled.
        ///// </summary>
        public bool IsDoubleHopEnabled
        {
            get
            {
                return Properties.Settings.Default.IsDoubleHopEnabled;
            }
            set
            {
                Properties.Settings.Default.IsDoubleHopEnabled = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Gets or sets the entry location.
        /// </summary>
        public ILocation SelectedLocation
        {
            get => Properties.Settings.Default.EntryLocation;

            set
            {
                Properties.Settings.Default.EntryLocation = value;
                Properties.Settings.Default.Save();
            }
        }

        public void PickManually()
        {
            // Maximum 3 applications can be added
            if (Applications.Count < 3)
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
            else
            {
                var message = string.Format(CultureInfo.CurrentCulture, Strings.MaxThreeApplication);
                _dialog.ShowMessageBox(message, Strings.ReachedMaxLimit, Enums.MessageBoxOptions.Ok);
            }

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
                        var message = string.Format(CultureInfo.CurrentCulture, Strings.UnsupportedFileChosen, fileName);
                        System.Windows.MessageBox.Show(message, Strings.InvalidApplication, MessageBoxButton.OK, MessageBoxImage.Error);
                        e.Cancel = true;
                        continue;
                    }

                    if (Applications.Any(_ => _.Path == filePath))
                    {
                        var message = string.Format(Strings.ApplicationAlreadyAdded, fileName);
                        System.Windows.MessageBox.Show(message, Strings.DuplicateApplication, MessageBoxButton.OK, MessageBoxImage.Error);
                        e.Cancel = true;
                        continue;
                    }
                   
                    Applications.Add(new ApplicationDetails(Path.GetFileNameWithoutExtension(filePath),filePath));
                    UpdateConfiguration();
                    NotifyOfPropertyChange(nameof(ShowList));
                }
            }
        }

        private void UpdateConfiguration()
        {
            _trafficOptimizerConfig.PrioritizedApps = Applications.Select(_ => _.Path).ToArray();
            SDK.SetTrafficOptimizerConfig(_trafficOptimizerConfig);
        }

        public void DeleteApplication(ApplicationDetails process)
        {
            try
            {
                Applications.Remove(process);
                UpdateConfiguration();
                NotifyOfPropertyChange(nameof(ShowList));
            }
            catch (Exception)
            {
                //ignore.
            }
        }        
    }
}


