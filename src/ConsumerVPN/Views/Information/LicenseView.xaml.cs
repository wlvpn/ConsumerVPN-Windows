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
using System.Windows.Resources;
using System.Windows.Shapes;
using WLVPN.Helpers;

namespace WLVPN.Views.Information
{
    /// <summary>
    /// Interaction logic for LicenseView.xaml
    /// </summary>
    public partial class LicenseView : UserControl
    {
        public LicenseView()
        {
            InitializeComponent();

            string licenseUri = Helpers.Resource.Get<string>("BRAND_LICENSE", "pack://application:,,,/Resources/licenses.rtf");
            StreamResourceInfo streamResourceInfo = Application.GetResourceStream(new Uri(licenseUri, UriKind.RelativeOrAbsolute));

            if (streamResourceInfo != null)
            {
                using (var stream = streamResourceInfo.Stream)
                {
                    TextRange textRange = new TextRange(LicenseRichBoxFlowDocument.ContentStart, LicenseRichBoxFlowDocument.ContentEnd);
                    textRange.Load(stream, System.Windows.DataFormats.Rtf);
                    textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Resource.Get<Brush>("IdealForegroundBrush"));
                }
            }
        }
    }
}
