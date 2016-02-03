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
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync;
using GenSync.EntityMapping;
using GenSync.EntityRepositories;
using GenSync.Logging;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Task = Google.Apis.Tasks.v1.Data.Task;

namespace CalDavSynchronizer.Implementation.GoogleTasks
{
  class GoogleTaskRepository : IEntityRepository<Task, string, string>
  {
    private readonly TasksService _taskService;
    private readonly TaskList _taskList;

    public GoogleTaskRepository (TasksService taskService, TaskList taskList)
    {
      if (taskService == null)
        throw new ArgumentNullException (nameof (taskService));
      if (taskList == null)
        throw new ArgumentNullException (nameof (taskList));

      _taskService = taskService;
      _taskList = taskList;
    }


    public System.Threading.Tasks.Task Delete (string entityId, string version)
    {
      _taskService.Tasks.Delete (_taskList.Id, entityId);
      return System.Threading.Tasks.Task.FromResult (0);
    }

    public Task<EntityVersion<string, string>> Update (string entityId, string version, Task entityToUpdate, Func<Task, Task> entityModifier)
    {
      throw new NotImplementedException();
    }

    public async Task<EntityVersion<string, string>> Create (Func<Task, Task> entityInitializer)
    {
      var task = entityInitializer (new Task());
      var result = await _taskService.Tasks.Insert (task, _taskList.Id).ExecuteAsync();
      return EntityVersion.Create (result.Id, result.ETag);
    }

    public Task<IReadOnlyList<EntityVersion<string, string>>> GetVersions (IEnumerable<IdWithAwarenessLevel<string>> idsOfEntitiesToQuery)
    {
      throw new NotSupportedException();
    }

    public async Task<IReadOnlyList<EntityVersion<string, string>>> GetAllVersions (IEnumerable<string> idsOfknownEntities)
    {
      var request = _taskService.Tasks.List (_taskList.Id);
      request.Fields = "Etag";
      var result = await request.ExecuteAsync();
      return result.Items.Select (t => EntityVersion.Create (t.Id, t.ETag)).ToArray();
    }

    public async Task<IReadOnlyList<EntityWithId<string, Task>>> Get (ICollection<string> ids, ILoadEntityLogger logger)
    {
      var request = _taskService.Tasks.List (_taskList.Id);
      var result = await request.ExecuteAsync();
      return result.Items.Select (t => EntityWithId.Create (t.Id, t)).ToArray();
    }

    public void Cleanup (IReadOnlyDictionary<string, Task> entities)
    {
    }
  }
}