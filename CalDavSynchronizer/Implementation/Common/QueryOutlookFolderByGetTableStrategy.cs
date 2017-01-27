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
  public class QueryOutlookFolderByGetTableStrategy : IQueryOutlookFolderStrategy
  {
    private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private const string PR_GLOBAL_OBJECT_ID = "http://schemas.microsoft.com/mapi/id/{6ED8DA90-450B-101B-98DA-00AA003F1305}/00030102";
    private const string PR_LONG_TERM_ENTRYID_FROM_TABLE = "http://schemas.microsoft.com/mapi/proptag/0x66700102";
    private const string LastModificationTimeColumnId = "LastModificationTime";
    private const string SubjectColumnId = "Subject";
    private const string StartColumnId = "Start";
    private const string EndColumnId = "End";

    public static readonly IQueryOutlookFolderStrategy Instance = new QueryOutlookFolderByGetTableStrategy();
    private QueryOutlookFolderByGetTableStrategy()
    {
    }

    List<AppointmentSlim> IQueryOutlookAppointmentItemFolderStrategy.QueryAppointmentFolder(NameSpace session, Folder calendarFolder, string filter)
    {
      var events = new List<AppointmentSlim>();

      using (var tableWrapper = GenericComObjectWrapper.Create(
        calendarFolder.GetTable(filter)))
      {
        var table = tableWrapper.Inner;
        table.Columns.RemoveAll();
        table.Columns.Add (PR_GLOBAL_OBJECT_ID);
        table.Columns.Add (PR_LONG_TERM_ENTRYID_FROM_TABLE);
        table.Columns.Add (LastModificationTimeColumnId);
        table.Columns.Add (SubjectColumnId);
        table.Columns.Add (StartColumnId);
        table.Columns.Add (EndColumnId);

        while (!table.EndOfTable)
        {
          var row = table.GetNextRow();
          var entryId =  row.BinaryToString(PR_LONG_TERM_ENTRYID_FROM_TABLE);
          var globalAppointmentId = row.BinaryToString(PR_GLOBAL_OBJECT_ID);
          var lastModificationTime = (DateTime) row[LastModificationTimeColumnId];
          var subject = (string) row[SubjectColumnId];
          var start = (DateTime) row[StartColumnId];
          var end = (DateTime) row[EndColumnId];

          events.Add(new AppointmentSlim(EntityVersion.Create(new AppointmentId(entryId, globalAppointmentId), lastModificationTime), start, end, subject));
        }
      }
      return events;
    }

    List<EntityVersion<string, DateTime>> IQueryOutlookContactItemFolderStrategy.QueryContactItemFolder(NameSpace session, Folder folder, string expectedFolderId, string filter)
    {
      return QueryFolder (folder, filter);
    }

    List<EntityVersion<string, DateTime>> IQueryOutlookTaskItemFolderStrategy.QueryTaskFolder (NameSpace session,Folder folder,string filter)
    {
      return QueryFolder (folder, filter);
    }

    List<EntityVersion<string, DateTime>> IQueryOutlookDistListItemFolderStrategy.QueryDistListFolder (NameSpace session, Folder folder, string expectedFolderId, string filter)
    {
      return QueryFolder(folder, filter);
    }

    List<EntityVersion<string, DateTime>> QueryFolder (Folder folder, string filter)
    {
      var versions = new List<EntityVersion<string, DateTime>> ();

      using (var tableWrapper = GenericComObjectWrapper.Create (folder.GetTable (filter)))
      {
        var table = tableWrapper.Inner;
        table.Columns.RemoveAll ();
        table.Columns.Add (PR_LONG_TERM_ENTRYID_FROM_TABLE);
        table.Columns.Add (LastModificationTimeColumnId);

        while (!table.EndOfTable)
        {
          var row = table.GetNextRow ();
          var entryId = row.BinaryToString (PR_LONG_TERM_ENTRYID_FROM_TABLE);
          var lastModificationTime = (DateTime) row[LastModificationTimeColumnId];
          versions.Add (new EntityVersion<string, DateTime> (entryId, lastModificationTime));
        }
      }

      return versions;
    }
  }
}