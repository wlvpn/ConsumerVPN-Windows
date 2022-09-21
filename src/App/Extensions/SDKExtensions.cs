using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WLVPN.Interfaces;
using WLVPN.Properties;
using Serilog;
using VpnSDK;
using VpnSDK.Enums;
using VpnSDK.Interfaces;
using MessageBoxOptions = WLVPN.Enums.MessageBoxOptions;
using System.Globalization;

namespace WLVPN.Extensions
{
    public static class SDKExtensions
    {
        private static CancellationTokenSource TokenSource { get; set; }
        private static IDialogManager _dialog = AppBootstrapper.ContainerInstance.GetInstance<IDialogManager>();

        public static CancellationTokenSource GetCancellationTokenSource(this ISDK sdk)
        {
            return TokenSource;
        }

        public static void SetLocation(this ISDK sdk, ILocation location)
        {

            if (Settings.Default.SelectedCountry != location?.CountryCode)
            {
                Settings.Default.SelectedCountry = location?.CountryCode;
            }

            if (Settings.Default.SelectedCity != location?.CityCode)
            {
                Settings.Default.SelectedCity = location?.CityCode;
            }
            Properties.Settings.Default.Save();
        }


        public static void SetCancellationTokenSource(this ISDK sdk, CancellationTokenSource cts)
        {
            try
            {
                TokenSource?.Dispose();
                TokenSource = null;
            }
            catch (ObjectDisposedException)
            {
                // Ignore
            }
            TokenSource = cts;
        }

        public static async Task InitiateConnection(this ISDK sdk, ILocation SpecificLocation = null)
        {
            if (sdk.ActiveConnectionInformation != null && SpecificLocation != null)
            {
                _dialog.ShowMessageBox(
                    string.Format(CultureInfo.InvariantCulture,
                        Strings.LocationChangeConfirmation,
                        sdk.ActiveConnectionInformation.Location?.City,
                        sdk.ActiveConnectionInformation.Location?.Country,
                        SpecificLocation.City, 
                        SpecificLocation.Country
                        ),
                    Properties.Strings.Warning,
                    MessageBoxOptions.YesNo, async box =>
                    {
                        if (box.WasSelected(MessageBoxOptions.Yes))
                        {
                            await sdk.Disconnect();
                            await Connect(sdk, SpecificLocation);
                        }
                        else
                        {
                            return;
                        }
                    });
            }
            else
            {        
                await Connect(sdk, SpecificLocation);
            }
        }

        public static async Task Reconnect(this ISDK sdk, bool lastAttempt)
        {
            Log.Information("Reconnecting...");
            await Connect(sdk, null, true, lastAttempt);
        }

        private static async Task Connect(ISDK sdk, ILocation SpecificLocation = null, bool reconnect = false, bool lastAttempt = false)
        {
            if (SpecificLocation != null)
            {
                sdk.SetLocation(SpecificLocation);
            }

            try
            {
                IConnectionConfiguration configuration = null;

                if (Settings.Default.ConnectionProtocol == NetworkConnectionType.OpenVPN)
                {
                    configuration = new OpenVpnConnectionConfigurationBuilder()
                        .SetCipher(Settings.Default.Scramble ? OpenVpnCipherType.AES_128_CBC : Settings.Default.CipherType)
                        .SetScramble(Settings.Default.Scramble)
                        .SetNetworkProtocol(Settings.Default.OpenVpnProtocol)
                        .Build();
                }
                else if (Settings.Default.ConnectionProtocol == NetworkConnectionType.WireGuard)
                {
                    configuration = new WireGuardConnectionConfigurationBuilder()
                        .Build();
                }
                else
                {
                    configuration = new RasConnectionConfigurationBuilder()
                        .SetConnectionType(Settings.Default.ConnectionProtocol)
                        .Build();
                }

                sdk.SetCancellationTokenSource(new CancellationTokenSource());

                if (SpecificLocation != null)
                {
                    // If specific location is set
                    await sdk.Connect(SpecificLocation, configuration, sdk.GetCancellationTokenSource().Token);
                }
                else
                {
                    // If no specific location set, use whatever was saved to the Settings.
                    if (!string.IsNullOrEmpty(Settings.Default.SelectedCity) && !string.IsNullOrEmpty(Settings.Default.SelectedCountry))
                    {
                        ILocation location = sdk.Locations.FirstOrDefault(x => x.CityCode == Settings.Default.SelectedCity);
                        await sdk.Connect(location, configuration, sdk.GetCancellationTokenSource().Token);
                    }
                    else if (!string.IsNullOrEmpty(Settings.Default.SelectedCountry))
                    {
                        List<ILocation> locations = sdk.Locations.Where(x => x.CountryCode == Settings.Default.SelectedCountry).ToList();
                        await sdk.Connect(locations, configuration, sdk.GetCancellationTokenSource().Token);
                    }
                    else
                    {
                        // First() is always Best Available.
                        await sdk.Connect(sdk.Locations.First(), configuration, sdk.GetCancellationTokenSource().Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Do nothing. Finally block will handle it.
            }
            catch (TapAdapterException)
            {
                _dialog.ShowMessageBox(Strings.TapNotInstalledText, Strings.TapNotInstalledTitle,
                    MessageBoxOptions.YesNo, async box =>
                    {
                        if (box.WasSelected(MessageBoxOptions.Yes))
                        {
                            await sdk.TapInstallOrRepair();
                        }
                    });
            }
            catch (Exception e)
            {
                if (reconnect == false || (reconnect == true && lastAttempt == true))
                {
                    _dialog.ShowMessageBox(e.Message, "VPN Connection Failed");
                }
            }
        }

        public static async Task TapInstallOrRepair(this ISDK sdk)
        {
            try
            {
                Log.Information("Installing OpenVPN TAP Device.");
                DriverInstallResult result = await sdk.InstallTapDriver();

                switch (result)
                {
                    case DriverInstallResult.Success:
                        Log.Information("TAP device installed.");
                        _dialog.ShowMessageBox(Properties.Strings.TapDriverSuccess, Properties.Strings.TapInstaller);
                        break;

                    case DriverInstallResult.RebootRequired:
                        Log.Warning("TAP device installed but requires reboot.");
                        _dialog.ShowMessageBox(Properties.Strings.TabDriverRebootRequired,
                            Properties.Strings.TapInstaller);
                        break;

                    case DriverInstallResult.Failed:
                        Log.Error("TAP device failed to install.");
                        _dialog.ShowMessageBox(Properties.Strings.TapDriverFail, Properties.Strings.TapInstaller);
                        break;
                }
            }
            catch (UnsupportedProtocolException)
            {
                Log.Error("OpenVPN is not a supported protocol.");
                _dialog.ShowMessageBox(Properties.Strings.OpenVpnNotSupportedTapInstall);
            }
            catch (Exception e)
            {
                Log.Error(e, "TAP install had an unknown error.");
                _dialog.ShowMessageBox($"{Localization.GetValue<string>(nameof(Strings.TapDriverFail))}\r\n{e.Message}", Strings.TapInstaller);
            }
        }
    }
}
