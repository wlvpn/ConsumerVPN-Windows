using DynamicData.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VpnSDK.Interfaces;

namespace WLVPN.Extensions
{
    public static class LocationExtension
    {
        private static Dictionary<Type, PropertyInfo> PropInfo = new Dictionary<Type, PropertyInfo>();

        public static bool HasNode(this ILocation location)
        {
            if (ReferenceEquals(location, null))
            {
                return false;
            }

            bool? result = CheckCachedPropertyInfo(location);

            if (result.HasValue)
                return result.Value;

            PropertyInfo propInfo = location.GetType().GetProperty("HasNode", BindingFlags.Instance | BindingFlags.NonPublic);
            PropInfo.Add(location.GetType(), propInfo);

            return CheckCachedPropertyInfo(location).ValueOr(false);
        }

        internal static bool? CheckCachedPropertyInfo(ILocation location)
        {
            if (PropInfo.TryGetValue(location.GetType(), out var propertyInfo))
            {
                if (propertyInfo == null)
                {
                    return string.IsNullOrEmpty(location?.Id);
                }
                return (bool)propertyInfo.GetValue(location);
            }

            return null;
        }
    }
}