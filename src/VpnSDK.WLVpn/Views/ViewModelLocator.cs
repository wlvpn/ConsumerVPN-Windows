// <copyright file="ViewModelLocator.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;

using SimpleInjector;
using VpnSDK.WLVpn.ViewModels;

namespace VpnSDK.WLVpn.Views
{
    /// <summary>
    /// ViewModelLocator allows a view to resolve what view model to use at design and run time.
    /// </summary>
    public class ViewModelLocator
    {
        private DependencyObject _dummy = new DependencyObject();

        /// <summary>
        /// The resolver dictionary allows the view to look up the view model.  Instead of using magic strings, the relationship between
        /// the view and the view model is explictly defined in the list below
        /// the tuple is one of View name, View Mode Type and the Design Time View Model type.
        /// all of the design time view models are the same as the run time for now, but if you
        /// want to add a design time view model resolution that is different that the runtime view model
        /// this is where you would do it.
        /// </summary>
        private Dictionary<string, Tuple<Type, Type>> _viewModelsResolver = new Dictionary<string, Tuple<Type, Type>>
        {
            // view name, tuple of < runtime viewmodel type, design time viewmodel type >
            { "MainView",                    new Tuple<Type, Type>(typeof(MainViewModel),                    typeof(MainViewModel)) },
            { "DisconnectedView",            new Tuple<Type, Type>(typeof(DisconnectedViewModel),            typeof(DisconnectedViewModel)) },
            { "ConnectedView",               new Tuple<Type, Type>(typeof(ConnectedViewModel),               typeof(ConnectedViewModel)) },
            { "BasicDialogView",             new Tuple<Type, Type>(typeof(BasicDialogViewModel),             typeof(BasicDialogViewModel)) },
            { "LoginView",                   new Tuple<Type, Type>(typeof(LoginViewModel),                   typeof(LoginViewModel)) },
            { "LocationListView",            new Tuple<Type, Type>(typeof(LocationListViewModel),            typeof(LocationListViewModel)) },
            { "SettingsView",                new Tuple<Type, Type>(typeof(SettingsViewModel),                typeof(SettingsViewModel)) },
            { "SettingsAboutView",           new Tuple<Type, Type>(typeof(SettingsAboutViewModel),           typeof(SettingsAboutViewModel)) },
            { "SettingsConnectionsView",     new Tuple<Type, Type>(typeof(SettingsConnectionsViewModel),     typeof(SettingsConnectionsViewModel)) },
            { "SettingsGeneralView",         new Tuple<Type, Type>(typeof(SettingsGeneralViewModel),         typeof(SettingsGeneralViewModel)) },
            { "SettingsLicensesView",        new Tuple<Type, Type>(typeof(SettingsLicensesViewModel),        typeof(SettingsLicensesViewModel)) },
            { "SettingsLogView",             new Tuple<Type, Type>(typeof(SettingsLogViewModel),             typeof(SettingsLogViewModel)) },
            { "IsBusyView",                  new Tuple<Type, Type>(typeof(IsBusyViewModel),                  typeof(IsBusyViewModel)) },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelLocator"/> class.
        /// </summary>
        public ViewModelLocator()
        {
            Container = App.ContainerInstance;

            ResolveViewModel = (key) =>
            {
                // this is the logic used to determine the view model to use for the view
                // it looks it up by the view name.
                if (_viewModelsResolver.ContainsKey(key))
                {
                    if (!IsInDesignMode())
                    {
                        return Container.GetInstance(_viewModelsResolver[key].Item1);
                    }

                    return Container.GetInstance(_viewModelsResolver[key].Item2);
                }

                return null;
            };
        }

        // the following methods are referenced in the View XAML.
        // you will see something like the following line
        //        DataContext="{Binding Path=SettingsConnectionsView, Source={StaticResource ViewModelLocator}}"
        // somewhere in the UserControl initial properties settings in the xaml file.

        /// <summary>
        /// Gets the proper view model for the MainView
        /// </summary>
        public object MainView
        {
            get { return GetViewModel(); }
        }

        /// <summary>
        /// Gets the proper view model for the DisconnectedView
        /// </summary>
        public object DisconnectedView
        {
            get { return GetViewModel(); }
        }

        /// <summary>
        /// Gets the proper view model for the ConnectedView
        /// </summary>
        public object ConnectedView
        {
            get { return GetViewModel(); }
        }

        /// <summary>
        /// Gets the proper view model for the BasicDialogView
        /// </summary>
        public object BasicDialogView
        {
            get { return GetViewModel(); }
        }

        /// <summary>
        /// Gets the proper view model for the LoginView
        /// </summary>
        public object LoginView
        {
            get { return GetViewModel(); }
        }

        /// <summary>
        /// Gets the proper view model for the LocationListView
        /// </summary>
        public object LocationListView
        {
            get { return GetViewModel(); }
        }

        /// <summary>
        /// Gets the proper view model for the SettingsAboutView
        /// </summary>
        public object SettingsAboutView
        {
            get { return GetViewModel(); }
        }

        /// <summary>
        /// Gets the proper view model for the SettingsView
        /// </summary>
        public object SettingsView
        {
            get { return GetViewModel(); }
        }

        /// <summary>
        /// Gets the proper view model for the SettingsConnectionsView
        /// </summary>
        public object SettingsConnectionsView
        {
            get { return GetViewModel(); }
        }

        /// <summary>
        /// Gets the proper view model for the SettingsGeneralView
        /// </summary>
        public object SettingsGeneralView
        {
            get { return GetViewModel(); }
        }

        /// <summary>
        /// Gets the proper view model for the SettingsLicensesView
        /// </summary>
        public object SettingsLicensesView
        {
            get { return GetViewModel(); }
        }

        /// <summary>
        /// Gets the proper view model for the SettingsLogView
        /// </summary>
        public object SettingsLogView
        {
            get { return GetViewModel(); }
        }

        /// <summary>
        /// Gets the proper view model for the IsBusyView
        /// </summary>
        public object IsBusyView
        {
            get { return GetViewModel(); }
        }

        /// <summary>
        /// Gets or sets a value indicating the view model resolver logic to use.
        /// if you want to change how the view models are resolved, provide you own function here.  Just return the right
        /// VM based upon the name of the View given as the key.
        /// </summary>
        public Func<string, object> ResolveViewModel { get; set; }

        /// <summary>
        /// Gets the current Dependency Injection container.  This is used to create an instance of the view model.
        /// </summary>
        private Container Container { get; } = null;

        /// <summary>
        /// THis method returns the view model using the defined resolver method.
        /// </summary>
        /// <param name="key">The name of the view to get a view model for</param>
        /// <returns>The proper view model for the current runtime environment</returns>
        public object GetViewModel([CallerMemberName] string key = null)
        {
            return ResolveViewModel(key);
        }

        /// <summary>
        /// This method is used to determine if the class is being invoked at runtime or during a Visual Studio design session.
        /// </summary>
        /// <returns>true if the class is being executed in the designer</returns>
        private bool IsInDesignMode()
        {
            return System.ComponentModel.DesignerProperties.GetIsInDesignMode(_dummy);
        }
    }
}
