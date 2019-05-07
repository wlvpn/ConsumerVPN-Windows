using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VpnSDK.Interfaces;

namespace WLVPN.ViewModels
{
    public class LocationTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the template to use for a region.
        /// </summary>
        public DataTemplate RegionTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template to use for the Best Available row.
        /// </summary>
        public DataTemplate BestAvailableTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template to use for when no location is selected.
        /// </summary>
        public DataTemplate NullTemplate { get; set; }

        /// <summary>
        /// Returns the proper template for the given data type.
        /// </summary>
        /// <param name="item">The object we need a data template for.</param>
        /// <param name="container">Where the template will be used.</param>
        /// <returns>The correct data template.</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is IBestAvailable)
            {
                return BestAvailableTemplate;
            }

            if (item is IRegion)
            {
                return RegionTemplate;
            }

            return NullTemplate;
        }
    }
}