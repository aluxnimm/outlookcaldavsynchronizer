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

namespace CalDavSynchronizer.Implementation.Tasks
{
  public class OutlookTaskRepository : IEntityRepository<TaskItemWrapper, string, DateTime>
  {
    private readonly Folder _taskFolder;
    private readonly NameSpace _mapiNameSpace;

    public OutlookTaskRepository (Folder taskFolder, NameSpace mapiNameSpace)
    {
      if (taskFolder == null)
        throw new ArgumentNullException ("taskFolder");
      if (mapiNameSpace == null)
        throw new ArgumentNullException ("mapiNameSpace");

      _taskFolder = taskFolder;
      _mapiNameSpace = mapiNameSpace;
    }

    public Dictionary<string, DateTime> GetVersions (DateTime fromUtc, DateTime toUtc)
    {
      // is from and to still relevant for tasks ?
      throw new NotImplementedException();
    }

    public IReadOnlyDictionary<string, TaskItemWrapper> Get (ICollection<string> ids, ITotalProgress progress)
    {
      throw new NotImplementedException ();
    }

    public void Cleanup (IReadOnlyDictionary<string, TaskItemWrapper> entities)
    {
      foreach (var appointmentItemWrapper in entities.Values)
        appointmentItemWrapper.Dispose();
    }

    public EntityIdWithVersion<string, DateTime> Update (string entityId, TaskItemWrapper entityToUpdate, Func<TaskItemWrapper, TaskItemWrapper> entityModifier)
    {
      throw new NotImplementedException ();
    }

    public bool Delete (string entityId)
    {
      throw new NotImplementedException ();
    }

    public EntityIdWithVersion<string, DateTime> Create (Func<TaskItemWrapper, TaskItemWrapper> entityInitializer)
    {
      throw new NotImplementedException ();
    }

  }
}