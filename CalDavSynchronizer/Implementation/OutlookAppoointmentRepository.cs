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
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.Generic.EntityVersionManagement;
using CalDavSynchronizer.Generic.ProgressReport;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation
{
  internal class OutlookAppoointmentRepository : IEntityRepository<AppointmentItem, string, DateTime>
  {
    private readonly MAPIFolder _calendarFolder;
    private readonly NameSpace _mapiNameSpace;

    public OutlookAppoointmentRepository (MAPIFolder calendarFolder, NameSpace mapiNameSpace)
    {
      if (calendarFolder == null)
        throw new ArgumentNullException ("calendarFolder");
      if (mapiNameSpace == null)
        throw new ArgumentNullException ("mapiNameSpace");
      _calendarFolder = calendarFolder;
      _mapiNameSpace = mapiNameSpace;
    }

    private const string c_entryIdColumnName = "EntryID";

    public Dictionary<string, DateTime> GetVersions (DateTime fromUtc, DateTime toUtc)
    {
      var events = new Dictionary<string, DateTime>();

      string filter = String.Format ("[Start] > '{0}' And [End] < '{1}'", ToOutlookDateString (fromUtc), ToOutlookDateString (toUtc));

      var table = _calendarFolder.GetTable (filter);
      table.Columns.RemoveAll();
      table.Columns.Add (c_entryIdColumnName);

      var storeId = _calendarFolder.StoreID;

      while (!table.EndOfTable)
      {
        var row = table.GetNextRow();
        var entryId = (string) row[c_entryIdColumnName];
        var appointment = (AppointmentItem) _mapiNameSpace.GetItemFromID (entryId, storeId);
        events.Add (appointment.EntryID, appointment.LastModificationTime);
      }

      return events;
    }


    private static readonly CultureInfo _enUsCultureInfo = CultureInfo.GetCultureInfo ("en-US");

    private string ToOutlookDateString (DateTime value)
    {
      return value.ToString ("g", _enUsCultureInfo);
    }


    public IReadOnlyDictionary<string, AppointmentItem> Get (ICollection<string> ids, ITotalProgress progress)
    {
      using (var stepProgress = progress.StartStep (ids.Count))
      {
        var storeId = _calendarFolder.StoreID;
        var result = ids.ToDictionary (id => id, id => (AppointmentItem) _mapiNameSpace.GetItemFromID (id, storeId));
        stepProgress.IncreaseBy (ids.Count);
        return result;
      }
    }

    public EntityIdWithVersion<string, DateTime> Update (string entityId, AppointmentItem entityToUpdate, Func<AppointmentItem, AppointmentItem> entityModifier)
    {
      var appointment = Get (new[] { entityId }, NullTotalProgress.Instance).Single().Value;
      appointment = entityModifier (appointment);
      appointment.Save();
      return new EntityIdWithVersion<string, DateTime> (appointment.EntryID, appointment.LastModificationTime);
    }

    public bool Delete (string entityId)
    {
      var appointment = Get (new[] { entityId }, NullTotalProgress.Instance).Values.SingleOrDefault();
      if (appointment != null)
      {
        appointment.Delete();
        return true;
      }
      else
      {
        return false;
      }
    }

    public EntityIdWithVersion<string, DateTime> Create (Func<AppointmentItem, AppointmentItem> entityInitializer)
    {
      var appointment = (AppointmentItem) _calendarFolder.Items.Add (OlItemType.olAppointmentItem);
      appointment = entityInitializer (appointment);
      appointment.Save();
      return new EntityIdWithVersion<string, DateTime> (appointment.EntryID, appointment.LastModificationTime);
    }
  }
}