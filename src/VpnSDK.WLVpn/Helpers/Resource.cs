// <copyright file="Resource.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Windows;

namespace VpnSDK.WLVpn.Helpers
{
    /// <summary>
    /// Class Resource. Provides static helper methods to get values from the application-wide combined <see cref="ResourceDictionary"/>.
    /// </summary>
    public static class Resource
    {
        /// <summary>
        /// Gets the specified resource name from the application-wise <see cref="ResourceDictionary"/>.
        /// </summary>
        /// <typeparam name="T">The expected resource type.</typeparam>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="defaultValue">The default value if unable to find or convert.</param>
        /// <returns>T.</returns>
        public static T Get<T>(string resourceName, T defaultValue = default(T))
        {
            ResourceDictionary currentResourceDictionary = Application.Current.Resources;
            if (currentResourceDictionary.Contains(resourceName))
            {
                object result = currentResourceDictionary[resourceName];
                try
                {
                    if (result is T variable)
                    {
                        return variable;
                    }

                    return (T)Convert.ChangeType(result, typeof(T));
                }
                catch
                {
                    // ignored
                }
            }

            return defaultValue;
        }
    }
}