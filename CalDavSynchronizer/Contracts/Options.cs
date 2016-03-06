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
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using CalDavSynchronizer.Implementation;
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

    public ProxyOptions ProxyOptions { get; set; }
    public MappingConfigurationBase MappingConfiguration { get; set; }

    public OptionsDisplayType DisplayType { get; set; }

    public string GetEffectivePassword (IOutlookAccountPasswordProvider outlookAccountPasswordProvider)
    {
      return UseAccountPassword
          ? outlookAccountPasswordProvider.GetPassword (OutlookFolderAccountName)
          : Password;
    }

    [XmlIgnore]
    public string Password
    {
      get
      {
        if (string.IsNullOrEmpty (ProtectedPassword))
          return string.Empty;

        var salt = Convert.FromBase64String (Salt);

        var data = Convert.FromBase64String (ProtectedPassword);
        try
        {
          var transformedData = ProtectedData.Unprotect (data, salt, DataProtectionScope.CurrentUser);
          return Encoding.Unicode.GetString (transformedData);
        }
        catch (CryptographicException x)
        {
          s_logger.Error ("Error while decrypting password. Using empty password", x);
          return string.Empty;
        }
      }
      set
      {
        byte[] salt = new byte[c_saltLength];
        s_random.NextBytes (salt);
        Salt = Convert.ToBase64String (salt);

        var data = Encoding.Unicode.GetBytes (value);
        var transformedData = ProtectedData.Protect (data, salt, DataProtectionScope.CurrentUser);
        ProtectedPassword = Convert.ToBase64String (transformedData);
      }
    }

    public static Options CreateDefault (OptionsDisplayType type)
    {
      var options = new Options();

      options.ConflictResolution = ConflictResolution.Automatic;
      options.DaysToSynchronizeInTheFuture = 180;
      options.DaysToSynchronizeInThePast = 60;
      options.SynchronizationIntervalInMinutes = 30;
      options.SynchronizationMode = SynchronizationMode.MergeInBothDirections;
      options.Name = "<New Profile>";
      options.Id = Guid.NewGuid();
      options.Inactive = false;
      options.PreemptiveAuthentication = true;
      options.ForceBasicAuthentication = false;
      options.ProxyOptions = new ProxyOptions() { ProxyUseDefault = true };
      options.DisplayType = type;
      options.CalenderUrl = PopulateDavUrl (options.DisplayType);

      if (type == OptionsDisplayType.GmxCalendar)
      {
        options.MappingConfiguration = new EventMappingConfiguration
        {
          CreateEventsInUTC = true
        };
      }

      return options;
    }

    private static string PopulateDavUrl (OptionsDisplayType type)
    {
      switch (type)
      {
        case OptionsDisplayType.Fruux:
          return "https://dav.fruux.com";
        case OptionsDisplayType.Posteo:
          return "https://posteo.de:8443";
        case OptionsDisplayType.Yandex:
          return "https://caldav.yandex.ru";
        case OptionsDisplayType.GmxCalendar:
          return "https://caldav.gmx.net";
        default:
          return null;
      }
    }
  }
}