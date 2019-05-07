using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WLVPN.Views.Login
{
    /// <summary>
    /// Interaction logic for NotAuthenticated.xaml
    /// </summary>
    public partial class NotAuthenticated : UserControl
    {
        public NotAuthenticated()
        {
            Loaded += NotAuthenticated_Loaded;
            InitializeComponent();
        }

        private void NotAuthenticated_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(Username);
        }
    }
}
