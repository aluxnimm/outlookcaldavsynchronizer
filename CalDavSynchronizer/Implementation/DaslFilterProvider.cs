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
    private const string c_appointmentFilterCistartswith= "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" ci_startswith 'IPM.Appointment'";
    private const string c_appointmentFilterLike = "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" like 'IPM.Appointment%'";
    private const string c_appointmentFilterExact = "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" = 'IPM.Appointment'";

    private const string c_taskFilterCistartswith = "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" ci_startswith 'IPM.Task'";
    private const string c_taskFilterLike = "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" like 'IPM.Task%'";
    private const string c_taskFilterExact = "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" = 'IPM.Task'";

    private const string c_contactFilterCistartswith = "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" ci_startswith 'IPM.Contact'";
    private const string c_contactFilterLike = "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" like 'IPM.Contact%'";
    private const string c_contactFilterExact = "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" = 'IPM.Contact'";

    private const string c_distListFilterCistartswith = "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" ci_startswith 'IPM.DistList'";
    private const string c_distListFilterLike = "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" like 'IPM.DistList%'";
    private const string c_distListFilterExact = "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" = 'IPM.DistList'";


    private string _appointmentFilter;
    private string _taskFilter;
    private string _contactFilter;
    private string _distListFilter;
    
    private bool _doIncludeCustomMessageClasses;

    public DaslFilterProvider (bool doIncludeCustomMessageClasses)
    {
      SetDoIncludeCustomMessageClasses (doIncludeCustomMessageClasses);
    }

    public void SetDoIncludeCustomMessageClasses (bool value)
    {
      _doIncludeCustomMessageClasses = value;
      _appointmentFilter = value
          ? c_appointmentFilterCistartswith
          : c_appointmentFilterExact;
      _taskFilter = value
          ? c_taskFilterCistartswith
          : c_taskFilterExact;
      _contactFilter = value
          ? c_contactFilterCistartswith
          : c_contactFilterExact;
      _distListFilter = value
         ? c_distListFilterCistartswith
         : c_distListFilterExact;
    }
    
    /// <param name="isInstantSearchEnabled">specifies, if the filter should be created for a folder on which instant search is enabled</param>
    public string GetAppointmentFilter (bool isInstantSearchEnabled) => 
      isInstantSearchEnabled ? _appointmentFilter : _doIncludeCustomMessageClasses ? c_appointmentFilterLike : c_appointmentFilterExact;

    public string GetTaskFilter (bool isInstantSearchEnabled) => 
      isInstantSearchEnabled ? _taskFilter : _doIncludeCustomMessageClasses ? c_taskFilterLike : c_taskFilterExact;

    public string GetContactFilter (bool isInstantSearchEnabled) => 
      isInstantSearchEnabled ? _contactFilter : _doIncludeCustomMessageClasses ? c_contactFilterLike : c_contactFilterExact;

    public string GetDistListFilter(bool isInstantSearchEnabled) =>
      isInstantSearchEnabled ? _distListFilter : _doIncludeCustomMessageClasses ? c_distListFilterLike : c_distListFilterExact;
    
  }
}