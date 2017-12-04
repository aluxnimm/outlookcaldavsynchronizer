﻿// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
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
  public class Options
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private const int c_saltLength = 17;
    private static Random s_random = new Random ();

    public bool Inactive { get; set; }
    public string Name { get; set; }
    public Guid Id { get; set; }
    public string OutlookFolderEntryId { get; set; }
    public string OutlookFolderStoreId { get; set; }
    public string OutlookFolderAccountName { get; set; }

    public bool IgnoreSynchronizationTimeRange { get; set; }
    public int DaysToSynchronizeInThePast { get; set; }
    public int DaysToSynchronizeInTheFuture { get; set; }
    public SynchronizationMode SynchronizationMode { get; set; }
    public ConflictResolution ConflictResolution { get; set; }
    public string CalenderUrl { get; set; }
    public string EmailAddress { get; set; }
    public string UserName { get; set; }
    public int SynchronizationIntervalInMinutes { get; set; }
    public bool UseWebDavCollectionSync { get; set; }

    // ReSharper disable MemberCanBePrivate.Global
    public string Salt { get; set; }
    public string ProtectedPassword { get; set; }
    // ReSharper restore MemberCanBePrivate.Global
    public bool UseAccountPassword { get; set; }
    public ServerAdapterType ServerAdapterType { get; set; }
    public bool CloseAfterEachRequest { get; set; }
    public bool PreemptiveAuthentication { get; set; }
    public bool ForceBasicAuthentication { get; set; }
    public bool EnableChangeTriggeredSynchronization { get; set; }
    public bool IsChunkedSynchronizationEnabled { get; set; }
    public int ChunkSize { get; set; } = 100;
    public int? EffectiveChunkSize => IsChunkedSynchronizationEnabled ? ChunkSize : (int?) null;

    public ProxyOptions ProxyOptions { get; set; }
    public MappingConfigurationBase MappingConfiguration { get; set; }
    public string ProfileTypeOrNull { get; set; }

    public SecureString GetEffectivePassword (IOutlookAccountPasswordProvider outlookAccountPasswordProvider)
    {
      return UseAccountPassword
          ? outlookAccountPasswordProvider.GetPassword (OutlookFolderAccountName)
          : Password;
    }

    [XmlIgnore]
    public SecureString Password
    {
      get
      {
        if (string.IsNullOrEmpty (ProtectedPassword))
          return new SecureString();

        var salt = Convert.FromBase64String (Salt);

        var data = Convert.FromBase64String (ProtectedPassword);
        try
        {
          var transformedData = ProtectedData.Unprotect (data, salt, DataProtectionScope.CurrentUser);
          return SecureStringUtility.ToSecureString (Encoding.Unicode.GetString (transformedData));
        }
        catch (CryptographicException x)
        {
          s_logger.Error ("Error while decrypting password. Using empty password", x);
          return new SecureString ();
        }
      }
      set
      {
        byte[] salt = new byte[c_saltLength];
        s_random.NextBytes (salt);
        Salt = Convert.ToBase64String (salt);

        var data = Encoding.Unicode.GetBytes (SecureStringUtility.ToUnsecureString(value));
        var transformedData = ProtectedData.Protect (data, salt, DataProtectionScope.CurrentUser);
        ProtectedPassword = Convert.ToBase64String (transformedData);
      }
    }
  }
}
