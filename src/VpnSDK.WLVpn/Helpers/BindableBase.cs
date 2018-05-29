// <copyright file="BindableBase.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace VpnSDK.WLVpn.Helpers
{
    /// <summary>
    /// Class BindableBase. A base class that implements helper methods for INotifyPropertyChanged.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public abstract class BindableBase : INotifyPropertyChanged
    {
        private DependencyObject _dummy = new DependencyObject();
        private bool _selected = false;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets a value indicating whether the derived object is selected.
        /// useful for lists of object swith multi-select
        /// </summary>
        public bool IsSelected
        {
            get { return _selected; }
            set { SetProperty(ref _selected, value); }
        }

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            CommandManager.InvalidateRequerySuggested();
            return true;
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            var eventHandler = PropertyChanged;
            if (eventHandler != null)
            {
                RunOnDisplayThread(() =>
                {
                    eventHandler(this, new PropertyChangedEventArgs(propertyName));
                });
            }
        }

        /// <summary>
        /// This method will cause the action to be executed on the UI thread.
        /// </summary>
        /// <param name="actionToExecute">The action to perform</param>
        protected void RunOnDisplayThread(Action actionToExecute)
        {
            var dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;

            if (dispatcher != null && dispatcher.CheckAccess() == false)
            {
                dispatcher.BeginInvoke(actionToExecute);
            }
            else
            {
                actionToExecute();
            }
        }

        /// <summary>
        /// This method is used to determine if the class is being invoked at runtime or during a Visual Studio design session.
        /// </summary>
        /// <returns>true if the class is being executed in the designer</returns>
        protected bool IsInDesignMode()
        {
            return DesignerProperties.GetIsInDesignMode(_dummy);
        }
    }
}