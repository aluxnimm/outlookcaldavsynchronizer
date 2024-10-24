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
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer.Implementation.Common
{
    public class QueryOutlookFolderByGetTableStrategy : IQueryOutlookFolderStrategy
    {
        private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string PR_GLOBAL_OBJECT_ID = "http://schemas.microsoft.com/mapi/id/{6ED8DA90-450B-101B-98DA-00AA003F1305}/00030102";
        private const string PR_LONG_TERM_ENTRYID_FROM_TABLE = "http://schemas.microsoft.com/mapi/proptag/0x66700102";
        private const string PR_ENTRYID = "http://schemas.microsoft.com/mapi/proptag/0x0FFF0102";
        private const string LastModificationTimeColumnId = "LastModificationTime";
        private const string UserModificationTimeColumnId = "UserModifiedTime";
        private const string SubjectColumnId = "Subject";
        private const string StartColumnId = "Start";
        private const string EndColumnId = "End";

        public static readonly IQueryOutlookFolderStrategy Instance = new QueryOutlookFolderByGetTableStrategy();

        private QueryOutlookFolderByGetTableStrategy()
        {
        }

        List<AppointmentSlim> IQueryOutlookAppointmentItemFolderStrategy.QueryAppointmentFolder(IOutlookSession session, Folder calendarFolder, string filter, IGetVersionsLogger logger)
        {
            var events = new List<AppointmentSlim>();

            using (var tableWrapper = GenericComObjectWrapper.Create(
                calendarFolder.GetTable(filter)))
            {
                var table = tableWrapper.Inner;
                table.Columns.RemoveAll();
                table.Columns.Add(PR_GLOBAL_OBJECT_ID);
                table.Columns.Add(PR_LONG_TERM_ENTRYID_FROM_TABLE);
                table.Columns.Add(PR_ENTRYID);
                table.Columns.Add(LastModificationTimeColumnId);
                table.Columns.Add(SubjectColumnId);
                table.Columns.Add(StartColumnId);
                table.Columns.Add(EndColumnId);
                table.Columns.Add(UserModificationTimeColumnId);
                while (!table.EndOfTable)
                {
                    var row = table.GetNextRow();

                    string entryId;
                    byte[] entryIdArray = row[PR_LONG_TERM_ENTRYID_FROM_TABLE] as byte[];
                    if (entryIdArray != null && entryIdArray.Length > 0)
                    {
                        entryId = row.BinaryToString(PR_LONG_TERM_ENTRYID_FROM_TABLE);
                    }
                    else
                    {
                        // Fall back to short-term ENTRYID if long-term ID not available
                        entryId = row.BinaryToString(PR_ENTRYID);
                        s_logger.Warn($"Could not access long-term ENTRYID of appointment '{entryId}', use short-term ENTRYID as fallback.");
                    }

                    string globalAppointmentId = null;
                    try
                    {
                        byte[] globalIdArray = row[PR_GLOBAL_OBJECT_ID] as byte[];
                        if (globalIdArray != null && globalIdArray.Length > 0)
                        {
                            globalAppointmentId = row.BinaryToString(PR_GLOBAL_OBJECT_ID);
                        }
                    }
                    catch (Exception ex)
                    {
                        s_logger.Warn($"Could not access GlobalAppointmentID of appointment '{entryId}'.", ex);
                    }

                    var subject = (string) row[SubjectColumnId];
                    var appointmentId = new AppointmentId(entryId, globalAppointmentId);

                    var lastModificationTimeObject = row[LastModificationTimeColumnId];
                    var userModificationTimeObject = row[UserModificationTimeColumnId];
                    DateTime lastModificationTime;
                    DateTime userModificationTime;
                    if (lastModificationTimeObject != null)
                    {
                        lastModificationTime = ((DateTime) lastModificationTimeObject).ToUniversalTime();
                        if (userModificationTimeObject != null)
                        {
                            userModificationTime = ((DateTime)userModificationTimeObject).ToUniversalTime();
                            if (userModificationTime > lastModificationTime)
                                lastModificationTime = userModificationTime;
                        }
                    }
                    else
                    {
                        s_logger.Warn($"Column '{nameof(LastModificationTimeColumnId)}' of event '{entryId}' is NULL.");
                        logger.LogWarning(entryId, $"Column '{nameof(LastModificationTimeColumnId)}' is NULL.");
                        lastModificationTime = OutlookUtility.OUTLOOK_DATE_NONE;
                    }

                    var startObject = row[StartColumnId];
                    DateTime? start;
                    if (startObject != null)
                    {
                        start = (DateTime) startObject;
                    }
                    else
                    {
                        s_logger.Warn($"Column '{nameof(StartColumnId)}' of event '{entryId}' is NULL.");
                        logger.LogWarning(entryId, $"Column '{nameof(StartColumnId)}' is NULL.");
                        start = null;
                    }

                    var endObject = row[EndColumnId];
                    DateTime? end;
                    if (endObject != null)
                    {
                        end = (DateTime) endObject;
                    }
                    else
                    {
                        s_logger.Warn($"Column '{nameof(EndColumnId)}' of event '{entryId}' is NULL.");
                        logger.LogWarning(entryId, $"Column '{nameof(EndColumnId)}' is NULL.");
                        end = null;
                    }

                    events.Add(new AppointmentSlim(EntityVersion.Create(appointmentId, lastModificationTime.ToUniversalTime()), start, end, subject));
                }
            }

            return events;
        }

        List<EntityVersion<string, DateTime>> IQueryOutlookContactItemFolderStrategy.QueryContactItemFolder(IOutlookSession session, Folder folder, string expectedFolderId, string filter, IGetVersionsLogger logger)
        {
            return QueryFolder(folder, filter, logger);
        }

        List<EntityVersion<string, DateTime>> IQueryOutlookTaskItemFolderStrategy.QueryTaskFolder(IOutlookSession session, Folder folder, string filter, IGetVersionsLogger logger)
        {
            return QueryFolder(folder, filter, logger);
        }

        List<EntityVersion<string, DateTime>> IQueryOutlookDistListItemFolderStrategy.QueryDistListFolder(IOutlookSession session, Folder folder, string expectedFolderId, string filter, IGetVersionsLogger logger)
        {
            return QueryFolder(folder, filter, logger);
        }

        List<EntityVersion<string, DateTime>> QueryFolder(Folder folder, string filter, IGetVersionsLogger logger)
        {
            var versions = new List<EntityVersion<string, DateTime>>();

            using (var tableWrapper = GenericComObjectWrapper.Create(folder.GetTable(filter)))
            {
                var table = tableWrapper.Inner;
                table.Columns.RemoveAll();
                table.Columns.Add(PR_LONG_TERM_ENTRYID_FROM_TABLE);
                table.Columns.Add(PR_ENTRYID);
                table.Columns.Add(LastModificationTimeColumnId);

                while (!table.EndOfTable)
                {
                    var row = table.GetNextRow();

                    string entryId;
                    byte[] entryIdArray = row[PR_LONG_TERM_ENTRYID_FROM_TABLE] as byte[];
                    if (entryIdArray != null && entryIdArray.Length > 0)
                    {
                        entryId = row.BinaryToString(PR_LONG_TERM_ENTRYID_FROM_TABLE);
                    }
                    else
                    {
                        // Fall back to short-term ENTRYID if long-term ID not available
                        entryId = row.BinaryToString(PR_ENTRYID);
                        s_logger.Warn($"Could not access long-term ENTRYID of entity '{entryId}', use short-term ENTRYID as fallback.");
                    }

                    var lastModificationTimeObject = row[LastModificationTimeColumnId];
                    DateTime lastModificationTime;
                    if (lastModificationTimeObject != null)
                    {
                        lastModificationTime = ((DateTime) lastModificationTimeObject).ToUniversalTime();
                    }
                    else
                    {
                        s_logger.Warn($"Column '{nameof(LastModificationTimeColumnId)}' of entity '{entryId}' is NULL.");
                        logger.LogWarning(entryId, $"Column '{nameof(LastModificationTimeColumnId)}' is NULL.");
                        lastModificationTime = OutlookUtility.OUTLOOK_DATE_NONE;
                    }

                    versions.Add(new EntityVersion<string, DateTime>(entryId, lastModificationTime.ToUniversalTime()));
                }
            }

            return versions;
        }
    }
}