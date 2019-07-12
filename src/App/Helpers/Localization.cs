using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WLVPN;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

public static class Localization
{
    public static T GetValue<T>(string key)
    {
        return LocExtension.GetLocalizedValue<T>(key, LocalizeDictionary.Instance);
    }

    public static string Get(string key)
    {
        return LocExtension.GetLocalizedValue<string>(key, LocalizeDictionary.Instance);
    }

}