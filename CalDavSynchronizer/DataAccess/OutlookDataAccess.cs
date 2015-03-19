// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using System.Globalization;
using System.Linq;
using System.Text;
using CalDavSynchronizer.EntityVersionManagement;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.DataAccess
{
  internal class OutlookDataAccess : IOutlookDataAccess
  {
    private readonly MAPIFolder _calendarFolder;
    private readonly NameSpace _mapiNameSpace;

    public OutlookDataAccess (MAPIFolder calendarFolder, NameSpace mapiNameSpace)
    {
      if (calendarFolder == null)
        throw new ArgumentNullException ("calendarFolder");
      if (mapiNameSpace == null)
        throw new ArgumentNullException ("mapiNameSpace");
      _calendarFolder = calendarFolder;
      _mapiNameSpace = mapiNameSpace;
    }

    const string c_entryIdColumnName = "EntryID";
    const string c_lastModificationTimeColumnId = "LastModificationTime";

    public IEnumerable<EntityIdWithVersion<string, DateTime>> GetEvents (DateTime? fromUtc, DateTime? toUtc)
    {
      var events = new List<EntityIdWithVersion<string, DateTime>> ();

      var filterBuilder = new StringBuilder();

      if (fromUtc.HasValue)
      {
        filterBuilder.AppendFormat ("[Start] > '{0}'", ToOutlookDateString(fromUtc.Value));
      }

      if (toUtc.HasValue)
      {
        if (filterBuilder.Length > 0)
          filterBuilder.Append (" And ");

        filterBuilder.AppendFormat ("[End] < '{0}'", ToOutlookDateString(toUtc.Value));
      }

      var table = _calendarFolder.GetTable(filterBuilder.ToString());
      table.Columns.RemoveAll();
      table.Columns.Add (c_entryIdColumnName);
      table.Columns.Add (c_lastModificationTimeColumnId);

      while (!table.EndOfTable)
      {
        var row = table.GetNextRow();
        var entryId = (string) row[c_entryIdColumnName];
        var lastModificationTime = (DateTime) row[c_lastModificationTimeColumnId];
        events.Add (new EntityIdWithVersion<string, DateTime> (entryId, lastModificationTime));
      }

      return events;
    }


    private static readonly CultureInfo _enUsCultureInfo = CultureInfo.GetCultureInfo ("en-US");
    private string ToOutlookDateString (DateTime value)
    {
      return value.ToString ("g", _enUsCultureInfo);
    }

    public IEnumerable<AppointmentItem> GetEvents (IEnumerable<string> ids)
    {
      var storeId = _calendarFolder.StoreID;
      return ids.Select (id => (AppointmentItem) _mapiNameSpace.GetItemFromID (id, storeId)).ToList();
    }


    public AppointmentItem CreateNewEvent ()
    {
      var appointment = (AppointmentItem) _calendarFolder.Items.Add (OlItemType.olAppointmentItem);
      return appointment;
    }
  }
}