using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WLVPN.Utils
{
    public class ExtendedImage : Image
    {

        public static readonly DependencyProperty BrushProperty = DependencyProperty.RegisterAttached(
            "Brush",
            typeof(SolidColorBrush),
            typeof(ExtendedImage),
            new PropertyMetadata(new SolidColorBrush(Colors.White))
        );

        public static void SetBrush(UIElement element, SolidColorBrush value)
        {
            element.SetValue(BrushProperty, value);
        }

        public static SolidColorBrush GetBrush(UIElement element)
        {
            return (SolidColorBrush)element.GetValue(BrushProperty);
        }
    }
}
