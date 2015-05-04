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
using CalDavSynchronizer.Generic.EntityRepositories;
using CalDavSynchronizer.Generic.EntityVersionManagement;
using CalDavSynchronizer.Generic.ProgressReport;
using CalDavSynchronizer.Implementation.ComWrappers;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation
{
  internal class OutlookAppointmentRepository : IEntityRepository<AppointmentItemWrapper, string, DateTime>
  {
    private readonly Folder _calendarFolder;
    private readonly NameSpace _mapiNameSpace;

    public OutlookAppointmentRepository (Folder calendarFolder, NameSpace mapiNameSpace)
    {
      if (calendarFolder == null)
        throw new ArgumentNullException ("calendarFolder");
      if (mapiNameSpace == null)
        throw new ArgumentNullException ("mapiNameSpace");

      if (calendarFolder.DefaultItemType != OlItemType.olAppointmentItem)
        throw new ArgumentException(string.Format("Wrong ItemType in folder <{0}>. It should be a calendar folder.", calendarFolder.Name));

      _calendarFolder = calendarFolder;
      _mapiNameSpace = mapiNameSpace;
    }

    private const string c_entryIdColumnName = "EntryID";

    public Dictionary<string, DateTime> GetVersions (DateTime fromUtc, DateTime toUtc)
    {
      var events = new Dictionary<string, DateTime>();

      string filter = String.Format ("[Start] < '{0}' And [End] > '{1}'", ToOutlookDateString (toUtc), ToOutlookDateString (fromUtc));
      using (var tableWrapper = GenericComObjectWrapper.Create ((Table) _calendarFolder.GetTable (filter)))
      {
        var table = tableWrapper.Inner;
        table.Columns.RemoveAll();
        table.Columns.Add (c_entryIdColumnName);

        var storeId = _calendarFolder.StoreID;

        while (!table.EndOfTable)
        {
          var row = table.GetNextRow();
          var entryId = (string) row[c_entryIdColumnName];
          using (var appointmentWrapper = GenericComObjectWrapper.Create ((AppointmentItem) _mapiNameSpace.GetItemFromID (entryId, storeId)))
          {
            events.Add (appointmentWrapper.Inner.EntryID, appointmentWrapper.Inner.LastModificationTime);
          }
        }
      }
      return events;
    }

    private static readonly CultureInfo _currentCultureInfo = CultureInfo.CurrentCulture;

    private string ToOutlookDateString (DateTime value)
    {
      return value.ToString ("g", _currentCultureInfo);
    }


    public IReadOnlyDictionary<string, AppointmentItemWrapper> Get (ICollection<string> ids, ITotalProgress progress)
    {
      using (var stepProgress = progress.StartStep (ids.Count, string.Format ("Loading {0} entities from Outlook...", ids.Count)))
      {
        var storeId = _calendarFolder.StoreID;
        var result = ids.ToDictionary (id => id, id => new AppointmentItemWrapper ((AppointmentItem) _mapiNameSpace.GetItemFromID (id, storeId), entryId => (AppointmentItem) _mapiNameSpace.GetItemFromID (entryId, storeId)));
        stepProgress.IncreaseBy (ids.Count);
        return result;
      }
    }

    public void Cleanup (IReadOnlyDictionary<string, AppointmentItemWrapper> entities)
    {
      foreach (var appointmentItemWrapper in entities.Values)
        appointmentItemWrapper.Dispose();

    }

    public EntityIdWithVersion<string, DateTime> Update (string entityId, AppointmentItemWrapper entityToUpdate, Func<AppointmentItemWrapper, AppointmentItemWrapper> entityModifier)
    {
      entityToUpdate = entityModifier (entityToUpdate);
      entityToUpdate.Inner.Save();
      return new EntityIdWithVersion<string, DateTime> (entityToUpdate.Inner.EntryID, entityToUpdate.Inner.LastModificationTime);
    }

    public bool Delete (string entityId)
    {
      using (var appointment = Get (new[] { entityId }, NullTotalProgress.Instance).Values.SingleOrDefault())
      {
        if (appointment != null)
        {
          appointment.Inner.Delete();
          return true;
        }
        else
        {
          return false;
        }
      }
    }

    public EntityIdWithVersion<string, DateTime> Create (Func<AppointmentItemWrapper, AppointmentItemWrapper> entityInitializer)
    {
      using (var wrapper = new AppointmentItemWrapper ((AppointmentItem) _calendarFolder.Items.Add (OlItemType.olAppointmentItem), entryId => (AppointmentItem) _mapiNameSpace.GetItemFromID (entryId, _calendarFolder.StoreID)))
      {
        using (var initializedWrapper = entityInitializer (wrapper))
        {
          initializedWrapper.Inner.Save();
          var result = new EntityIdWithVersion<string, DateTime> (initializedWrapper.Inner.EntryID, initializedWrapper.Inner.LastModificationTime);
          return result;
        }
      }
    }
  }
}