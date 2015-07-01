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
using System.Threading.Tasks;
using CalDavSynchronizer.Generic;
using CalDavSynchronizer.Generic.EntityRepositories;
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

    private const string c_entryIdColumnName = "EntryID";

    public IReadOnlyList<EntityIdWithVersion<string, DateTime>> GetVersions (DateTime fromUtc, DateTime toUtc)
    {
      var entities = new List<EntityIdWithVersion<string, DateTime>>();

      using (var tableWrapper = GenericComObjectWrapper.Create ((Table) _taskFolder.GetTable()))
      {
        var table = tableWrapper.Inner;
        table.Columns.RemoveAll();
        table.Columns.Add (c_entryIdColumnName);

        var storeId = _taskFolder.StoreID;

        while (!table.EndOfTable)
        {
          var row = table.GetNextRow();
          var entryId = (string) row[c_entryIdColumnName];
          using (var appointmentWrapper = GenericComObjectWrapper.Create ((TaskItem) _mapiNameSpace.GetItemFromID (entryId, storeId)))
          {
            entities.Add (EntityIdWithVersion.Create (appointmentWrapper.Inner.EntryID, appointmentWrapper.Inner.LastModificationTime));
          }
        }
      }
      return entities;
    }

    private static readonly CultureInfo _currentCultureInfo = CultureInfo.CurrentCulture;

    private string ToOutlookDateString (DateTime value)
    {
      return value.ToString ("g", _currentCultureInfo);
    }

    public async Task<IReadOnlyList<EntityWithVersion<string, TaskItemWrapper>>> Get (ICollection<string> ids)
    {
      var storeId = _taskFolder.StoreID;
      return ids
          .Select (id => EntityWithVersion.Create (
              id,
              new TaskItemWrapper (
                  (TaskItem) _mapiNameSpace.GetItemFromID (id, storeId),
                  entryId => (TaskItem) _mapiNameSpace.GetItemFromID (entryId, storeId))))
          .ToArray();
    }

    public void Cleanup (IReadOnlyDictionary<string, TaskItemWrapper> entities)
    {
      foreach (var wrapper in entities.Values)
        wrapper.Dispose();
    }

    public EntityIdWithVersion<string, DateTime> Update (string entityId, TaskItemWrapper entityToUpdate, Func<TaskItemWrapper, TaskItemWrapper> entityModifier)
    {
      entityToUpdate = entityModifier (entityToUpdate);
      entityToUpdate.Inner.Save();
      return new EntityIdWithVersion<string, DateTime> (entityToUpdate.Inner.EntryID, entityToUpdate.Inner.LastModificationTime);
    }

    public bool Delete (string entityId)
    {
      var entityWithId = Get (new[] { entityId }).Result.SingleOrDefault ();
      if (entityWithId == null)
        return false;

      using (var entity = entityWithId.Entity)
      {
        entity.Inner.Delete ();
        return true;
      }

    }

    public EntityIdWithVersion<string, DateTime> Create (Func<TaskItemWrapper, TaskItemWrapper> entityInitializer)
    {
      using (var wrapper = new TaskItemWrapper ((TaskItem) _taskFolder.Items.Add (OlItemType.olTaskItem), entryId => (TaskItem) _mapiNameSpace.GetItemFromID (entryId, _taskFolder.StoreID)))
      {
        using (var initializedWrapper = entityInitializer (wrapper))
        {
          initializedWrapper.Inner.Save();
          var result = new EntityIdWithVersion<string, DateTime> (initializedWrapper.Inner.EntryID, initializedWrapper.Inner.LastModificationTime);
          return result;
        }
      }
    }
  }
}