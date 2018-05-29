// <copyright file="BasicDialogViewModel.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using VpnSDK.WLVpn.Events;
using VpnSDK.WLVpn.Helpers;

namespace VpnSDK.WLVpn.ViewModels
{
    /// <summary>
    /// Class BasicDialogViewModel. Provides the View Model for a pop up dialog.
    /// </summary>
    /// <seealso cref="BindableBase" />
    public class BasicDialogViewModel : BindableBase
    {
        private EventAggregator _eventAggregator;
        private DialogAction _action = null;
        private RelayCommand _okCmd = null;
        private RelayCommand _cancelCmd = null;
        private RelayCommand _otherCmd = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicDialogViewModel"/> class.
        /// </summary>
        /// <param name="eventAggregator">The event aggregator.</param>
        public BasicDialogViewModel(EventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        /// Gets or sets what the actions of the dialog are.
        /// </summary>
        /// <value>A <see cref="DialogAction"/> describing the potential information/options to display.</value>
        public DialogAction WhatToDo
        {
            get { return _action; }
            set { SetProperty(ref _action, value); }
        }

        /// <summary>
        /// Gets the command to execute when OK is pressed.
        /// </summary>
        /// <value>The OK command.</value>
        public RelayCommand OKCmd
        {
            get
            {
                if (_okCmd == null)
                {
                    _okCmd = new RelayCommand(
                        (parm) =>
                        {
                            if (_action != null)
                            {
                                _action.OKAction();
                            }
                            _eventAggregator.Publish<ShowDialogEvent>(new ShowDialogEvent { Show = false });
                        },
                        (parm) => true);
                }

                return _okCmd;
            }
        }

        /// <summary>
        /// Gets the command to execute when cancel is pressed.
        /// </summary>
        /// <value>The cancel command.</value>
        public RelayCommand CancelCmd
        {
            get
            {
                if (_cancelCmd == null)
                {
                    _cancelCmd = new RelayCommand(
                        (parm) =>
                        {
                            _action?.CancelAction();
                            _eventAggregator.Publish<ShowDialogEvent>(new ShowDialogEvent { Show = false });
                        },
                        (parm) => true);
                }

                return _cancelCmd;
            }
        }

        /// <summary>
        /// Gets the command for a user defined action (known as Other command).
        /// </summary>
        /// <value>The other command.</value>
        public RelayCommand OtherCmd
        {
            get
            {
                if (_otherCmd == null)
                {
                    _otherCmd = new RelayCommand(
                        (parm) =>
                        {
                            _action?.OtherAction();
                            _eventAggregator.Publish<ShowDialogEvent>(new ShowDialogEvent { Show = false });
                        },
                        (parm) => true);
                }

                return _otherCmd;
            }
        }
    }
}