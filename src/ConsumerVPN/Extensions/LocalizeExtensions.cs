using PropertyChanged;
using System;
using System.Windows.Markup;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;
using XAMLMarkupExtensions.Base;

namespace WLVPN.Extensions
{
    [DoNotNotify]
    [MarkupExtensionReturnType(typeof(string))]
    public class LocProgressExtension : LocExtension
    {
        public LocProgressExtension()
        {
        }

        public LocProgressExtension(string key) : base(key)
        {
        }

        protected virtual string FormatText(string target)
        {
            return target ?? string.Empty;
        }

        public override object FormatOutput(TargetInfo endPoint, TargetInfo info)
        {
            var textMain = base.FormatOutput(endPoint, info) as string ?? string.Empty;
            return $"{textMain}...";
        }
    }
}