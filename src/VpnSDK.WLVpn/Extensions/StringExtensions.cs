// <copyright file="StringExtensions.cs" company="StackPath, LLC">
// Copyright (c) StackPath, LLC. All Rights Reserved.
// </copyright>

using System;
using System.Security.Cryptography;
using System.Text;

namespace VpnSDK.WLVpn.Extensions
{
    /// <summary>
    /// Class StringExtensions. Provides utilities for <see cref="string"/> objects.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// The extra entropy used for encrypting strings. This ensures that despite using the current user, we provide a salt to make the encryption also application specific.
        /// For developers wishing to implement this class, we suggest providing your own additional entropy. For developers looking to have fun, this is a chance
        /// to convert some of your favorite lyrics, a message to a loved one or even a in-joke within your team, the additional entropy bytes won't display to the end user.
        /// In this example implementation, we used Metallica lyrics. Just have fun with this, it will _not_ reduce your security for this to be predetermined and it also must be.
        /// </summary>
        // TODO: To the developer reading this, make your own "entropy"! Read the summary above.
        private static byte[] _saltEntropy =
            {
            70, 111, 114, 101, 118, 101, 114, 32, 116, 114, 117, 115, 116, 105, 110, 103, 32, 119, 104, 111, 32, 119, 101, 32, 97, 114, 101,
            32, 32, 65, 110, 100, 32, 110, 111, 116, 104, 105, 110, 103, 32, 101, 108, 115, 101, 32, 109, 97, 116, 116, 101, 114, 115
            };

        /// <summary>
        /// Protects/encrypts the specified text unique to the application and local user.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>An encrypted string.</returns>
        public static string Protect(this string text)
        {
            return Convert.ToBase64String(ProtectedData.Protect(Encoding.Default.GetBytes(text), _saltEntropy, DataProtectionScope.CurrentUser));
        }

        /// <summary>
        /// Unprotects/decrypts the specified text unique to the application and specific user.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>An unencrypted string.</returns>
        public static string Unprotect(this string text)
        {
            if (text == null)
            {
                return null;
            }

            string result;

            try
            {
                result = Encoding.Default.GetString(ProtectedData.Unprotect(
                    Convert.FromBase64String(text), _saltEntropy, DataProtectionScope.CurrentUser));
            }
            catch (Exception)
            {
                return null;
            }

            return result;
        }
    }
}