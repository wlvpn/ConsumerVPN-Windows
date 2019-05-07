using DynamicData.Binding;
using WLVPN.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using VpnSDK;
using VpnSDK.Enums;
using VpnSDK.Interfaces;

namespace WLVPN.Helpers
{
    public class ValueChanged<T>
    {
        public T Previous { get; }
        public T Current { get; }

        public ValueChanged(T previous, T current)
        {
            Previous = previous;
            Current = current;
        }
    }

    public static class ObservableVPN
    {
        public static IObservable<ValueChanged<ConnectionStatus>> ObserveConnectionState(this ISDK sdk)
        {
            return Observable
                .FromEvent<SDKChangeEventHandler<ConnectionStatus>, ValueChanged<ConnectionStatus>>(onNextHandler => (ISDK s, ConnectionStatus previous, ConnectionStatus current) => onNextHandler(new ValueChanged<ConnectionStatus>(previous, current)), h => sdk.VpnConnectionStatusChanged += h, h => sdk.VpnConnectionStatusChanged -= h);
        }

        public static IObservable<NetworkInterface> ObserveVPNAdapter(this ISDK sdk, bool completeOnDisconnect = true)
        {
            return Observable.Create<NetworkInterface>((subject) =>
            {
                var connectionObservable = sdk.ObserveConnectionState().Subscribe(x =>
                {
                    if (x.Current == ConnectionStatus.Connected)
                    {
                        subject.OnNext(NetworkInterfaceExtensions.GetVpnInterface());
                    }

                    if (x.Current != ConnectionStatus.Connected && x.Previous == ConnectionStatus.Connected)
                    {
                        if (completeOnDisconnect)
                        {
                            subject.OnCompleted();
                        }
                        subject.OnNext(null);
                    }
                });

                return Disposable.Create(() => connectionObservable.Dispose());
            });
        }
    }
}