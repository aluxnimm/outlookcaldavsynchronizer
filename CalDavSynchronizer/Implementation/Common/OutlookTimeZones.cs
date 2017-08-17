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
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Events;
using Microsoft.Office.Interop.Outlook;
using TimeZone = Microsoft.Office.Interop.Outlook.TimeZone;

namespace CalDavSynchronizer.Implementation.Common
{
  class OutlookTimeZones : IOutlookTimeZones
  {
    private readonly Application _application;
    private readonly Dictionary<string, string> _outlookTimeZoneIdByCaseInsensitiveId;

    public OutlookTimeZones(Application application)
    {
      _application = application ?? throw new ArgumentNullException(nameof(application));

      _outlookTimeZoneIdByCaseInsensitiveId = application.TimeZones
        .Cast<TimeZone>()
        .ToSafeEnumerable()
        .ToDictionary(t => t.ID, t => t.ID, StringComparer.OrdinalIgnoreCase);
    }

    public TimeZone CurrentTimeZone => _application.TimeZones.CurrentTimeZone;

    public TimeZone this[string id]
    {
      get
      {
        if (_outlookTimeZoneIdByCaseInsensitiveId.TryGetValue(id, out string outlookId))
          return _application.TimeZones[outlookId];
        else
          return _application.TimeZones[TimeZoneMapper.IanaToWindowsOrNull(id) ?? CurrentTimeZone.ID];
      }
    }
  }
}
