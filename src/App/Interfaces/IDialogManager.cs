using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using WLVPN.Enums;

namespace WLVPN.Interfaces
{
    public interface IDialogManager
    {
        event EventHandler<bool> IsActiveChanged;

        void ShowDialog(IScreen dialogModel, Action<IScreen> callback = null);

        void ShowMessageBox(string message, string title = null, MessageBoxOptions options = MessageBoxOptions.Ok, Action<IMessageBox> callback = null);
    }
}
