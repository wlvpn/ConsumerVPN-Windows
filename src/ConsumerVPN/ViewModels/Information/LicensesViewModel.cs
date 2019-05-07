using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using WLVPN.Extensions;
using WLVPN.Helpers;
using WLVPN.Interfaces;
using WLVPN.Views.Information;

namespace WLVPN.ViewModels.Information
{
    public class LicenseViewModel : Screen, IInformationTabItem
    {
        public static Style Icon => Resource.Get<Style>("LicenseIcon");

        public string TabHeaderTitle => Properties.Strings.TabInformationLicenses;
    }
}
