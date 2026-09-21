using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WLVPN.Views
{
    /// <summary>
    /// Interaction logic for DedicatedIpView.xaml
    /// </summary>
    public partial class DedicatedIpView : UserControl
    {
        public DedicatedIpView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Expanding is deliberately independent of row selection: the row style already forces
        /// IsSelected on focus, and collapsing through selection would clear the connect target.
        /// </summary>
        private void Expander_Click(object sender, RoutedEventArgs e)
        {
            Toggle(FindAncestor<DataGridRow>((DependencyObject)sender));
            e.Handled = true;
        }

        /// <summary>
        /// Tab walks the rows, but the expander is not in the tab order, so Left and Right
        /// drive it: Right expands and then steps in, Left steps back out and then collapses,
        /// matching how a tree behaves.
        /// </summary>
        private void DedicatedIpDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Left && e.Key != Key.Right)
            {
                return;
            }

            DependencyObject focused = Keyboard.FocusedElement as DependencyObject;
            DataGridRow row = FindAncestor<DataGridRow>(focused);

            if (row == null)
            {
                return;
            }

            bool insideServerList = FindAncestor<ListBoxItem>(focused) != null;

            if (e.Key == Key.Right)
            {
                if (insideServerList)
                {
                    return;
                }

                if (row.DetailsVisibility == Visibility.Visible)
                {
                    FindDescendant<ListBoxItem>(row)?.Focus();
                }
                else
                {
                    row.DetailsVisibility = Visibility.Visible;
                }
            }
            else if (insideServerList)
            {
                row.Focus();
            }
            else
            {
                row.DetailsVisibility = Visibility.Collapsed;
            }

            e.Handled = true;
        }

        private static void Toggle(DataGridRow row)
        {
            if (row != null)
            {
                row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        private static T FindAncestor<T>(DependencyObject item)
            where T : DependencyObject
        {
            for (DependencyObject current = item; current is Visual; current = VisualTreeHelper.GetParent(current))
            {
                if (current is T match)
                {
                    return match;
                }
            }

            return null;
        }

        private static T FindDescendant<T>(DependencyObject parent)
            where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T match)
                {
                    return match;
                }

                T found = FindDescendant<T>(child);

                if (found != null)
                {
                    return found;
                }
            }

            return null;
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
