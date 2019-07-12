using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WLVPN.Utils
{
    public sealed class InterfaceTemplateSelector : DataTemplateSelector
    {
        private Dictionary<Type, DataTemplate> TemplateCache = new Dictionary<Type, DataTemplate>();

        /// <inheritdoc/>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement containerElement = container as FrameworkElement;

            if (null == item || null == containerElement)
                return base.SelectTemplate(item, container);

            Type itemType = item.GetType();

            if (TemplateCache.ContainsKey(itemType))
            {
                return TemplateCache[itemType];
            }

            IEnumerable<Type> dataTypes = itemType.GetInterfaces();

            DataTemplate template
                = dataTypes.Select(t => new DataTemplateKey(t))
                    .Select(containerElement.TryFindResource)
                    .OfType<DataTemplate>()
                    .FirstOrDefault();

            if (template != null)
            {
                TemplateCache.Add(itemType, template);
            }

            return template ?? base.SelectTemplate(item, container);
        }
    }
}