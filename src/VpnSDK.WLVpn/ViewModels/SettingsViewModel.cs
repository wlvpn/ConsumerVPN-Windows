// <copyright file="SettingsViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System.Windows.Controls;

using SimpleInjector;
using VpnSDK.WLVpn.Events;
using VpnSDK.WLVpn.Helpers;
using VpnSDK.WLVpn.Views;

namespace VpnSDK.WLVpn.ViewModels
{
    /// <summary>
    /// Class SettingsViewModel. Provides the View Model for the <see cref="SettingsView"/>.
    /// </summary>
    /// <seealso cref="BindableBase" />
    public class SettingsViewModel : BindableBase
    {
        private EventAggregator _eventAggregator = null;
        private SettingsLogViewModel _logVM = null;
        private RelayCommand _saveCmd = null;
        private RelayCommand _cancelCmd = null;
        private ViewDefinition _selectedTab = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        /// <param name="sdk">The instance of the <see cref="SDKMonitor"/> to use.</param>
        /// <param name="eventAggregator">The instance of the <see cref="EventAggregator"/> to be bound to.</param>
        public SettingsViewModel(SDKMonitor sdk, EventAggregator eventAggregator)
        {
            InitViews();

            if (IsInDesignMode())
            {
                Init();
                return;
            }

            _eventAggregator = eventAggregator;
            SDKMonitor = sdk;
        }

        /// <summary>
        /// Gets the command to execute when Save is pressed.
        /// </summary>
        public RelayCommand SaveCmd
        {
            get
            {
                if (_saveCmd == null)
                {
                    _saveCmd = new RelayCommand(
                        (parm) =>
                        {
                            Save();
                        },
                        (parm) => true);
                }

                return _saveCmd;
            }
        }

        /// <summary>
        /// Gets the command to execute when Cancel is pressed.
        /// </summary>
        public RelayCommand CancelCmd
        {
            get
            {
                if (_cancelCmd == null)
                {
                    _cancelCmd = new RelayCommand(
                        (parm) =>
                        {
                            Cancel();
                        },
                        (parm) => true);
                }

                return _cancelCmd;
            }
        }

        /// <summary>
        /// Gets or sets the selected settings tab.
        /// </summary>
        /// <value>The selected settings tab.</value>
        public ViewDefinition SelectedTabItem
        {
            get
            {
                return _selectedTab;
            }

            set
            {
                SetProperty(ref _selectedTab, value);
                if (value != null)
                {
                    ViewDefinition viewDefinition = value as ViewDefinition;
                    if (viewDefinition == null)
                    {
                        return;
                    }

                    UserControl view = viewDefinition.View as UserControl;
                    if (view == null)
                    {
                        return;
                    }

                    _logVM = view.DataContext as SettingsLogViewModel;
                    if (_logVM != null)
                    {
                        _logVM.UpdateLogs();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the set of views available to be shown.
        /// </summary>
        public SmartCollection<ViewDefinition> TabViews { get; } = new SmartCollection<ViewDefinition>();

        private SDKMonitor SDKMonitor { get; }

        /// <summary>
        /// Invokes <see cref="ISettingsViewModel.Save"/> on all child <see cref="ISettingsViewModel"/>.
        /// </summary>
        public void Save()
        {
            // go to each tab on the settings view and tell them to save.
            foreach (ViewDefinition viewDef in TabViews)
            {
                UserControl uc = viewDef.View as UserControl;
                if (uc == null)
                {
                    continue;
                }

                ISettingsViewModel vm = uc.DataContext as ISettingsViewModel;
                if (vm == null)
                {
                    continue;
                }

                vm.Save();
            }

            Return();
        }

        /// <summary>
        /// Invokes <see cref="ISettingsViewModel.Cancel"/> on all child <see cref="ISettingsViewModel"/>.
        /// </summary>
        public void Cancel()
        {
            // go to each tab on the settings view and tell them to cancel.
            foreach (ViewDefinition viewDef in TabViews)
            {
                UserControl uc = viewDef.View as UserControl;
                if (uc == null)
                {
                    continue;
                }

                ISettingsViewModel vm = uc.DataContext as ISettingsViewModel;
                if (vm == null)
                {
                    continue;
                }

                vm.Cancel();
            }

            Return();
        }

        /// <summary>
        /// Initializes all the views used as tabs in this screen.
        /// </summary>
        private void InitViews()
        {
            Container ioc = App.ContainerInstance;
            TabViews.Add(new ViewDefinition { ID = Common.ViewList.Views.Settings, Title = Resources.Branding.Strings.SETTINGS_TAB_GENERAL, View = ioc.GetInstance<SettingsGeneralView>() });
            TabViews.Add(new ViewDefinition { ID = Common.ViewList.Views.Settings, Title = Resources.Branding.Strings.SETTINGS_TAB_CONNECTION, View = ioc.GetInstance<SettingsConnectionsView>() });
            TabViews.Add(new ViewDefinition { ID = Common.ViewList.Views.Settings, Title = Resources.Branding.Strings.SETTINGS_TAB_LOGS, View = ioc.GetInstance<SettingsLogView>() });
            TabViews.Add(new ViewDefinition { ID = Common.ViewList.Views.Settings, Title = Resources.Branding.Strings.SETTINGS_TAB_LICENSES, View = ioc.GetInstance<SettingsLicensesView>() });
            TabViews.Add(new ViewDefinition { ID = Common.ViewList.Views.Settings, Title = Resources.Branding.Strings.SETTINGS_TAB_ABOUT, View = ioc.GetInstance<SettingsAboutView>() });
        }

        /// <summary>
        /// Initialize any data needed at design time to help the Visual Studio designer display data.
        /// </summary>
        private void Init()
        {
        }

        /// <summary>
        /// Invokes an event that tells the main view to navigate back to the previous view.
        /// </summary>
        private void Return()
        {
            _eventAggregator.Publish<ShowViewEvent>(new ShowViewEvent { ID = Common.ViewList.Views.Back });
        }
    }
}