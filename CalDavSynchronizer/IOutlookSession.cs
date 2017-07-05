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

using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Ui;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer
{
  public interface IOutlookSession
  {
    string ApplicationVersion { get; }
    IOutlookTimeZones TimeZones { get; }
    Categories Categories { get; }

    string GetCurrentUserEmailAddressOrNull();

    OutlookFolderDescriptor GetFolderDescriptorFromId(string entryId, object storeId);
    Folder GetFolderFromId (string entryId, object storeId);

    AppointmentItem GetAppointmentItemOrNull(string entryId, string expectedFolderId, string storeId);
    TaskItem GetTaskItemOrNull(string entryId, string expectedFolderId, string storeId);
    ContactItem GetContactItemOrNull (string entryId, string expectedFolderId, string storeId);
    DistListItem GetDistListItemOrNull (string entryId, string expectedFolderId, string storeId);

    AppointmentItem GetAppointmentItem (string entryId,string storeId);
    AppointmentItem GetAppointmentItem (string entryId);
    TaskItem GetTaskItem(string entryId, string storeId);
    ContactItem GetContactItem(string entryId, string storeId);
    DistListItem GetDistListItem(string entryId, string storeId);

    
    Recipient CreateRecipient (string recipientName);
  }
}