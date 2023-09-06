using Caliburn.Micro;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using VpnSDK.Common.Settings;
using VpnSDK.Common.Utilities;
using VpnSDK.Interfaces;
using WLVPN.Interfaces;
using WLVPN.Properties;

namespace WLVPN.ViewModels
{
    internal class SplitTunnelingDomainViewModel : Caliburn.Micro.Screen, ISplitTunnelingTabItem
    {
        public string TabHeaderTitle => Properties.Strings.Domains;
        public bool ShowList => Domains.Any();
        public string DomainName { get; set; }
        public string ValidationError { get; set; }
        public ISDK SDK { get; set; }

        public ObservableCollection<SplitTunnelDomain> Domains { get; set; } 

        public SplitTunnelingDomainViewModel(ISDK sdk)
        {
            SDK = sdk;
            Domains = new ObservableCollection<SplitTunnelDomain>(SDK.SplitTunnelAllowedDomains);
        }
        public void AddDomain()
        {
            try
            {
                ValidationError = string.Empty;
                var domain = new SplitTunnelDomain(DomainName);
                if (Domains.Any(d => d.DomainName.Equals(domain.DomainName, StringComparison.OrdinalIgnoreCase)))
                {
                    ValidationError = Strings.AddDomainAlreadyExistsError;
                    return;
                }
                Domains.Add(domain);
                SDK.SplitTunnelAllowedDomains.Add(domain);
                DomainName = string.Empty;
                NotifyOfPropertyChange(nameof(ShowList));
            }
            catch (Exception ex)
            {
                ValidationError =  ex.Message;
            }
        }

        public void UpdateIncludeSubDomains(bool includeAllSubDomains, SplitTunnelDomain domain)
        {
            domain.IncludeAllSubdomains = includeAllSubDomains;
        }

        public void DeleteDomain(SplitTunnelDomain domain)
        {
            try
            {
                Domains.Remove(domain);
                SDK.SplitTunnelAllowedDomains.Remove(domain);
            }
            catch (Exception)
            {

                //ignore.
            }
           
        }
    }
}
