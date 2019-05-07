using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WLVPN.Helpers;
using WLVPN.Interfaces;

namespace WLVPN.ViewModels.Information
{
    public class HelpViewModel : Screen, IInformationTabItem
    {
        public static Style Icon => Resource.Get<Style>("HelpIcon");

        public string TabHeaderTitle => Properties.Strings.TabInformationHelp;
    }
}
