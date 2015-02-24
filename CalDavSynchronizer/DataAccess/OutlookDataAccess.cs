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
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.DataAccess
{
  internal class OutlookDataAccess : IOutlookDataAccess
  {
    private readonly MAPIFolder _calendarFolder;

    public OutlookDataAccess (MAPIFolder calendarFolder)
    {
      if (calendarFolder == null)
        throw new ArgumentNullException ("calendarFolder");
      _calendarFolder = calendarFolder;
    }


    public IEnumerable<AppointmentItem> GetEvents (DateTime? fromUtc, DateTime? toUtc)
    {
      List<AppointmentItem> events = new List<AppointmentItem>();

      foreach (AppointmentItem evt in _calendarFolder.Items)
      {
        if (fromUtc == null || evt.StartUTC >= fromUtc)
          if (toUtc == null || evt.EndUTC <= toUtc)
            events.Add (evt);
      }
      return events;
    }


    public IEnumerable<AppointmentItem> GetEvents (IEnumerable<string> ids)
    {
      HashSet<string> idsHashSet = new HashSet<string> (ids);
      List<AppointmentItem> events = new List<AppointmentItem>();

      foreach (AppointmentItem evt in _calendarFolder.Items)
      {
        if (idsHashSet.Contains (evt.EntryID))
          events.Add (evt);
      }
      return events;
    }


    public AppointmentItem CreateNewEvent ()
    {
      var appointment = (AppointmentItem) _calendarFolder.Items.Add (OlItemType.olAppointmentItem);
      return appointment;
    }
  }
}