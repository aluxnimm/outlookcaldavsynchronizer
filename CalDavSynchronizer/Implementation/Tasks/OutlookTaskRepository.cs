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
using System.Text;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.Events;
using log4net;

namespace CalDavSynchronizer.Implementation.Tasks
{
  public class OutlookTaskRepository : IEntityRepository<TaskItemWrapper, string, DateTime, int>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
    private readonly NameSpace _mapiNameSpace;
    private readonly string _folderId;
    private readonly string _folderStoreId;
    private readonly IDaslFilterProvider _daslFilterProvider;
    private readonly TaskMappingConfiguration _configuration;

    public OutlookTaskRepository (NameSpace mapiNameSpace, string folderId, string folderStoreId, IDaslFilterProvider daslFilterProvider, TaskMappingConfiguration configuration)
    {
      if (mapiNameSpace == null)
        throw new ArgumentNullException (nameof (mapiNameSpace));
      if (daslFilterProvider == null)
        throw new ArgumentNullException (nameof (daslFilterProvider));
      if (configuration == null)
        throw new ArgumentNullException (nameof(configuration));
      if (String.IsNullOrEmpty (folderId))
        throw new ArgumentException ("Argument is null or empty", nameof (folderId));
      if (String.IsNullOrEmpty (folderStoreId))
        throw new ArgumentException ("Argument is null or empty", nameof (folderStoreId));

      _mapiNameSpace = mapiNameSpace;
      _folderId = folderId;
      _folderStoreId = folderStoreId;
      _daslFilterProvider = daslFilterProvider;
      _configuration = configuration;
    }

    private const string c_entryIdColumnName = "EntryID";

    public Task<IReadOnlyList<EntityVersion<string, DateTime>>> GetVersions (IEnumerable<IdWithAwarenessLevel<string>> idsOfEntitiesToQuery, int context)
    {
      var result = new List<EntityVersion<string, DateTime>>();

      foreach (var id in idsOfEntitiesToQuery)
      {
        var task = _mapiNameSpace.GetEntryOrNull<TaskItem> (id.Id, _folderId, _folderStoreId);
        if (task != null)
        {
          try
          {
            if (id.IsKnown || DoesMatchCategoryCriterion (task))
            {
              result.Add (EntityVersion.Create (id.Id, task.LastModificationTime));
            }
          }
          finally
          {
            Marshal.FinalReleaseComObject (task);
          }
        }
      }
      return Task.FromResult<IReadOnlyList<EntityVersion<string, DateTime>>> (result);
    }

    private GenericComObjectWrapper<Folder> CreateFolderWrapper ()
    {
      return GenericComObjectWrapper.Create ((Folder) _mapiNameSpace.GetFolderFromID (_folderId, _folderStoreId));
    }

    public Task<IReadOnlyList<EntityVersion<string, DateTime>>> GetAllVersions (IEnumerable<string> idsOfknownEntities, int context)
    {
      List<EntityVersion<string,DateTime>> tasks;
      Func<TaskItem, EntityVersion<string, DateTime>> selector = a => new EntityVersion<string, DateTime>(a.EntryID, a.LastModificationTime);

      using (var taskFolderWrapper = CreateFolderWrapper())
      {
        bool isInstantSearchEnabled = false;

        try
        {
          using (var store = GenericComObjectWrapper.Create (taskFolderWrapper.Inner.Store))
          {
            if (store.Inner != null)
              isInstantSearchEnabled = store.Inner.IsInstantSearchEnabled;
          }
        }
        catch (COMException)
        {
          s_logger.Info ("Can't access IsInstantSearchEnabled property of store, defaulting to false.");
        }

        var filterBuilder = new StringBuilder(_daslFilterProvider.GetTaskFilter (isInstantSearchEnabled));
        if (_configuration.UseTaskCategoryAsFilter)
        {
          OutlookEventRepository.AddCategoryFilter (filterBuilder, _configuration.TaskCategory, _configuration.InvertTaskCategoryFilter);
        }

        s_logger.InfoFormat ("Using Outlook DASL filter: {0}", filterBuilder.ToString());

        tasks = QueryFolder(_mapiNameSpace, taskFolderWrapper, filterBuilder, selector);
      }

      if (_configuration.UseTaskCategoryAsFilter)
      {
        var knownEntitesThatWereFilteredOut = idsOfknownEntities.Except(tasks.Select(e => e.Id));
        tasks.AddRange(
            knownEntitesThatWereFilteredOut
                .Select(id => _mapiNameSpace.GetEntryOrNull<TaskItem>(id, _folderId, _folderStoreId))
                .Where(i => i != null)
                .ToSafeEnumerable()
                .Select(selector));
      }

      return Task.FromResult<IReadOnlyList<EntityVersion<string, DateTime>>> (tasks);
    }

