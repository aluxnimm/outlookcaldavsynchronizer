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
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.EntityMapping;
using GenSync.Logging;
using Microsoft.Office.Interop.Outlook;
using Task = Google.Apis.Tasks.v1.Data.Task;

namespace CalDavSynchronizer.Implementation.GoogleTasks
{
  class GoogleTaskMapper : IEntityMapper<ITaskItemWrapper, Task, int>
  {
    public GoogleTaskMapper()
    {

    }

    public Task<Task> Map1To2 (ITaskItemWrapper source, Task target, IEntitySynchronizationLogger logger, int context)
    {
      target.Title = source.Inner.Subject;
      target.Notes = source.Inner.Body;

      if (source.Inner.DueDate != OutlookUtility.OUTLOOK_DATE_NONE)
      {
        target.Due = new DateTime (source.Inner.DueDate.Year, source.Inner.DueDate.Month, source.Inner.DueDate.Day, 23, 59, 59, DateTimeKind.Utc);
      }
      else
      {
        target.Due = null;
      }
      if (source.Inner.Complete && source.Inner.DateCompleted != OutlookUtility.OUTLOOK_DATE_NONE)
      {
        target.Completed = source.Inner.DateCompleted.ToUniversalTime();
      }
      else
      {
        target.Completed = null;
      }

      target.Status = MapStatus1To2 (source.Inner.Status);

      return System.Threading.Tasks.Task.FromResult(target); 
    }

    private string MapStatus1To2 (OlTaskStatus value)
    {
      switch (value)
      {
        case OlTaskStatus.olTaskComplete:
          return "completed";
        case OlTaskStatus.olTaskDeferred:
        case OlTaskStatus.olTaskInProgress:
        case OlTaskStatus.olTaskWaiting:
        case OlTaskStatus.olTaskNotStarted:
          return "needsAction";
      }
      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }

    public Task<ITaskItemWrapper> Map2To1 (Task source, ITaskItemWrapper target, IEntitySynchronizationLogger logger, int context)
    {
      target.Inner.Subject = source.Title;
      target.Inner.Body = source.Notes;

      if (source.Due != null)
      {
        if (source.Due < target.Inner.StartDate)
        {
          target.Inner.StartDate = source.Due.Value.ToUniversalTime();
        }
        target.Inner.DueDate = source.Due.Value.ToUniversalTime();
      }
      else
      {
        target.Inner.DueDate = OutlookUtility.OUTLOOK_DATE_NONE;
      }

      if (source.Completed != null)
      {
        target.Inner.DateCompleted = source.Completed.Value.Date;
        target.Inner.Complete = true;
      }
      else
      {
        target.Inner.Complete = false;
      }

      target.Inner.Status = MapStatus2To1 (source.Status);

      return System.Threading.Tasks.Task.FromResult(target);
    }

    private OlTaskStatus MapStatus2To1 (string status)
    {
      if (status == "completed")
        return OlTaskStatus.olTaskComplete;
      else
        return OlTaskStatus.olTaskInProgress;
    }
  }
}