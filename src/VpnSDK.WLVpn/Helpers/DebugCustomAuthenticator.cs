using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VpnSDK.WLVpn.Interfaces;

namespace VpnSDK.WLVpn.Helpers
{
    public class DebugCustomAuthenticator : ICustomAuthenticator
    {
        public async Task<Tuple<string, string>> Login(string username, string password)
        {
            /* The following is an example of a custom authenticator. Uncomment it and put in your own logic if you need pre-authentication against your own API.
            var messageboxResult = await Task.Run(() => MessageBox.Show($"User {username} is trying to login. Do you want to allow it?", "Debug Authenticator", MessageBoxButton.YesNo));

            if (messageboxResult == MessageBoxResult.No)
            {
                throw new Exception("Login denied!");
            }

            return new Tuple<string, string>(username, password);
            */
            return new Tuple<string, string>(username, password);
        }
    }
}