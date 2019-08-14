using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WLVPN.Views
{
    /// <summary>
    /// Interaction logic for BusyView.xaml
    /// </summary>
    public partial class BusyView : UserControl
    {
        public BusyView()
        {
            InitializeComponent();
        }

        private void Busy_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible && this.IsLoaded)
            {
                Busy.Focus();
            }
        }
    }
}
