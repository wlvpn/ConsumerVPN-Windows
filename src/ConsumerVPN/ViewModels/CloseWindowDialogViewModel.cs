using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using WLVPN.Enums;

namespace WLVPN.ViewModels
{
    public class CloseWindowDialogViewModel : Screen
    {
        private CloseDialogOptions _selection;

        public CloseWindowDialogViewModel()
        {

        }

        public void Quit()
        {
            Select(CloseDialogOptions.Quit);
        }

        public void Hide()
        {
            Select(CloseDialogOptions.Hide);
        }

        void Select(CloseDialogOptions option)
        {
            _selection = option;
            TryClose();
        }

        public bool WasSelected(CloseDialogOptions option)
        {
            return (_selection & option) == option;
        }
    }
}
