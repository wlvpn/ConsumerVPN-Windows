using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using DynamicData;
using DynamicData.Binding;
using Serilog;
using VpnSDK;
using VpnSDK.Enums;
using VpnSDK.Interfaces;
using WLVPN.Extensions;
using WLVPN.Helpers;
using WLVPN.Interfaces;

namespace WLVPN.ViewModels
{
    /// <summary>
    /// Lists the dedicated IP servers assigned to the account. Only reachable when the
    /// user holds the <see cref="UserEntitlement.DedicatedIp"/> entitlement, see <see cref="MainViewModel"/>.
    /// </summary>
    public class DedicatedIpViewModel : Screen, IMainScreenTabItem
    {
        private readonly ObservableCollectionExtended<ILocation> _items = new ObservableCollectionExtended<ILocation>();
        private ILocation _selectedLocationItem;
        private IServer _selectedServer;

        public DedicatedIpViewModel(ISDK sdk)
        {
            SDK = sdk;

            var filter = this.WhenValueChanged(t => t.Search)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Select(BuildFilter);

            SDK.DedicatedIpServerLocations
                .ToObservableChangeSet(x => x.Id)
                .Filter(filter)
                .ObserveOn(SynchronizationContext.Current)
                .Bind(_items)
                .DisposeMany()
                .Subscribe();

            Locations = CollectionViewSource.GetDefaultView(_items) as ListCollectionView;
        }

        public ISDK SDK { get; }

        public string Title => Properties.Strings.DedicatedIp;

        public Style Icon => Resource.Get<Style>("DedicatedIpIcon");

        public ListCollectionView Locations { get; }

        public string Search { get; set; }

        /// <summary>
        /// The city row picked in the grid. Selecting one drops any server selection, so that
        /// there is a single destination at a time, the same way the SDK example's tree behaves.
        /// </summary>
        public ILocation SelectedLocationItem
        {
            get => _selectedLocationItem;

            set
            {
                _selectedLocationItem = value;

                if (value != null && _selectedServer != null)
                {
                    _selectedServer = null;
                    NotifyOfPropertyChange(nameof(SelectedServer));
                }
            }
        }

        /// <summary>
        /// The dedicated server picked inside an expanded city.
        /// </summary>
        public IServer SelectedServer
        {
            get => _selectedServer;

            set
            {
                // Every expanded city renders its own server list, and the lists that do not hold
                // the selection push a null back through this binding. The SDK example applies the
                // same guard in ExtendedTreeView.
                if (value == null)
                {
                    return;
                }

                _selectedServer = value;

                if (_selectedLocationItem != null)
                {
                    _selectedLocationItem = null;
                    NotifyOfPropertyChange(nameof(SelectedLocationItem));
                }
            }
        }

        /// <summary>
        /// What Connect acts on: the selected server when there is one, otherwise the selected city.
        /// </summary>
        public ILocation SelectedDestination => (ILocation)SelectedServer ?? SelectedLocationItem;

        /// <summary>
        /// Fetches the dedicated server list when there is nothing to show. The SDK populates it
        /// on login and keeps it current on its own timer, and a refresh that does run rebuilds
        /// the collection, which collapses the expanded rows and drops the selection, so
        /// refreshing on every activation costs the user more than it buys.
        /// </summary>
        protected override async void OnActivate()
        {
            base.OnActivate();

            if (SDK.DedicatedIpServerLocations.Count == 0)
            {
                await RefreshServers();
            }
        }

        public async Task Connect()
        {
            if (SelectedDestination == null)
            {
                return;
            }

            await SDK.InitiateConnection(SelectedDestination);
        }

        /// <summary>
        /// Double click on a city row.
        /// </summary>
        public async Task ConnectFromLocation(ILocation row, object view, MouseButtonEventArgs args)
        {
            // A DataGridRow raises MouseDoubleClick for everything inside it, the expander and the
            // expanded server list included, and both of those handle their own double clicks.
            for (DependencyObject source = args.OriginalSource as DependencyObject; source is Visual; source = VisualTreeHelper.GetParent(source))
            {
                if (source is ButtonBase || source is ListBoxItem)
                {
                    return;
                }
            }

            args.Handled = true;
            await SDK.InitiateConnection(row);
        }

        /// <summary>
        /// Double click on a dedicated server row, connects to that server's dedicated IP.
        /// </summary>
        public async Task ConnectFromServer(IServer server, MouseButtonEventArgs args)
        {
            args.Handled = true;
            await SDK.InitiateConnection(server);
        }

        private async Task RefreshServers()
        {
            try
            {
                await SDK.RefreshDedicatedIpServerLocations();
            }
            catch (FeatureNotAvailableException)
            {
                Log.Information("Dedicated IP is not available for this account.");
            }
            catch (Exception e)
            {
                Log.Error(e, "Unable to refresh the dedicated IP server list.");
            }
        }

        private Func<ILocation, bool> BuildFilter(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return x => true;
            }

            return x =>
            {
                if (!x.HasNode())
                {
                    return false;
                }

                if (x is IChildren<IServer> region)
                {
                    if (region.Children.Any(y => y.Hostname.CaseInsensitiveContains(searchText)
                                                 || y.DedicatedIpAddress?.Contains(searchText) == true))
                    {
                        return true;
                    }
                }

                return x.Id.CaseInsensitiveContains(searchText) || x.City.CaseInsensitiveContains(searchText) ||
                       x.Country.CaseInsensitiveContains(searchText) ||
                       x.SearchName.CaseInsensitiveContains(searchText);
            };
        }
    }
}
