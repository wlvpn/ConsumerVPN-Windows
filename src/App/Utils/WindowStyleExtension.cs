using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WLVPN.Utils
{
    public class WindowStyleExtension : Window
    {
        public static readonly DependencyProperty SecondTitleProperty =
            DependencyProperty.RegisterAttached("SecondTitle", typeof(string), typeof(WindowStyleExtension), new PropertyMetadata(string.Empty));

        public static void SetSecondTitle(DependencyObject element, string value)
        {
            element.SetValue(SecondTitleProperty, value);
        }

        public static string GetSecondTitle(DependencyObject element)
        {
            return (string)element.GetValue(SecondTitleProperty);
        }
    }
}
