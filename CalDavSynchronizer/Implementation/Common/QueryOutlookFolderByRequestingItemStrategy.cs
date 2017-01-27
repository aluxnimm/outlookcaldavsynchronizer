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

    List<AppointmentSlim> IQueryOutlookAppointmentItemFolderStrategy.QueryAppointmentFolder(NameSpace session, Folder calendarFolder, string filter)
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
            using (var appointmentWrapper = GenericComObjectWrapper.Create((AppointmentItem) session.GetItemFromID(entryId, storeId)))
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

    List<EntityVersion<string, DateTime>> IQueryOutlookContactItemFolderStrategy.QueryContactItemFolder (NameSpace session, Folder folder, string expectedFolderId, string filter)
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

    List<EntityVersion<string, DateTime>> IQueryOutlookTaskItemFolderStrategy.QueryTaskFolder (NameSpace session,Folder folder,string filter)
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
            using (var taskWrapper = GenericComObjectWrapper.Create ((TaskItem) session.GetItemFromID (entryId, storeId)))
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

    List<EntityVersion<string, DateTime>> IQueryOutlookDistListItemFolderStrategy.QueryDistListFolder (NameSpace session, Folder folder, string expectedFolderId, string filter)
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