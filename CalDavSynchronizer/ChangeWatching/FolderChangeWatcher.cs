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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CalDavSynchronizer.Implementation.Events;
using GenSync;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.ChangeWatching
{
  class FolderChangeWatcher : IItemCollectionChangeWatcher
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);

    private readonly Items _folderItems;
    private readonly Folder _folder;

    private readonly string _folderEntryId;
    private readonly string _folderStoreId;

    public event EventHandler<ItemSavedEventArgs> ItemSavedOrDeleted;

    public FolderChangeWatcher (Folder folder, string folderEntryId, string folderStoreId)
    {
      if (folder == null)
        throw new ArgumentNullException (nameof (folder));

      _folder = folder;
      _folderItems = folder.Items;

      _folderEntryId = folderEntryId;
      _folderStoreId = folderStoreId;

      _folder.BeforeItemMove += FolderEvents_BeforeItemMove;
      _folderItems.ItemAdd += _folderItems_ItemAdd;
      _folderItems.ItemChange += _folderItems_ItemChange;
    }

    private void FolderEvents_BeforeItemMove (object item, MAPIFolder moveTo, ref bool cancel)
    {
      OnItemSavedOrDeleted (item,  ItemAction.Delete);
    }

    private void _folderItems_ItemChange (object item)
    {
      OnItemSavedOrDeleted (item, ItemAction.Change);
    }

    private void _folderItems_ItemAdd (object item)
    {
      OnItemSavedOrDeleted (item, ItemAction.Add);
    }

    public void Dispose ()
    {
      _folderItems.ItemAdd -= _folderItems_ItemAdd;
      _folderItems.ItemChange -= _folderItems_ItemChange;
      _folder.BeforeItemMove -= FolderEvents_BeforeItemMove;

      Marshal.FinalReleaseComObject (_folderItems);
      Marshal.FinalReleaseComObject (_folder);
    }

    private void OnItemSavedOrDeleted (object item, ItemAction action)
    {
      IOutlookId entryId = null;

      bool wasDeleted = action == ItemAction.Delete;

      var appointment = item as AppointmentItem;
      if (appointment != null)
      {
        //Don't add appointment if it is a meeting invite, since this can cause doubled and/or cancelled meetings
        if (appointment.MeetingStatus != OlMeetingStatus.olMeetingReceived)
        {
          s_logger.Debug ($"'{nameof (ItemAction)}.{action}': Appointment '{appointment.Subject}' '{appointment.EntryID}' ");
          entryId = new AppointmentId(new Implementation.Events.AppointmentId(appointment.EntryID, appointment.GlobalAppointmentID), appointment.LastModificationTime.ToUniversalTime(), wasDeleted);
        }
      }
      else
      {
        var task = item as TaskItem;
        if (task != null)
        {
          s_logger.Debug ($"'{nameof (ItemAction)}.{action}': Task '{task.Subject}' '{task.EntryID}' ");
          entryId = new GenericId(task.EntryID, task.LastModificationTime.ToUniversalTime(), wasDeleted);
        }
        else
        {
          var contact = item as ContactItem;
          if (contact != null)
          {
            s_logger.Debug ($"'{nameof (ItemAction)}.{action}': Contact '{contact.LastNameAndFirstName}' '{contact.EntryID}' ");
            entryId = new GenericId (contact.EntryID, contact.LastModificationTime.ToUniversalTime(), wasDeleted);
          }
        }
      }

      if (entryId != null)
        OnItemSavedOrDeleted (entryId);
    }

    protected virtual void OnItemSavedOrDeleted (IOutlookId entryId)
    {
      ItemSavedOrDeleted?.Invoke (
          this,
          new ItemSavedEventArgs (entryId));
    }
  }
}