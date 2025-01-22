using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WLVPN.Model;
using WLVPN.Utils;

namespace WLVPN.Interfaces
{
    /// <summary>
    /// WifiService Interface.
    /// </summary>
    public interface IWifiService
    {
        IReadOnlyList<WifiNetwork> Networks { get; }
        Task Refresh(CancellationToken cancellationToken);        
        event EventHandler NetworkChanged;       
    }
}
