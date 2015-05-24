// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using CalDavSynchronizer.Implementation;

namespace CalDavSynchronizer.Contracts
{
  public class Options
  {
    private const int c_saltLength = 17;

    public bool Inactive { get; set; }
    public string Name { get; set; }
    public Guid Id { get; set; }
    public string OutlookFolderEntryId { get; set; }
    public string OutlookFolderStoreId { get; set; }

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

    [XmlIgnore]
    public string Password
    {
      get
      {
        if (string.IsNullOrEmpty (ProtectedPassword))
          return string.Empty;

        var salt = Convert.FromBase64String (Salt);

        var data = Convert.FromBase64String (ProtectedPassword);
        var transformedData = ProtectedData.Unprotect (data, salt, DataProtectionScope.CurrentUser);
        return Encoding.Unicode.GetString (transformedData);
      }
      set
      {
        byte[] salt = new byte[c_saltLength];
        new Random().NextBytes (salt);
        Salt = Convert.ToBase64String (salt);

        var data = Encoding.Unicode.GetBytes (value);
        var transformedData = ProtectedData.Protect (data, salt, DataProtectionScope.CurrentUser);
        ProtectedPassword = Convert.ToBase64String (transformedData);
      }
    }


    public static Options CreateDefault (string outlookFolderEntryId, string outlookFolderStoreId)
    {
      var options = new Options();

      options.OutlookFolderEntryId = outlookFolderEntryId;
      options.OutlookFolderStoreId = outlookFolderStoreId;
      options.ConflictResolution = ConflictResolution.OutlookWins;
      options.DaysToSynchronizeInTheFuture = 100;
      options.DaysToSynchronizeInThePast = 100;
      options.SynchronizationIntervalInMinutes = 30;
      options.SynchronizationMode = SynchronizationMode.ReplicateOutlookIntoServer;
      options.Name = "<Enter name here>";
      options.Id = Guid.NewGuid();
      options.Inactive = false;
      return options;
    }
  }
}