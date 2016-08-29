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
using System.Text;
using System.Threading.Tasks;
using DDay.iCal;

namespace CalDavSynchronizer.Implementation
{
  public class TimeZoneCacheProvider
  {
    private readonly Dictionary<string, ITimeZone> _tzOutlookMap;
    private readonly Dictionary<string, ITimeZone> _tzHistoricalMap;

    public TimeZoneCacheProvider()
    {
      _tzOutlookMap = new Dictionary<string, ITimeZone>();
      _tzHistoricalMap = new Dictionary<string, ITimeZone>();
    }

    public ITimeZone GetTzOrNull (string tzId, bool includeHistoricalData)
    {
      if (includeHistoricalData)
      {
        return _tzHistoricalMap.ContainsKey (tzId) ? _tzHistoricalMap[tzId] : null;
      }
      else
      {
        return _tzOutlookMap.ContainsKey (tzId) ? _tzOutlookMap[tzId] : null;
      }
    }

    public void AddTz (string tzId, ITimeZone timeZone, bool includeHistoricalData)
    {
      if (includeHistoricalData)
      {
        _tzHistoricalMap.Add (tzId, timeZone);
      }
      else
      {
        _tzOutlookMap.Add (tzId, timeZone);
      }
    }
  }
}
