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
using CalDavSynchronizer.Implementation.ComWrappers;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.Common
{
  public static class NameSpaceExtensions
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodInfo.GetCurrentMethod ().DeclaringType);

    public static TItemType GetEntryOrNull<TItemType> (this NameSpace mapiNameSpace, string entryId, string folderId, string storeId)
    where TItemType : class
    {
      try
      {
        var item = (TItemType) mapiNameSpace.GetItemFromID (entryId, storeId);
        var appointmentItem = item as AppointmentItem;
        if (appointmentItem != null)
        {
          using (var folderWrapper = GenericComObjectWrapper.Create (appointmentItem.Parent as Folder))
          {
            if (folderWrapper.Inner?.EntryID == folderId) return item;
          }
        }
        else
        {
          var contactItem = item as ContactItem;
          if (contactItem != null)
          {
            using (var folderWrapper = GenericComObjectWrapper.Create (contactItem.Parent as Folder))
            {
              if (folderWrapper.Inner?.EntryID == folderId) return item;
            }
          }
          else
          {
            var taskItem = item as TaskItem;
            if (taskItem != null)
            {
              using (var folderWrapper = GenericComObjectWrapper.Create (taskItem.Parent as Folder))
              {
                if (folderWrapper.Inner?.EntryID == folderId) return item;
              }
            }
          }
        }
        return null;

      }
      catch (COMException x)
      {
        const int messageNotFoundResult = -2147221233;
        if (x.HResult != messageNotFoundResult)
          s_logger.Error ("Error while fetching entity.", x);
        return null;
      }
    }

  }
}
