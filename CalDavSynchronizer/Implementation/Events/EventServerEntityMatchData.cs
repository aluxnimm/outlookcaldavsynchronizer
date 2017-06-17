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
using DDay.iCal;

namespace CalDavSynchronizer.Implementation.Events
{
  public class EventServerEntityMatchData
  {
    public string Summary { get; }
    public bool IsAllDay { get; }
    public DateTime Start { get;  }
    public (DateTime Value,bool IsUniversalTime)? End { get;  }
    public (DateTime Value,bool IsUniversalTime)? DTEnd { get;  }
    public bool IsStartUniversalTime { get;  }

    public EventServerEntityMatchData(IICalendar calendar)
    {
      var evt = calendar.Events[0];
      Summary = evt.Summary;
      IsAllDay = evt.IsAllDay;
      Start = evt.Start.Value;
      IsStartUniversalTime = evt.Start.IsUniversalTime;
      End = evt.End != null ? (evt.End.Value, evt.End.IsUniversalTime) : ((DateTime, bool)?)null;
      DTEnd = evt.DTEnd != null ? (evt.DTEnd.Value, evt.DTEnd.IsUniversalTime) : ((DateTime, bool)?)null;
    }
  }
}