using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using VpnSDK.Enums;
using VpnSDK.Interfaces;
using WLVPN.Helpers;
using WLVPN.Interfaces;
using WLVPN.Extensions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using DynamicData;
using DynamicData.Alias;
using DynamicData.Binding;
using DynamicData.PLinq;
using System.Reactive.Linq;
using System.Windows.Input;
using VpnSDK.DTO;
using System.Net;
using System.Threading;

namespace WLVPN.ViewModels
{
    public class HomeViewModel : Screen, IMainScreenTabItem
    {
        private ILocation _selectedLocationItem = null;
        private const double SpeedNormalization = 1000.0;
        private ObservableCollectionExtended<ILocation> _items = new ObservableCollectionExtended<ILocation>();
        private double _uploadedData;
        private double _downloadedData;

        public ISDK SDK { get; }

        public string Title => Properties.Strings.MainTabControlName;

        public Style Icon => Resource.Get<Style>("HomeIcon");

        public ConnectionStatus ConnectedState { get; set; } = ConnectionStatus.Disconnected;

        public ListCollectionView Locations { get; protected set; }

        public IPAddress IPAddress { get; set; }

        public string VisibleLocation { get; set; }

        public string VisibleLocationFlag { get; set; }

        public string Search { get; set; }

        /// <summary>
        /// Gets or sets uploaded bytes.
        /// </summary>
        public double UploadedData
        {
            get => _uploadedData;
            set {_uploadedData = value;}
        }

        /// <summary>
        /// Gets or sets downloaded bytes.
        /// </summary>
        public double DownloadedData
        {
            get => _downloadedData;
            set { _downloadedData = value; }
        }

        public HomeViewModel(ISDK sdk)
        {
            SDK = sdk;
            SDK.VpnConnectionStatusChanged += OnVpnConnectionStatusChanged;
            SDK.UserLocationStatusChanged += SdkOnUserLocationStatusChanged;

            SDK.DataTransferUpdate += OnDataTransferUpdate;
            var filter = this.WhenValueChanged(t => t.Search)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Select(BuildFilter);

            SDK.Locations
                .ToObservableChangeSet(x => x.Id)
                .Filter(filter)
                .ObserveOn(SynchronizationContext.Current)
                .Bind(_items)
                .DisposeMany()
                .Subscribe();

                Locations = CollectionViewSource.GetDefaultView(_items) as ListCollectionView;

        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            SelectedLocationItem = _items.FirstOrDefault();
        }

        private Func<ILocation, bool> BuildFilter(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return x => true;
            }

            return x =>
            {
                if (x is IBestAvailable bestAvailable)
                {
                    return bestAvailable.SearchName.ToUpperInvariant().Contains(searchText.ToUpperInvariant())
                           || bestAvailable.SearchName.ToUpperInvariant().Contains(searchText.ToUpperInvariant());
                }
                if (!x.HasNode())
                {
                    return false;
                }
                if (string.IsNullOrEmpty(searchText))
                {
                    return true;
                }
            
                return x.Id.CaseInsensitiveContains(searchText) || x.City.CaseInsensitiveContains(searchText) ||
                       x.Country.CaseInsensitiveContains(searchText) ||
                       x.SearchName.CaseInsensitiveContains(searchText);
            };
        }

        private void OnVpnConnectionStatusChanged(ISDK sender, ConnectionStatus previous, ConnectionStatus current)
        {
            if (current == ConnectionStatus.Connected || current == ConnectionStatus.Disconnected)
            {
                ConnectedState = current;
            }
        }

        private void OnDataTransferUpdate(ISDK sender, VpnSDK.DataTransferEventArgs args)
        {
            UploadedData = Math.Max(0, args.UploadedBytes / SpeedNormalization);
            DownloadedData = Math.Max(0, args.DownloadedBytes / SpeedNormalization);
        }

        public async Task ConnectFromLocation(ILocation row, object view, MouseButtonEventArgs args)
        {
            args.Handled = true;
            await SDK.InitiateConnection(row);
        }

        private void SdkOnUserLocationStatusChanged(ISDK sdk, OperationStatus status, NetworkGeolocation args)
        {
            if (!sdk.IsConnected)
            {
                VisibleLocation = Properties.Strings.Updating + Properties.Strings.ProgressSuffix;
                VisibleLocationFlag = null;
                IPAddress = null;
            }
            else
            {
                IPAddress = args?.IPAddress;

                if (args?.City != null && args?.Country != null)
                {
                    VisibleLocation = $"{args.City}, {args.Country}";
                    VisibleLocationFlag = args.CountryCode;
                }
                else if (args?.City == null && args?.Country != null)
                {
                    VisibleLocation = args.Country;
                    VisibleLocationFlag = args.CountryCode;
                }

            }
        }

        public string SelectedLocation { get; set; }

        public ILocation SelectedLocationItem
        {
            get => _selectedLocationItem;
            set
            {
                if (value == null) return;

                _selectedLocationItem = value;
                SDK.SetLocation(value);
            }
        }
    }
}
