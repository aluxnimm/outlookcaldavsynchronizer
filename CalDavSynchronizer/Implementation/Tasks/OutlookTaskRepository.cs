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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using Microsoft.Office.Interop.Outlook;
using System.Runtime.InteropServices;
using log4net;

namespace CalDavSynchronizer.Implementation.Tasks
{
  public class OutlookTaskRepository : IEntityRepository<TaskItemWrapper, string, DateTime>, IOutlookRepository
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
    private readonly NameSpace _mapiNameSpace;
    private readonly string _folderId;
    private readonly string _folderStoreId;
    private readonly IDaslFilterProvider _daslFilterProvider;

    public OutlookTaskRepository (NameSpace mapiNameSpace, string folderId, string folderStoreId, IDaslFilterProvider daslFilterProvider)
    {
      if (mapiNameSpace == null)
        throw new ArgumentNullException (nameof (mapiNameSpace));
      if (daslFilterProvider == null)
        throw new ArgumentNullException (nameof (daslFilterProvider));
      if (String.IsNullOrEmpty (folderId))
        throw new ArgumentException ("Argument is null or empty", nameof (folderId));
      if (String.IsNullOrEmpty (folderStoreId))
        throw new ArgumentException ("Argument is null or empty", nameof (folderStoreId));

      _mapiNameSpace = mapiNameSpace;
      _folderId = folderId;
      _folderStoreId = folderStoreId;
      _daslFilterProvider = daslFilterProvider;
    }

    private const string c_entryIdColumnName = "EntryID";

    public Task<IReadOnlyList<EntityVersion<string, DateTime>>> GetVersions (IEnumerable<IdWithAwarenessLevel<string>> idsOfEntitiesToQuery)
    {
      return Task.FromResult<IReadOnlyList<EntityVersion<string, DateTime>>> (
          idsOfEntitiesToQuery
              .Select (id => (TaskItem) _mapiNameSpace.GetItemFromID (id.Id, _folderStoreId))
              .ToSafeEnumerable()
              .Select (c => EntityVersion.Create (c.EntryID, c.LastModificationTime))
              .ToList());
    }

    private GenericComObjectWrapper<Folder> CreateFolderWrapper ()
    {
      return GenericComObjectWrapper.Create ((Folder) _mapiNameSpace.GetFolderFromID (_folderId, _folderStoreId));
    }

    public Task<IReadOnlyList<EntityVersion<string, DateTime>>> GetAllVersions (IEnumerable<string> idsOfknownEntities)
    {
      var entities = new List<EntityVersion<string, DateTime>>();

      using (var taskFolderWrapper = CreateFolderWrapper ())
      using (var tableWrapper = 
        GenericComObjectWrapper.Create (taskFolderWrapper.Inner.GetTable (_daslFilterProvider.GetTaskFilter (taskFolderWrapper.Inner.Store.IsInstantSearchEnabled))))
      {
        var table = tableWrapper.Inner;
        table.Columns.RemoveAll();
        table.Columns.Add (c_entryIdColumnName);

        while (!table.EndOfTable)
        {
          var row = table.GetNextRow();
          var entryId = (string) row[c_entryIdColumnName];
          try
          {
            using (var taskItemWrapper = GenericComObjectWrapper.Create ((TaskItem) _mapiNameSpace.GetItemFromID (entryId, _folderStoreId)))
            {
              entities.Add (EntityVersion.Create (taskItemWrapper.Inner.EntryID, taskItemWrapper.Inner.LastModificationTime));
            }
          }
          catch (COMException ex)
          {
            s_logger.Error ("Could not fetch TaskItem, skipping.", ex);
          }
        }
      }

      return Task.FromResult<IReadOnlyList<EntityVersion<string, DateTime>>> (entities);
    }

    private static readonly CultureInfo _currentCultureInfo = CultureInfo.CurrentCulture;

    private string ToOutlookDateString (DateTime value)
    {
      return value.ToString ("g", _currentCultureInfo);
    }

#pragma warning disable 1998
    public async Task<IReadOnlyList<EntityWithId<string, TaskItemWrapper>>> Get (ICollection<string> ids, ILoadEntityLogger logger)
#pragma warning restore 1998
    {
      return ids
          .Select (id => EntityWithId.Create (
              id,
              new TaskItemWrapper (
                  (TaskItem) _mapiNameSpace.GetItemFromID (id, _folderStoreId),
                  entryId => (TaskItem) _mapiNameSpace.GetItemFromID (entryId, _folderStoreId))))
          .ToArray();
    }

    public void Cleanup (IReadOnlyDictionary<string, TaskItemWrapper> entities)
    {
      foreach (var wrapper in entities.Values)
        wrapper.Dispose();
    }

    public Task<EntityVersion<string, DateTime>> Update (
        string entityId,
        DateTime entityVersion,
        TaskItemWrapper entityToUpdate,
        Func<TaskItemWrapper, TaskItemWrapper> entityModifier)
    {
      entityToUpdate = entityModifier (entityToUpdate);
      entityToUpdate.Inner.Save();
      return Task.FromResult (new EntityVersion<string, DateTime> (entityToUpdate.Inner.EntryID, entityToUpdate.Inner.LastModificationTime));
    }

    public Task Delete (string entityId, DateTime version)
    {
      var entityWithId = Get (new[] { entityId }, NullLoadEntityLogger.Instance).Result.SingleOrDefault ();
      if (entityWithId == null)
        return Task.FromResult (0);

      using (var entity = entityWithId.Entity)
      {
        entity.Inner.Delete();
      }

      return Task.FromResult (0);
    }

    public Task<EntityVersion<string, DateTime>> Create (Func<TaskItemWrapper, TaskItemWrapper> entityInitializer)
    {
      using (var taskFolderWrapper = CreateFolderWrapper ())
      using (var wrapper = new TaskItemWrapper ((TaskItem) taskFolderWrapper.Inner.Items.Add (OlItemType.olTaskItem), entryId => (TaskItem) _mapiNameSpace.GetItemFromID (entryId, _folderStoreId)))
      {
        using (var initializedWrapper = entityInitializer (wrapper))
        {
          initializedWrapper.SaveAndReload ();
          var result = new EntityVersion<string, DateTime> (initializedWrapper.Inner.EntryID, initializedWrapper.Inner.LastModificationTime);
          return Task.FromResult (result);
        }
      }
    }

    public bool IsResponsibleForFolder (string folderEntryId, string folderStoreId)
    {
      return folderEntryId == _folderId && folderStoreId == _folderStoreId;
    }
  }
}