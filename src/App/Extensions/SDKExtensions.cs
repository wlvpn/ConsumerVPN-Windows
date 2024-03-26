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
using WLVPN.ViewModels;

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

            Settings.Default.SelectedDestination = location;
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
                List<IConnectionConfiguration> configurations = new List<IConnectionConfiguration>();

                // Order is important for Automatic Protocol feature. It starts with first configuration and then uses the next one as a fallback.
                // Lets stick with the next protocols order for now: WG, OpenVPN, IKEv2.                
                configurations.Add(new WireGuardConnectionConfigurationBuilder()
               .SetBlockUntunneledTraffic(!Properties.Settings.Default.AllowLanInterfaces)
               .SetDoubleHopSettings(Properties.Settings.Default.IsDoubleHopEnabled, Properties.Settings.Default.EntryLocation, Properties.Settings.Default.SelectedDestination)
               .Build());
                configurations.Add(new RasConnectionConfigurationBuilder().SetConnectionType(NetworkConnectionType.IKEv2).Build());
                configurations.Add(
                    new OpenVpnConnectionConfigurationBuilder()
                        .SetCipher(Properties.Settings.Default.Scramble ? OpenVpnCipherType.AES_128_CBC : OpenVpnCipherType.AES_256_CBC)
                        .SetScramble(Properties.Settings.Default.Scramble)
                        .SetNetworkProtocol(Properties.Settings.Default.OpenVpnProtocol)
                        .SetDoubleHopSettings(Properties.Settings.Default.IsDoubleHopEnabled, Properties.Settings.Default.EntryLocation, Properties.Settings.Default.SelectedDestination)
                        .Build());

                sdk.SetCancellationTokenSource(new CancellationTokenSource());

                if (SpecificLocation != null)
                {
                    // If specific location is set
                    await Connect(sdk,SpecificLocation, configurations, sdk.GetCancellationTokenSource().Token);
                }
                else
                {
                    // If no specific location set, use whatever was saved to the Settings.
                    if (!string.IsNullOrEmpty(Settings.Default.SelectedCity) && !string.IsNullOrEmpty(Settings.Default.SelectedCountry))
                    {
                        ILocation location = sdk.Locations.FirstOrDefault(x => x.CityCode == Settings.Default.SelectedCity);
                        await Connect(sdk,location, configurations, sdk.GetCancellationTokenSource().Token);
                    }
                    else if (!string.IsNullOrEmpty(Settings.Default.SelectedCountry))
                    {
                        List<ILocation> locations = sdk.Locations.Where(x => x.CountryCode == Settings.Default.SelectedCountry).ToList();
                        await Connect(sdk,null,configurations, sdk.GetCancellationTokenSource().Token, locations);
                    }
                    else
                    {
                        // First() is always Best Available.
                        await Connect(sdk,sdk.Locations.First(), configurations, sdk.GetCancellationTokenSource().Token);
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
            catch (InvalidDoubleHopConfigurationException e)
            {
                _dialog.ShowMessageBox(e.Message, "Double hop");
            }
            catch (Exception e)
            {
              if (reconnect == false || (reconnect == true && lastAttempt == true))
                {
                    _dialog.ShowMessageBox(e.Message, "VPN Connection Failed");
                }
            }
        }

        private async static Task Connect(ISDK sdk, ILocation location, List<IConnectionConfiguration> configurations, CancellationToken token, List<ILocation> locations = null)
        {
            // If the network connection type is Unspecified, this means that the Automatic Protocol feature is selected.
            if (Properties.Settings.Default.ConnectionProtocol == NetworkConnectionType.Automatic)
            {
                await sdk.Connect(locations ?? new List<ILocation>() { location }, configurations, token);
            }
            else
            {
                // Else, connect to a VPN server using a specific Network connection type.
                var connectionType = configurations.First(x => x.ConnectionType == Properties.Settings.Default.ConnectionProtocol);
                await sdk.Connect(locations ?? new List<ILocation>() { location }, connectionType, token);
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
