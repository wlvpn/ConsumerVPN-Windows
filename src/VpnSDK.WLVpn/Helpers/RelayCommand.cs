// <copyright file="RelayCommand.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;

namespace VpnSDK.WLVpn.Helpers
{
    /// <summary>
    /// Class RelayCommand. Represents a command class that allows the user to define the logic to perfom via a user defined <see cref="Action"/>.
    /// </summary>
    public class RelayCommand : ICommand, INotifyPropertyChanged, IDisposable
    {
        private Action<object> _execute;

        private Func<object, bool> _canExecute;

        private bool _isExecutable = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <exception cref="ArgumentNullException">If the execute delegate is null.</exception>
        public RelayCommand(Action<object> execute)
            : this(execute, canExecute: (obj) => true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <exception cref="ArgumentNullException">If the execute delegate is null.</exception>
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        /// <summary>
        /// The property changed event. Raises whenever a property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }

            remove
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the relay command can be executed.
        /// </summary>
        /// <returns>a bool</returns>
        public bool IsExecutable
        {
            get
            {
                return _isExecutable;
            }

            set
            {
                _isExecutable = value;
                RaisePropertyChanged("IsExecutable");
            }
        }

        /// <summary>
        /// This method will cause the action to be executed on the UI thread.
        /// </summary>
        /// <param name="actionToExecute">The action to perform</param>
        public static void RunOnDisplayThread(Action actionToExecute)
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
        /// Raises the <see cref="CanExecuteChanged" /> event.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This cannot be an event")]
        public void RaiseCanExecuteChanged()
        {
            RunOnDisplayThread(() =>
            {
                CommandManager.InvalidateRequerySuggested();
                RaisePropertyChanged("IsExecutable");
            });
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">This parameter will always be ignored.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            IsExecutable = _canExecute(parameter);
            return IsExecutable;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">This parameter will always be ignored.</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// This method is called to clean up any references
        /// </summary>
        public void Dispose()
        {
            _execute = null;
            _canExecute = null;
            PropertyChanged = null;
        }

        /// <summary>
        /// This method is used to signal a change in one of the properties of the command
        /// </summary>
        /// <param name="name">the property to raise an event for</param>
        private void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}