// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Utilities;
using log4net;

namespace CalDavSynchronizer.Contracts
{
    public class ProxyOptions
    {
        private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);
        private const int c_saltLength = 17;

        // ReSharper disable MemberCanBePrivate.Global
        public string ProxySalt { get; set; }
        public bool ShouldSerializeProxySalt()
        {
            return !ProxyUseDefault && ProxyUseManual;
        }
        public string ProxyProtectedPassword { get; set; }
        public bool ShouldSerializeProxyProtectedPassword()
        {
            return !ProxyUseDefault && ProxyUseManual;
        }
        // ReSharper restore MemberCanBePrivate.Global

        public bool ProxyUseDefault { get; set; }
        public bool ProxyUseManual { get; set; }
        public string ProxyUrl { get; set; }
        public string ProxyUserName { get; set; }

        [XmlIgnore]
        public SecureString ProxyPassword
        {
            get
            {
                if (string.IsNullOrEmpty(ProxyProtectedPassword))
                    return new SecureString();

                var salt = Convert.FromBase64String(ProxySalt);

                var data = Convert.FromBase64String(ProxyProtectedPassword);
                try
                {
                    var transformedData = ProtectedData.Unprotect(data, salt, DataProtectionScope.CurrentUser);
                    return SecureStringUtility.ToSecureString(Encoding.Unicode.GetString(transformedData));
                }
                catch (CryptographicException x)
                {
                    s_logger.Error("Error while decrypting proxy password. Using empty password", x);
                    return new SecureString();
                }
            }
            set
            {
                byte[] salt = new byte[c_saltLength];
                new Random().NextBytes(salt);
                ProxySalt = Convert.ToBase64String(salt);

                var data = Encoding.Unicode.GetBytes(SecureStringUtility.ToUnsecureString(value));
                var transformedData = ProtectedData.Protect(data, salt, DataProtectionScope.CurrentUser);
                ProxyProtectedPassword = Convert.ToBase64String(transformedData);
            }
        }
    }
}