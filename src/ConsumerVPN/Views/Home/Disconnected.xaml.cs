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

namespace WLVPN.Views.Home
{
    /// <summary>
    /// Interaction logic for Disconnected.xaml
    /// </summary>
    public partial class Disconnected : UserControl
    {
        public Disconnected()
        {
            InitializeComponent();
        }

        private void Search_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            searchStackPanel.Visibility = Visibility.Hidden;
        }

        private void Search_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(Search.Text))
            {
                searchStackPanel.Visibility = Visibility.Visible;
            }
        }
    }
}
