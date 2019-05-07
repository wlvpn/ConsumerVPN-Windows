
using Caliburn.Micro;
using WLVPN.Enums;

namespace WLVPN.Interfaces
{
    public interface IMessageBox : IScreen
    {
        string Message { get; set; }
        MessageBoxOptions Options { get; set; }

        void Ok();
        void Cancel();
        void Yes();
        void No();

        bool WasSelected(MessageBoxOptions messageBoxOptions);
    }
}
