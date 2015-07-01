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
using CalDavSynchronizer.Generic.InitialEntityMatching;
using CalDavSynchronizer.Implementation.ComWrappers;
using DDay.iCal;

namespace CalDavSynchronizer.Implementation.Events
{
  internal class InitialEventEntityMatcher : InitialEntityMatcherByPropertyGrouping<AppointmentItemWrapper, string, DateTime, string, IICalendar, Uri, string, string>
  {
    public InitialEventEntityMatcher (IEqualityComparer<Uri> btypeIdEqualityComparer)
        : base(btypeIdEqualityComparer)
    {
    }

    protected override bool AreEqual (AppointmentItemWrapper atypeEntity, IICalendar btypeEntity)
    {
      var evt = btypeEntity.Events[0];

      return
          evt.Summary == atypeEntity.Inner.Subject &&
          (evt.IsAllDay && atypeEntity.Inner.AllDayEvent ||
           evt.Start.UTC == atypeEntity.Inner.StartUTC.ToUniversalTime() &&
           evt.DTEnd.UTC == atypeEntity.Inner.EndUTC.ToUniversalTime());
    }

    protected override string GetAtypePropertyValue (AppointmentItemWrapper atypeEntity)
    {
      return (atypeEntity.Inner.Subject != null ? atypeEntity.Inner.Subject.ToLower() : string.Empty);
    }

    protected override string GetBtypePropertyValue (IICalendar btypeEntity)
    {
      return (btypeEntity.Events[0].Summary != null ? btypeEntity.Events[0].Summary.ToLower() : string.Empty);
    }

    protected override string MapAtypePropertyValue (string value)
    {
      return value;
    }
  }
}