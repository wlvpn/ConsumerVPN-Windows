using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WLVPN.Utils
{
    /// <summary>
    /// Class ClrBugUtility. Provides fixes for run-time issues.
    /// </summary>
    public static class ClrBugUtility
    {
        /// <summary>
        /// Invokes an exception and returns true. When invoked, this can force the full assembly to load and resolve.
        /// Useful in cases where modules are being loaded at incorrect times.
        /// </summary>
        /// <returns></returns>
        public static bool InvokeExceptionAndReturnTrue()
        {
            try
            {
                throw new InvalidOperationException();
            }
            catch
            {
                // Ignore.
            }

            return true;
        }
    }
}
