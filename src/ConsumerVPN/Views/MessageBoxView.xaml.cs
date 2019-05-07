using System.Windows;
using System.Windows.Controls;

namespace WLVPN.Views
{
    /// <summary>
    /// Interaction logic for MessageBoxView.xaml
    /// </summary>
    public partial class MessageBoxView : UserControl
    {
        public MessageBoxView()
        {
            InitializeComponent();
        }

        private void MessageBoxView_OnLoaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Focus();
        }
    }
}
