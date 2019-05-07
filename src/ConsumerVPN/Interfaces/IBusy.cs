using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WLVPN.Interfaces
{
    public interface IBusyManager
    {
        bool IsActive { get; }

        void Activate(string busyText, Action callback = null);

        Task ActivateWithDelay(string busyText, TimeSpan delayPeriod, TimeSpan interval, Action intervalAction, Action callback = null);

        void ChangeBusyText(string text);

        TimeSpan DelayRemained { get; }

        void Dismiss();

        event EventHandler<bool> IsActiveChanged;
    }
}