    public static List<EntityVersion<string, DateTime>> QueryFolder(
    NameSpace session,
    GenericComObjectWrapper<Folder> taskFolderWrapper,
    StringBuilder filterBuilder)
    {
      return QueryFolder(
          session,
          taskFolderWrapper,
          filterBuilder,
          a => new EntityVersion<string, DateTime>(a.EntryID, a.LastModificationTime));
    }

    static List<T> QueryFolder<T>(
        NameSpace session,
        GenericComObjectWrapper<Folder> taskFolderWrapper,
        StringBuilder filterBuilder,
        Func<TaskItem, T> selector)
      where T : IEntity<string>
    {
      var tasks = new List<T>();

      using (var tableWrapper = GenericComObjectWrapper.Create(
          taskFolderWrapper.Inner.GetTable(filterBuilder.ToString())))
      {
        var table = tableWrapper.Inner;
        table.Columns.RemoveAll();
        table.Columns.Add(c_entryIdColumnName);

        var storeId = taskFolderWrapper.Inner.StoreID;

        while (!table.EndOfTable)
        {
          var row = table.GetNextRow();
          var entryId = (string)row[c_entryIdColumnName];
          try
          {
            using (var taskWrapper = GenericComObjectWrapper.Create((TaskItem)session.GetItemFromID(entryId, storeId)))
            {
              tasks.Add(selector (taskWrapper.Inner));
            }
          }
          catch (COMException ex)
          {
            s_logger.Error("Could not fetch TaskItem, skipping.", ex);
          }
        }
      }
      return tasks;
    }

    private static readonly CultureInfo _currentCultureInfo = CultureInfo.CurrentCulture;

    private string ToOutlookDateString (DateTime value)
    {
      return value.ToString ("g", _currentCultureInfo);
    }

#pragma warning disable 1998
    public async Task<IReadOnlyList<EntityWithId<string, TaskItemWrapper>>> Get (ICollection<string> ids, ILoadEntityLogger logger, int context)
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

    public Task VerifyUnknownEntities (Dictionary<string, DateTime> unknownEntites, int context)
    {
      return Task.FromResult (0);
    }

    private bool DoesMatchCategoryCriterion (TaskItem item)
    {
      if (!_configuration.UseTaskCategoryAsFilter)
        return true;

      var categoryCsv = item.Categories;

      if (string.IsNullOrEmpty (categoryCsv))
        return _configuration.InvertTaskCategoryFilter;

      var found = item.Categories
          .Split(new[] { CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries)
          .Select(c => c.Trim())
          .Any(c => c == _configuration.TaskCategory);
      return _configuration.InvertTaskCategoryFilter ? !found : found;
    }


    public void Cleanup (IReadOnlyDictionary<string, TaskItemWrapper> entities)
    {
      foreach (var wrapper in entities.Values)
        wrapper.Dispose();
    }

    public Task<EntityVersion<string, DateTime>> TryUpdate (
        string entityId,
        DateTime entityVersion,
        TaskItemWrapper entityToUpdate,
        Func<TaskItemWrapper, TaskItemWrapper> entityModifier,
        int context)
    {
      entityToUpdate = entityModifier (entityToUpdate);
      entityToUpdate.Inner.Save();
      return Task.FromResult (new EntityVersion<string, DateTime> (entityToUpdate.Inner.EntryID, entityToUpdate.Inner.LastModificationTime));
    }

    public Task<bool> TryDelete (
      string entityId, 
      DateTime version,
      int context)
    {
      var entityWithId = Get (new[] { entityId }, NullLoadEntityLogger.Instance, 0).Result.SingleOrDefault ();
      if (entityWithId == null)
        return Task.FromResult (true);

      using (var entity = entityWithId.Entity)
      {
        entity.Inner.Delete();
      }

      return Task.FromResult (true);
    }

    public Task<EntityVersion<string, DateTime>> Create (Func<TaskItemWrapper, TaskItemWrapper> entityInitializer, int context)
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
  }
}