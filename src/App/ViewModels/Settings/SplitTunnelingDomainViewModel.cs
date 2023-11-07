using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using VpnSDK.Common.Settings;
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
            Domains = new ObservableCollection<SplitTunnelDomain>();
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
