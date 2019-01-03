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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Contacts;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.DistributionLists
{
  public class DistributionListSychronizationContext
  {
    private readonly Dictionary<string, CacheItem> _cacheItemsByEmailAddress = new Dictionary<string, CacheItem>();
    private readonly Dictionary<string, CacheItem> _cacheItemsByUid = new Dictionary<string, CacheItem>();
    private readonly Lazy<Dictionary<WebResourceName, string>> _outlookIdsByServerId;
    private readonly (string FolderId, string FolderStoreId) _contactFolder;

    public DistributionListSychronizationContext(
      CacheItem[] emailAddressCacheItems,
      IOutlookSession outlookSession,
      IEntityRelationDataAccess<string, DateTime, WebResourceName, string> contactEntityRelationDataAccess,
      (string FolderId, string FolderStoreId) contactFolder)
    {
      if (emailAddressCacheItems == null) throw new ArgumentNullException(nameof(emailAddressCacheItems));
      if (outlookSession == null) throw new ArgumentNullException(nameof(outlookSession));
      if (contactEntityRelationDataAccess == null) throw new ArgumentNullException(nameof(contactEntityRelationDataAccess));

      OutlookSession = outlookSession;
      _contactFolder = contactFolder;
      foreach (var cacheItem in emailAddressCacheItems)
      {
        foreach (var emailAddress in cacheItem.EmailAddresses)
        {
          _cacheItemsByEmailAddress[emailAddress] = cacheItem;
        }
        if(!string.IsNullOrEmpty(cacheItem.Uid)) // can be null, if the cache is from a previous version
          _cacheItemsByUid[cacheItem.Uid] = cacheItem;
      }

      _outlookIdsByServerId = new Lazy<Dictionary<WebResourceName, string>>(
        () => contactEntityRelationDataAccess.LoadEntityRelationData().ToDictionary(r => r.BtypeId, r => r.AtypeId, WebResourceName.Comparer),
        false);
    }

    public IOutlookSession OutlookSession { get; }

    public string GetServerFileNameByEmailAddress(string emailAddress)
    {
      if (_cacheItemsByEmailAddress.TryGetValue(emailAddress, out var cacheItem))
        return cacheItem.Id.GetServerFileName();
      else
        return null;
    }

    public string GetUidByEmailAddress(string emailAddress)
    {
      if (_cacheItemsByEmailAddress.TryGetValue(emailAddress, out var cacheItem))
        return cacheItem.Uid;
      else
        return null;
    }

    public (GenericComObjectWrapper<ContactItem> ContactWrapper, string EmailAddress) GetContactByUidOrNull(string uid, IEntitySynchronizationLogger synchronizationLogger, ILog logger)
    {
      if (_cacheItemsByUid.TryGetValue(uid, out var cacheItem))
      {
        if (_outlookIdsByServerId.Value.TryGetValue(cacheItem.Id, out var outlookId))
        {
          var contactItemOrNull = OutlookSession.GetContactItemOrNull(outlookId, _contactFolder.FolderId, _contactFolder.FolderStoreId);
          return contactItemOrNull != null ? (GenericComObjectWrapper.Create(contactItemOrNull),cacheItem.EmailAddresses.FirstOrDefault()) : (null, null);
        }
        else
        {
          var logMessage = $"Did not find OutlookId for ServerId '{cacheItem.Id}'";
          logger.WarnFormat(logMessage);
          synchronizationLogger.LogWarning(logMessage);

          return (null, null);
        }
      }
      else
      {
        var logMessage = $"Did not find cacheEntry for Uid '{uid}'";
        logger.WarnFormat(logMessage);
        synchronizationLogger.LogWarning(logMessage);
        return (null, null);
      }
    }
  }
}
