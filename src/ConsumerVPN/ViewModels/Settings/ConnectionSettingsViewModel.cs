using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using WLVPN.Interfaces;
using VpnSDK.Enums;
using VpnSDK.Interfaces;
using WPFLocalizeExtension.Engine;
using System.Windows;
using WLVPN.Helpers;

namespace WLVPN.ViewModels
{
    public class ConnectionSettingsViewModel : Screen , ISettingsTabItem
    {
        private bool _openVpnScramble = Properties.Settings.Default.Scramble;

        public ConnectionSettingsViewModel(ISDK sdk)
        {
            SDK = sdk;
            
            ConnectionTypes = SDK.AvailableProtocols.Where(x => x.Value == true).Select(x => x.Key).ToList();

            if (!SDK.AvailableProtocols.Where(x => x.Value == true).Select(x => x.Key.ToString().Equals(Properties.Settings.Default.ConnectionProtocol)).Any())
            {
                Properties.Settings.Default.ConnectionProtocol = ConnectionTypes.First();
            }

            Properties.Settings.Default.PropertyChanged += OnSettingsPropertyChanged;
        }

        public string TabHeaderTitle => Properties.Strings.TabSettingsConnection;

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
            }
        }

        public List<NetworkConnectionType> ConnectionTypes { get; }

        public List<NetworkProtocolType> OpenVpnProtocols { get; } = new List<NetworkProtocolType> { NetworkProtocolType.UDP, NetworkProtocolType.TCP };


        private void OnSettingsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Properties.Settings.Default.KillSwitch))
            {
                SDK.AllowOnlyVPNConnectivity = Properties.Settings.Default.KillSwitch;
                if (Properties.Settings.Default.KillSwitch == false)
                {
                    Properties.Settings.Default.BlockLANTraffic = false;
                }
            }
            if (e.PropertyName == nameof(Properties.Settings.Default.BlockLANTraffic))
            {
                SDK.AllowLANTraffic = !Properties.Settings.Default.BlockLANTraffic;
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
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Cipher Combo Box is enabled
        /// The Cipher must be set to 128 when Scramble is enabled and we do not let the user change it.
        /// </summary>
        public bool IsCipherEnabled {
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
            }
        }


        public bool IsOpenVpnAvailable
        {
            get => SDK.AvailableProtocols.Any(x => x.Key == NetworkConnectionType.OpenVPN && x.Value == true);
        }
    }
}
