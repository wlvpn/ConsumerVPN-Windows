// <copyright file="SettingsConnectionsViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using VpnSDK.Public.Enums;
using VpnSDK.WLVpn.Helpers;

namespace VpnSDK.WLVpn.ViewModels
{
    /// <summary>
    /// This view allows the user to chose and configure the various connection protocols
    /// </summary>
    public class SettingsConnectionsViewModel : BindableBase, ISettingsViewModel
    {
        private NetworkConnectionType _networkConnectionType = NetworkConnectionType.IKEv2;
        private NetworkProtocolType _selectedOpenVpnProtocol = NetworkProtocolType.UDP;
        private bool _openVpnScramble = false;
        private bool _cipherEnabled = false;
        private RelayCommand _installTapDriverCmd = null;
        private OpenVpnCipherType _cipherType = OpenVpnCipherType.AES_256_CBC;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsConnectionsViewModel"/> class.
        /// </summary>
        /// <param name="sdk">SDKMonitor is a singleton that represents the layer above the VpnSDK</param>
        public SettingsConnectionsViewModel(SDKMonitor sdk)
        {
            if (IsInDesignMode())
            {
                Init();
                return;
            }

            SdkMonitor = sdk;

            // get the right values for the fields below from the sdkMonitor class.
            // we then set them if the user says "save"
            // some of them may be things we do in the client instead of the sdk.
            // those we will have to save and restore from local storage.
            // since the Cancel method does this, we call it here.
            Cancel();
        }

        /// <summary>
        /// Gets or sets a value indicating what connection protocol the application should use.
        /// </summary>
        public NetworkConnectionType ConnectionProtocol
        {
            get
            {
                return _networkConnectionType;
            }

            set
            {
                SetProperty(ref _networkConnectionType, value);
                if (value == NetworkConnectionType.IKEv2)
                {
                    OpenVpnScramble = false;
                    Cipher = OpenVpnCipherType.AES_256_CBC;
                    SelectedOpenVpnProtocol = NetworkProtocolType.UDP;
                    IsCipherEnabled = false;
                }
                else
                {
                    IsCipherEnabled = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating what VPN protocol the application should use.
        /// </summary>
        public NetworkProtocolType SelectedOpenVpnProtocol
        {
            get { return _selectedOpenVpnProtocol; }
            set { SetProperty(ref _selectedOpenVpnProtocol, value); }
        }

        /// <summary>
        /// Gets a list of the available protocols for OpenVPN
        /// </summary>
        public List<NetworkProtocolType> AvailableOpenVpnProtocols => Enum.GetValues(typeof(NetworkProtocolType)).Cast<NetworkProtocolType>().ToList();

        /// <summary>
        /// Gets or sets a value indicating whether the application should enable Scramble
        /// </summary>
        /// <value>true or false</value>
        public bool OpenVpnScramble
        {
            get
            {
                return _openVpnScramble;
            }

            set
            {
                SetProperty(ref _openVpnScramble, value);
                if (value)
                {
                    Cipher = OpenVpnCipherType.AES_128_CBC;
                    IsCipherEnabled = false;
                }
                else
                {
                    Cipher = OpenVpnCipherType.AES_256_CBC;
                    IsCipherEnabled = true;
                }
            }
        }

        /// <summary>
        /// Gets a value that represents the command to execute when the user wants to install or reinstall TAP adapter.
        /// </summary>
        /// <value>the Command</value>
        public RelayCommand InstallTapDriverCmd
        {
            get
            {
                if (_installTapDriverCmd == null)
                {
                    _installTapDriverCmd = new RelayCommand(
                    (parm) =>
                    {
                        SdkMonitor?.InstallTapDriver();
                    }, (parm) => !SdkMonitor.IsConnected && SdkMonitor.IsLoggedIn);
                }

                return _installTapDriverCmd;
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
            get { return _cipherType; }
            set { SetProperty(ref _cipherType, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Cipher Combo Box is enabled
        /// The Cipher must be set to 128 when Scramble is enabled and we do not let the user change it.
        /// </summary>
        public bool IsCipherEnabled
        {
            get
            {
                return _cipherEnabled;
            }

            set
            {
                SetProperty(ref _cipherEnabled, value);
            }
        }

        /// <summary>
        /// Gets the SDKMonitor singleton
        /// </summary>
        public SDKMonitor SdkMonitor { get; }

        /// <summary>
        /// This method implements the steps needed to restore the properties to the values they were when the view was entered.
        /// </summary>
        /// <returns>true or false.  At the moment always true.  If validation is needed, this would be used to denote a failed validation</returns>
        public bool Cancel()
        {
            ConnectionProtocol = SdkMonitor.ConnectionProtocol;
            OpenVpnScramble = SdkMonitor.OpenVpnScramble;
            SelectedOpenVpnProtocol = SdkMonitor.SelectedOpenVpnProtocol;
            Cipher = SdkMonitor.CipherType;
            return true;
        }

        /// <summary>
        /// This method implements the steps needed to save the properties to the new values
        /// </summary>
        /// <returns>true or false.  At the moment always true.  If validation is needed, this would be used to denote a failed validation</returns>
        public bool Save()
        {
            SdkMonitor.ConnectionProtocol = ConnectionProtocol;
            SdkMonitor.OpenVpnScramble = OpenVpnScramble;
            SdkMonitor.SelectedOpenVpnProtocol = SelectedOpenVpnProtocol;
            SdkMonitor.CipherType = Cipher;
            return true;
        }

        /// <summary>
        /// This method is used to initialize any data needed at design time to help the Visual Studio designer display data
        /// </summary>
        private void Init()
        {
        }
    }
}
