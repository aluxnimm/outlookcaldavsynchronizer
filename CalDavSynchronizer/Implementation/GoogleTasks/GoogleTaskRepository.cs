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
using Google.Apis.Discovery;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Task = Google.Apis.Tasks.v1.Data.Task;

namespace CalDavSynchronizer.Implementation.GoogleTasks
{
  public class GoogleTaskRepository : IEntityRepository<string, string, Task, int>
  {
    private readonly TasksService _tasksService;
    private readonly TaskList _taskList;

    public GoogleTaskRepository (TasksService tasksService, TaskList taskList)
    {
      if (tasksService == null)
        throw new ArgumentNullException (nameof (tasksService));
      if (taskList == null)
        throw new ArgumentNullException (nameof (taskList));

      _tasksService = tasksService;
      _taskList = taskList;
    }


    public async System.Threading.Tasks.Task<bool> TryDelete (string entityId, string version, int context, IEntitySynchronizationLogger logger)
    {
      var deleteRequest =   _tasksService.Tasks.Delete (_taskList.Id, entityId);
      // Todo: how to set etag ?
      await deleteRequest.ExecuteAsync();
      return true;
    }

    public async Task<EntityVersion<string, string>> TryUpdate (string entityId, string version, Task entityToUpdate, Func<Task, Task<Task>> entityModifier, int context, IEntitySynchronizationLogger logger)
    {
      entityToUpdate = await entityModifier (entityToUpdate);
      var updateRequest = _tasksService.Tasks.Update (entityToUpdate, _taskList.Id, entityId);
      updateRequest.ETagAction = Google.Apis.ETagAction.IfMatch;
      var result = await updateRequest.ExecuteAsync();
      return EntityVersion.Create (result.Id, result.ETag);
    }

    public async Task<EntityVersion<string, string>> Create (Func<Task, Task<Task>> entityInitializer, int context)
    {
      var task = await entityInitializer (new Task());
      var result = await _tasksService.Tasks.Insert (task, _taskList.Id).ExecuteAsync();
      return EntityVersion.Create (result.Id, result.ETag);
    }

    public async Task<IEnumerable<EntityVersion<string, string>>> GetVersions (IEnumerable<IdWithAwarenessLevel<string>> idsOfEntitiesToQuery, int context, IGetVersionsLogger logger)
    {
      var idsOfEntitiesToQueryDictionary = idsOfEntitiesToQuery.ToDictionary (i => i.Id);
      return (await GetAllVersions (new string[] { }, context, logger))
        .Where (v => idsOfEntitiesToQueryDictionary.ContainsKey (v.Id))
        .ToArray();
    }

    public async Task<IEnumerable<EntityVersion<string, string>>> GetAllVersions(IEnumerable<string> idsOfknownEntities, int context, IGetVersionsLogger logger)
    {
      var request = _tasksService.Tasks.List(_taskList.Id);
      request.Fields = "items(etag,id),nextPageToken";
      var tasks = new List<EntityVersion<string, string>>();

      Google.Apis.Tasks.v1.Data.Tasks result = null;
      do
      {
        request.PageToken = result?.NextPageToken;
        result = await request.ExecuteAsync();
        if (result.Items != null)
          tasks.AddRange(result.Items.Select(t => EntityVersion.Create(t.Id, t.ETag)));
      } while (result.NextPageToken != null);

      return tasks;
    }

    public async Task<IEnumerable<EntityWithId<string, Task>>> Get (ICollection<string> ids, ILoadEntityLogger logger, int context)
    {
      var items = new List<EntityWithId<string, Task>> ();

      // All the requests are awaited sequentially in a loop, because creating all the requests at once ant awaiting them after the loop would
      // probably open up too many connections
      foreach (var id in ids)
      {
        var item = await _tasksService.Tasks.Get (_taskList.Id, id).ExecuteAsync();
        items.Add (EntityWithId.Create (item.Id, item));
      }

      return items;
    }

    public System.Threading.Tasks.Task VerifyUnknownEntities (Dictionary<string, string> unknownEntites, int context)
    {
      return System.Threading.Tasks.Task.FromResult (0);
    }

    public void Cleanup(Task entity)
    {
      
    }

    public void Cleanup(IEnumerable<Task> entities)
    {

    }
  }
}