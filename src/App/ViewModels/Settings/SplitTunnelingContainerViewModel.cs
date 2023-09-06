using Caliburn.Micro;
using DynamicData;
using Org.BouncyCastle.Bcpg;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VpnSDK.Common.Enums;
using VpnSDK.Common.Settings;
using VpnSDK.Interfaces;
using WLVPN.Helpers;
using WLVPN.Interfaces;

namespace WLVPN.ViewModels
{
    public class SplitTunnelingContainerViewModel : Conductor<ISplitTunnelingTabItem>.Collection.OneActive, ISettingsTabItem
    {
        public string TabHeaderTitle => Properties.Strings.TabSettingsSplitTunneling;

        public Style Icon => Resource.Get<Style>("SplitIcon");

        [AlsoNotifyFor("CanAddItems")]
        public bool IsSplitTunnelingOn
        {
            get => SDK.IsSplitTunnelEnabled;
            set
            {
                SDK.IsSplitTunnelEnabled = value;
            }
        }

        public ISDK SDK { get; }

        public bool CanAddItems => !IsSplitTunnelingOn && !SDK.IsConnected;
        public SplitTunnelingContainerViewModel(ISDK sdk, IEnumerable<ISplitTunnelingTabItem> tabs)
        {
            SDK = sdk;
            Items.AddRange(tabs);

            SDK.SplitTunnelMode = SplitTunnelMode.RouteSelectedTrafficOutsideVpn;
            SDK.SplitTunnelAllowedApps = new List<SplitTunnelApp>();
            SDK.SplitTunnelAllowedDomains = new List<SplitTunnelDomain>();
        }
    }
}
