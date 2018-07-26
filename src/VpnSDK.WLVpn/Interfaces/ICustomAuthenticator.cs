using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VpnSDK.WLVpn.Interfaces
{
    /// <summary>
    /// Interface ICustomAuthenticator. Allows implementation of a custom API pre-authentication method.
    /// </summary>
    public interface ICustomAuthenticator
    {
        /// <summary>
        /// Logins the specified username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>Task&lt;Tuple&lt;System.String, System.String&gt;&gt; containing the username and password to pass on to the WLVPN API.</returns>
        Task<Tuple<string, string>> Login(string username, string password);
    }
}