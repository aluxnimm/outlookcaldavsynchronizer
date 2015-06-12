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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Diagnostics;
using CalDavSynchronizer.Generic.EntityRepositories;
using CalDavSynchronizer.Generic.EntityVersionManagement;
using CalDavSynchronizer.Generic.ProgressReport;
using DDay.iCal;
using DDay.iCal.Serialization;
using DDay.iCal.Serialization.iCalendar;
using log4net;

namespace CalDavSynchronizer.Implementation.Tasks
{
  public class CalDavTaskRepository : IEntityRepository<ITodo, Uri, string>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly ICalDavDataAccess _calDavDataAccess;
    private readonly IStringSerializer _calendarSerializer;

    public CalDavTaskRepository (ICalDavDataAccess calDavDataAccess, IStringSerializer calendarSerializer)
    {
      _calDavDataAccess = calDavDataAccess;
      _calendarSerializer = calendarSerializer;
    }

    public Dictionary<Uri, string> GetVersions (DateTime from, DateTime to)
    {
      using (AutomaticStopwatch.StartInfo (s_logger, "CalDavEventRepository.GetVersions"))
      {
        return _calDavDataAccess.GetEvents (from, to);
      }
    }

    public IReadOnlyDictionary<Uri, ITodo> Get (ICollection<Uri> ids, ITotalProgress progress)
    {
      throw new NotImplementedException();
    }

    public void Cleanup (IReadOnlyDictionary<Uri, ITodo> entities)
    {
      // nothing to do
    }

    private IReadOnlyDictionary<Uri, IICalendar> ParallelDeserialize (IReadOnlyDictionary<Uri, string> serializedEvents)
    {
      throw new NotImplementedException();
    }


    public bool Delete (Uri entityId)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        return _calDavDataAccess.DeleteEvent (entityId);
      }
    }

    public EntityIdWithVersion<Uri, string> Update (Uri entityId, ITodo entityToUpdate, Func<ITodo, ITodo> entityModifier)
    {
      throw new NotImplementedException();
    }

    public EntityIdWithVersion<Uri, string> Create (Func<ITodo, ITodo> entityInitializer)
    {
      throw new NotImplementedException();
    }
   
  }
}