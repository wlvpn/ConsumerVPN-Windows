using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WLVPN.Views
{
    /// <summary>
    /// Interaction logic for CloseWindowDialogView.xaml
    /// </summary>
    public partial class CloseWindowDialogView : UserControl
    {
        public CloseWindowDialogView()
        {
            InitializeComponent();
        }

        private void CloseWindowDialogView_OnLoaded(object sender, RoutedEventArgs e)
        {
            Focus();
        }
    }
}
