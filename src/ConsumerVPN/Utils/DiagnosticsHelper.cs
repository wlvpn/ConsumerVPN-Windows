using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace WLVPN.Utils
{
    internal static class DiagnosticsHelper
    {
        /// <summary>
        /// Returns true if Windows 64-bit Environment else it will return
        /// false. 
        /// </summary>
        /// <returns>bool</returns>
        public static bool Is64Bit()
        {
            return IntPtr.Size == 8 ? true : false;
        }

        /// <summary>
        /// Return true if Windows Vista.
        /// </summary>
        /// <returns>bool</returns>
        public static bool IsWindowsVista()
        {
            return Environment.OSVersion.Version.ToString().StartsWith("6.0.", true, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns the full name and version of the currently running Windows Operating System
        /// </summary>
        /// <returns>string</returns>
        public static string GetOperatingSystemFullName()
        {
            string result;
            try
            {
                result = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get()
                    .Cast<ManagementObject>()
                    .FirstOrDefault()?.GetPropertyValue("Caption").ToString();
            }
            catch
            {
                result = Environment.OSVersion.VersionString;
            }
            return result;
        }

        /// <summary>
        /// Returns the version (Major.Minor.BuildNumber) of the currently running Windows OS. 
        /// </summary>
        /// <returns>string</returns>
        public static string GetOperatingSystemVersion()
        {
            string result;
            try
            {

                result = new ManagementObjectSearcher("SELECT Version FROM Win32_OperatingSystem").Get()
                    .Cast<ManagementObject>().FirstOrDefault()?.GetPropertyValue("Version").ToString();
            }
            catch
            {
                result = Environment.OSVersion.Version.ToString();
            }
            return result;
        }

        public static string GetSettingsStateInfo()
        {
            // Maybe return a json object?
            StringBuilder info = new StringBuilder();
            info.AppendLine("---- Settings State Start ----");

            foreach (SettingsProperty setting in Properties.Settings.Default.Properties)
            {
                if (setting.Name.Equals("Password", StringComparison.InvariantCulture) || setting.Name.Equals("Username", StringComparison.InvariantCulture))
                {
                    continue;
                }
                info.AppendLine($"{setting.Name} = {Properties.Settings.Default.PropertyValues[setting.Name]?.PropertyValue?.ToString()}"); 
            }
            info.AppendLine("---- Settings State End ----");

            return info.ToString();
        }

    }
}
