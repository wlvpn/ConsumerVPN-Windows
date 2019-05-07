using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Serilog;

namespace WLVPN.Utils
{
    internal static class ProtectData
    {

        /// <summary>
        /// The entropy key for saving data. This ensures other applications cannot decrypt our file.
        /// </summary>
        private static byte[] _entropy = new byte[]
        {
            33, 44, 21, 14, 212, 27, 54, 38, 77, 19, 45, 130, 2, 202, 3, 40, 23, 44, 51, 23, 63, 0, 9,
            2, 0, 0, 34, 1, 124, 11, 80, 93, 26, 10, 10, 10, 23, 56, 21, 22, 244, 227, 34, 66, 12, 3, 0
        };

        public static string ProtectString(string plainText)
        {
            try
            {
                var text = Convert.ToBase64String(ProtectedData.Protect(Encoding.Default.GetBytes(plainText), _entropy, DataProtectionScope.CurrentUser));
                return text;
            }
            catch { }

            return null;
        }

        public static string UnprotectString(string encryptedText)
        {
            if (encryptedText == null)
            {
                return null;
            }

            string result;

            try
            {
                result = Encoding.Default.GetString(ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedText), _entropy, DataProtectionScope.CurrentUser));
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to decrypt data.");
                return null;
            }

            return result;
        }
    }
}
