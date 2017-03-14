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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Events;
using GenSync;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.Common
{
  public class QueryOutlookFolderByRequestingItemStrategy : IQueryOutlookFolderStrategy
  {
    private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private const string c_entryIdColumnName = "EntryID";

    public static readonly IQueryOutlookFolderStrategy Instance = new QueryOutlookFolderByRequestingItemStrategy ();
    private QueryOutlookFolderByRequestingItemStrategy ()
    {
    }

    List<AppointmentSlim> IQueryOutlookAppointmentItemFolderStrategy.QueryAppointmentFolder(IOutlookSession session, Folder calendarFolder, string filter)
    {
      var events = new List<AppointmentSlim>();

      using (var tableWrapper = GenericComObjectWrapper.Create(
        calendarFolder.GetTable(filter)))
      {
        var table = tableWrapper.Inner;
        table.Columns.RemoveAll();
        table.Columns.Add(c_entryIdColumnName);

        var storeId = calendarFolder.StoreID;

        while (!table.EndOfTable)
        {
          var row = table.GetNextRow();
          var entryId = (string) row[c_entryIdColumnName];
          try
          {
            using (var appointmentWrapper = GenericComObjectWrapper.Create(session.GetAppointmentItem(entryId, storeId)))
            {
              events.Add(AppointmentSlim.FromAppointmentItem(appointmentWrapper.Inner));
            }
          }
          catch (COMException ex)
          {
            s_logger.Error("Could not fetch AppointmentItem, skipping.", ex);
          }
        }
      }
      return events;
    }

    List<EntityVersion<string, DateTime>> IQueryOutlookContactItemFolderStrategy.QueryContactItemFolder (IOutlookSession session, Folder folder, string expectedFolderId, string filter)
    {
      var contacts = new List<EntityVersion<string, DateTime>> ();

      using (var tableWrapper = GenericComObjectWrapper.Create (folder.GetTable (filter)))
      {
        var table = tableWrapper.Inner;
        table.Columns.RemoveAll ();
        table.Columns.Add (c_entryIdColumnName);

        var storeId = folder.StoreID;

        while (!table.EndOfTable)
        {
          var row = table.GetNextRow ();
          var entryId = (string) row[c_entryIdColumnName];

          var contact = session.GetContactItemOrNull (entryId, expectedFolderId, storeId);
          if (contact != null)
          {
            using (var contactWrapper = GenericComObjectWrapper.Create (contact))
            {
              contacts.Add (new EntityVersion<string, DateTime> (contactWrapper.Inner.EntryID, contactWrapper.Inner.LastModificationTime));
            }
          }
        }
      }

      return contacts;
    }

    List<EntityVersion<string, DateTime>> IQueryOutlookTaskItemFolderStrategy.QueryTaskFolder (IOutlookSession session,Folder folder,string filter)
    {
      var tasks = new List<EntityVersion<string, DateTime>> ();

      using (var tableWrapper = GenericComObjectWrapper.Create (
          folder.GetTable (filter)))
      {
        var table = tableWrapper.Inner;
        table.Columns.RemoveAll ();
        table.Columns.Add (c_entryIdColumnName);

        var storeId = folder.StoreID;

        while (!table.EndOfTable)
        {
          var row = table.GetNextRow ();
          var entryId = (string) row[c_entryIdColumnName];
          try
          {
            using (var taskWrapper = GenericComObjectWrapper.Create (session.GetTaskItem (entryId, storeId)))
            {
              tasks.Add (new EntityVersion<string, DateTime> (taskWrapper.Inner.EntryID, taskWrapper.Inner.LastModificationTime));
            }
          }
          catch (COMException ex)
          {
            s_logger.Error ("Could not fetch TaskItem, skipping.", ex);
          }
        }
      }
      return tasks;
    }

    List<EntityVersion<string, DateTime>> IQueryOutlookDistListItemFolderStrategy.QueryDistListFolder (IOutlookSession session, Folder folder, string expectedFolderId, string filter)
    {
      var contacts = new List<EntityVersion<string, DateTime>> ();

      using (var tableWrapper = GenericComObjectWrapper.Create (folder.GetTable (filter)))
      {
        var table = tableWrapper.Inner;
        table.Columns.RemoveAll ();
        table.Columns.Add (c_entryIdColumnName);

        var storeId = folder.StoreID;

        while (!table.EndOfTable)
        {
          var row = table.GetNextRow ();
          var entryId = (string) row[c_entryIdColumnName];

          var contact = session.GetDistListItemOrNull (entryId, expectedFolderId, storeId);
          if (contact != null)
          {
            using (var contactWrapper = GenericComObjectWrapper.Create (contact))
            {
              contacts.Add (new EntityVersion<string, DateTime> (contactWrapper.Inner.EntryID, contactWrapper.Inner.LastModificationTime));
            }
          }
        }
      }

      return contacts;
    }
  }
}