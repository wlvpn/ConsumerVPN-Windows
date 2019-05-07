using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WLVPN.Extensions
{
    public static class ProcessExtensions
    {
        public static Process SafeStart(string launchPath)
        {
            try
            {
                return Process.Start(launchPath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Process LaunchUrl(Uri url)
        {
            try
            {
                return Process.Start(new ProcessStartInfo("explorer.exe", $"\"{url}\""));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
