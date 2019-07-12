using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using WLVPN.Enums;
using WLVPN.Interfaces;
using WLVPN.Utils;

namespace WLVPN.ViewModels
{
    public class DialogConductorViewModel : PropertyChangedBase, IDialogManager, IConductActiveItem
    {
        private readonly IMessageBoxFactory _messageBoxFactory;
        private SAPIWrapper Voice { get; } = null;
        private IScreen _activeItem;
        public event EventHandler<ActivationProcessedEventArgs> ActivationProcessed = delegate { };
        public event EventHandler<bool> IsActiveChanged;


        public DialogConductorViewModel(IMessageBoxFactory messageBoxFactory, SAPIWrapper speech)
        {
            Voice = speech;
            _messageBoxFactory = messageBoxFactory;
        }

        public IScreen ActiveItem
        {
            get => _activeItem;
            set
            {
                _activeItem = value;
                IsActiveChanged?.Invoke(this, value != null);
                NotifyOfPropertyChange(() => ActiveItem);
            }
        }

        public IEnumerable GetChildren()
        {
            return ActiveItem != null ? new[] { ActiveItem } : Array.Empty<object>();
        }

        public void ActivateItem(object item)
        {
            ActiveItem = item as IScreen;

            if (ActiveItem is IChild child)
            {
                child.Parent = this;
            }

            ActiveItem?.Activate();

            ActivationProcessed(this, new ActivationProcessedEventArgs { Item = ActiveItem, Success = true });
        }

        public void DeactivateItem(object item, bool close)
        {
            if (item is IGuardClose guard)
            {
                guard.CanClose(result =>
                {
                    if (result)
                    {
                        CloseActiveItemCore();
                    }
                });
            }
            else
            {
                CloseActiveItemCore();
            }
        }

        object IHaveActiveItem.ActiveItem
        {
            get { return ActiveItem; }
            set { ActivateItem(value); }
        }

        public void ShowDialog(IScreen dialogModel, Action<IScreen> callback = null)
        {
            if (callback != null)
                dialogModel.Deactivated += delegate { callback(dialogModel); };
            ActivateItem(dialogModel);
        }

        public void ShowMessageBox(string message, string title = null, MessageBoxOptions options = MessageBoxOptions.Ok, Action<IMessageBox> callback = null)
        {
            IMessageBox box = _messageBoxFactory.CreateNew();
            box.DisplayName = title;
            box.Options = options;
            box.Message = message;
            Voice.Speak(message);

            if (callback != null)
                box.Deactivated += delegate { callback(box); };

            ActivateItem(box);
        }

        void CloseActiveItemCore()
        {
            IScreen oldItem = ActiveItem;
            ActivateItem(null);
            oldItem?.Deactivate(true);
        }

        IEnumerable IParent.GetChildren()
        {
            throw new NotImplementedException();
        }
    }
}
