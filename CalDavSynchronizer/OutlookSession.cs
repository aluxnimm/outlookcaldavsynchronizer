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
using System.Runtime.InteropServices;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Ui;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer
{
  public class OutlookSession : IOutlookSession
  {
    private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly NameSpace _nameSpace;

    public OutlookSession(NameSpace nameSpace)
    {
      if (nameSpace == null) throw new ArgumentNullException(nameof(nameSpace));

      _nameSpace = nameSpace;
      TimeZones = new OutlookTimeZones(_nameSpace.Application);
    }

    public string GetCurrentUserEmailAddressOrNull()
    {
      try
      {
        using (var currentUser = GenericComObjectWrapper.Create(_nameSpace.CurrentUser))
        {
          if (currentUser.Inner != null)
          {
            using (var addressEntry = GenericComObjectWrapper.Create(currentUser.Inner.AddressEntry))
            {
              if (addressEntry.Inner != null)
              {
                return OutlookUtility.GetEmailAdressOrNull(addressEntry.Inner, NullEntityMappingLogger.Instance, s_logger) ?? string.Empty;
              }
            }
          }
        }
      }
      catch (COMException ex)
      {
        s_logger.Error("Can't access currentuser email adress.", ex);
      }

      return null;
    }

    public OutlookFolderDescriptor GetFolderDescriptorFromId(string entryId, object storeId)
    {
      return new OutlookFolderDescriptor(_nameSpace.GetFolderFromID(entryId, storeId));
    }


    public ContactItem GetContactItemOrNull(string entryId, string expectedFolderId, string storeId)
    {
      return GetEntryOrNull<ContactItem>(entryId, expectedFolderId, storeId, a => (Folder) a.Parent);
    }

    public TaskItem GetTaskItemOrNull(string entryId, string expectedFolderId, string storeId)
    {
      return GetEntryOrNull<TaskItem>(entryId, expectedFolderId, storeId, a => (Folder) a.Parent);
    }

    public AppointmentItem GetAppointmentItemOrNull(string entryId, string expectedFolderId, string storeId)
    {
      return GetEntryOrNull<AppointmentItem>(entryId, expectedFolderId, storeId, a => (Folder) a.Parent);
    }

    public DistListItem GetDistListItemOrNull(string entryId, string expectedFolderId, string storeId)
    {
      return GetEntryOrNull<DistListItem>(entryId, expectedFolderId, storeId, a => (Folder) a.Parent);
    }

    private TItemType GetEntryOrNull<TItemType>(
      string entryId,
      string expectedFolderId,
      string storeId,
      Func<TItemType, Folder> parentFolderGetter)
      where TItemType : class
    {
      try
      {
        var item = (TItemType) _nameSpace.GetItemFromID(entryId, storeId);

        using (var folderWrapper = GenericComObjectWrapper.Create(parentFolderGetter(item)))
        {
          if (folderWrapper.Inner?.EntryID == expectedFolderId)
            return item;
        }

        return null;
      }
      catch (COMException x)
      {
        const int messageNotFoundResult = -2147221233;
        if (x.HResult != messageNotFoundResult)
          s_logger.Error("Error while fetching entity.", x);
        return null;
      }
    }

    public Folder GetFolderFromId (string entryId, object storeId)
    {
      return (Folder) _nameSpace.GetFolderFromID(entryId, storeId);
    }

    public AppointmentItem GetAppointmentItem(string entryId,  string storeId)
    {
      return (AppointmentItem) _nameSpace.GetItemFromID(entryId, storeId);
    }

    public AppointmentItem GetAppointmentItem (string entryId)
    {
      return (AppointmentItem) _nameSpace.GetItemFromID (entryId);
    }

    public TaskItem GetTaskItem (string entryId, string storeId)
    {
      return (TaskItem) _nameSpace.GetItemFromID(entryId, storeId);
    }

    public ContactItem GetContactItem (string entryId, string storeId)
    {
      return (ContactItem) _nameSpace.GetItemFromID (entryId, storeId);
    }

    public DistListItem GetDistListItem (string entryId, string storeId)
    {
      return (DistListItem) _nameSpace.GetItemFromID(entryId, storeId);
    }

    

    public string ApplicationVersion => _nameSpace.Application.Version;
    public IOutlookTimeZones TimeZones { get; }
    public Categories Categories => _nameSpace.Categories;
    public Recipient CreateRecipient(string recipientName)
    {
      return _nameSpace.CreateRecipient(recipientName);
    }
  }
}
