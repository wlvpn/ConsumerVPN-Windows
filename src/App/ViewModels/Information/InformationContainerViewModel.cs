using Caliburn.Micro;
using WLVPN.Helpers;
using WLVPN.Interfaces;
using System.Collections.Generic;
using System.Windows;

namespace WLVPN.ViewModels
{
    internal class InformationContainerViewModel : Conductor<IInformationTabItem>.Collection.OneActive, IMainScreenTabItem
    {
        public Style Icon => Resource.Get<Style>("InfoIcon");

        public string Title => Properties.Strings.TabInformation;

        public InformationContainerViewModel(IEnumerable<IInformationTabItem> tabs)
        {
            Items.AddRange(tabs);
        }
    }
}