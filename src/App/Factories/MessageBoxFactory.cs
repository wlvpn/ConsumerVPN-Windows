using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WLVPN.Interfaces;
using WLVPN.ViewModels;

namespace WLVPN.Factories
{
    public class MessageBoxFactory : IMessageBoxFactory
    {
        public IMessageBox CreateNew()
        {
            return new MessageBoxViewModel();
        }
    }
}
