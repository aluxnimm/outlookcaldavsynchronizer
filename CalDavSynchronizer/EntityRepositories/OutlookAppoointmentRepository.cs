// This file is Part of CalDavSynchronizer (https://sourceforge.net/projects/outlookcaldavsynchronizer/)
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
using System.Linq;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.EntityVersionManagement;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.EntityRepositories
{
  internal class OutlookAppoointmentRepository : EntityRepositoryBase<AppointmentItem, string, DateTime>
  {
    private readonly IOutlookDataAccess _outlookDataAccess;

    public OutlookAppoointmentRepository (IOutlookDataAccess outlookDataAccess)
    {
      _outlookDataAccess = outlookDataAccess;
    }

    public override IEnumerable<EntityIdWithVersion<string, DateTime>> GetEntityVersions (DateTime from, DateTime to)
    {
      return _outlookDataAccess
          .GetEvents (from, to)
          .Select (a => new EntityIdWithVersion<string, DateTime> (a.EntryID, a.LastModificationTime))
          .ToArray();
    }

    public override IDictionary<string, AppointmentItem> GetEntities (IEnumerable<string> sourceEntityIds)
    {
      return _outlookDataAccess.GetEvents (sourceEntityIds).ToDictionary (a => a.EntryID);
    }


    public override bool Delete (string entityId)
    {
      var appointment = _outlookDataAccess.GetEvents (new[] { entityId }).SingleOrDefault();
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

    public override EntityIdWithVersion<string, DateTime> Update (string entityId, Func<AppointmentItem, AppointmentItem> entityModifier)
    {
      var appointment = _outlookDataAccess.GetEvents (new[] { entityId }).Single();
      appointment = entityModifier (appointment);
      appointment.Save();
      return new EntityIdWithVersion<string, DateTime> (appointment.EntryID, appointment.LastModificationTime);
    }

    public override EntityIdWithVersion<string, DateTime> Create (Func<AppointmentItem, AppointmentItem> entityInitializer)
    {
      var appointment = _outlookDataAccess.CreateNewEvent();
      appointment = entityInitializer (appointment);
      appointment.Save();
      return new EntityIdWithVersion<string, DateTime> (appointment.EntryID, appointment.LastModificationTime);
    }
  }
}