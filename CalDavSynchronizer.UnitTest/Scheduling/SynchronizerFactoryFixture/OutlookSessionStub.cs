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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.using System;

using System;
using System.Collections.Generic;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Ui;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.UnitTest.Scheduling.SynchronizerFactoryFixture
{
  class OutlookSessionStub : IOutlookSession
  {
    public string ApplicationVersion { get; }
    public IOutlookTimeZones TimeZones { get; }
    public StringComparer CategoryNameComparer { get; }

    public IReadOnlyCollection<OutlookCategory> GetCategories()
    {
      throw new System.NotImplementedException();
    }

    public string GetCurrentUserEmailAddressOrNull()
    {
      return "testuser@bla.com";
    }

    public OutlookFolderDescriptor GetFolderDescriptorFromId(string entryId, object storeId)
    {
      return new OutlookFolderDescriptor("eid", "sid", OlItemType.olContactItem, "ContactFolder", 0);
    }

    public Folder GetFolderFromId(string entryId, object storeId)
    {
      throw new System.NotImplementedException();
    }

    public AppointmentItem GetAppointmentItemOrNull(string entryId, string expectedFolderId, string storeId)
    {
      throw new System.NotImplementedException();
    }

    public TaskItem GetTaskItemOrNull(string entryId, string expectedFolderId, string storeId)
    {
      throw new System.NotImplementedException();
    }

    public ContactItem GetContactItemOrNull(string entryId, string expectedFolderId, string storeId)
    {
      throw new System.NotImplementedException();
    }

    public DistListItem GetDistListItemOrNull(string entryId, string expectedFolderId, string storeId)
    {
      throw new System.NotImplementedException();
    }

    public AppointmentItem GetAppointmentItem(string entryId, string storeId)
    {
      throw new System.NotImplementedException();
    }

    public AppointmentItem GetAppointmentItem(string entryId)
    {
      throw new System.NotImplementedException();
    }

    public TaskItem GetTaskItem(string entryId, string storeId)
    {
      throw new System.NotImplementedException();
    }

    public ContactItem GetContactItem(string entryId, string storeId)
    {
      throw new System.NotImplementedException();
    }

    public DistListItem GetDistListItem(string entryId, string storeId)
    {
      throw new System.NotImplementedException();
    }

    public Recipient CreateRecipient(string recipientName)
    {
      throw new System.NotImplementedException();
    }

    public CreateCategoryResult AddCategoryNoThrow(string name, OlCategoryColor color)
    {
      throw new System.NotImplementedException();
    }

    public void AddOrUpdateCategoryNoThrow(string name, OlCategoryColor color, bool useColor, OlCategoryShortcutKey shortcutKey, bool useShortcutKey)
    {
      throw new System.NotImplementedException();
    }

    public IReadOnlyDictionary<string, IReadOnlyList<OutlookFolderDescriptor>> GetFoldersByName()
    {
      throw new NotImplementedException();
    }
  }
}