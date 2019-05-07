using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace WLVPN.Utils
{
    public static class ClipboardHelper
    { 
        public static bool Set(string text, out string process)
        {
            try
            {
                Clipboard.SetDataObject(text, true);
                process = null;
                return true;
            }
            catch
            {
                try
                {
                    process = FindProcess(NativeMethods.GetOpenClipboardWindow()).ProcessName;
                    return false;
                }
                catch
                {
                    process = null;
                    return false;
                }
            }
        }

        private static Process FindProcess(IntPtr yourHandle)
        {
            foreach (Process p in Process.GetProcesses())
            {
                if (p.Handle == yourHandle)
                {
                    return p;
                }
            }

            return null;
        }
    }
}
