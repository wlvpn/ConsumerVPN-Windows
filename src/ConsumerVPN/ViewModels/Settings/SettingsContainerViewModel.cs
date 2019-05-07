using Caliburn.Micro;
using WLVPN.Helpers;
using WLVPN.Interfaces;
using System.Collections.Generic;
using System.Windows;

namespace WLVPN.ViewModels
{
    internal class SettingsContainerViewModel : Conductor<ISettingsTabItem>.Collection.OneActive, IMainScreenTabItem
    {
        public Style Icon => Resource.Get<Style>("SettingsIcon");

        public string Title => Properties.Strings.TabSettings;

        public SettingsContainerViewModel(IEnumerable<ISettingsTabItem> tabs)
        {
            Items.AddRange(tabs);
        }
    }
}