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
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.Implementation.Tasks;

namespace CalDavSynchronizer.Implementation
{
  public class DaslFilterProvider : IDaslFilterProvider
  {
    private string _appointmentFilter;
    private string _taskFilter;
    private string _contactFilter;

    public DaslFilterProvider (bool doIncludeCustomMessageClasses)
    {
      SetDoIncludeCustomMessageClasses (doIncludeCustomMessageClasses);
    }

    public void SetDoIncludeCustomMessageClasses (bool value)
    {
      _appointmentFilter = value
          ? "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" ci_startswith 'IPM.Appointment'"
          : "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" = 'IPM.Appointment'";
      _taskFilter = value
          ? "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" ci_startswith 'IPM.Task'"
          : "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" = 'IPM.Task'";
      _contactFilter = value
          ? "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" ci_startswith 'IPM.Contact'"
          : "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" = 'IPM.Contact'";
    }

    public string AppointmentFilter => _appointmentFilter;
    public string TaskFilter => _taskFilter;
    public string ContactFilter => _contactFilter;
  }
}