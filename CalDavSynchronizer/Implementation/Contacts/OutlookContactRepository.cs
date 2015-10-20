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
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using GenSync;
using GenSync.EntityRepositories;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.Contacts
{
  public class OutlookContactRepository : IEntityRepository<GenericComObjectWrapper<ContactItem>, string, DateTime>
  {
    private readonly NameSpace _mapiNameSpace;
    private readonly string _folderId;
    private readonly string _folderStoreId;

    public OutlookContactRepository (NameSpace mapiNameSpace, string folderId, string folderStoreId)
    {
      if (mapiNameSpace == null)
        throw new ArgumentNullException ("mapiNameSpace");

      _mapiNameSpace = mapiNameSpace;
      _folderId = folderId;
      _folderStoreId = folderStoreId;
    }

    private const string c_entryIdColumnName = "EntryID";


    private GenericComObjectWrapper<Folder> CreateFolderWrapper ()
    {
      return GenericComObjectWrapper.Create ((Folder) _mapiNameSpace.GetFolderFromID (_folderId, _folderStoreId));
    }

    public Task<IReadOnlyList<EntityIdWithVersion<string, DateTime>>> GetVersions (ICollection<string> ids)
    {
      return Task.FromResult<IReadOnlyList<EntityIdWithVersion<string, DateTime>>> (
          ids
              .Select (id => (ContactItem) _mapiNameSpace.GetItemFromID (id, _folderStoreId))
              .ToSafeEnumerable()
              .Select (c => EntityIdWithVersion.Create (c.EntryID, c.LastModificationTime))
              .ToList());
    }

    public Task<IReadOnlyList<EntityIdWithVersion<string, DateTime>>> GetVersions ()
    {
      var events = new List<EntityIdWithVersion<string, DateTime>>();


      using (var calendarFolderWrapper = CreateFolderWrapper())
      {
        using (var tableWrapper = GenericComObjectWrapper.Create ((Table) calendarFolderWrapper.Inner.GetTable()))
        {
          var table = tableWrapper.Inner;
          table.Columns.RemoveAll();
          table.Columns.Add (c_entryIdColumnName);

          var storeId = calendarFolderWrapper.Inner.StoreID;

          while (!table.EndOfTable)
          {
            var row = table.GetNextRow();
            var entryId = (string) row[c_entryIdColumnName];
            using (var appointmentWrapper = GenericComObjectWrapper.Create ((ContactItem) _mapiNameSpace.GetItemFromID (entryId, storeId)))
            {
              events.Add (new EntityIdWithVersion<string, DateTime> (appointmentWrapper.Inner.EntryID, appointmentWrapper.Inner.LastModificationTime));
            }
          }
        }
      }

      return Task.FromResult<IReadOnlyList<EntityIdWithVersion<string, DateTime>>> (events);
    }

    private static readonly CultureInfo _currentCultureInfo = CultureInfo.CurrentCulture;

    private string ToOutlookDateString (DateTime value)
    {
      return value.ToString ("g", _currentCultureInfo);
    }

#pragma warning disable 1998
    public async Task<IReadOnlyList<EntityWithVersion<string, GenericComObjectWrapper<ContactItem>>>> Get (ICollection<string> ids)
#pragma warning restore 1998
    {
      return ids
          .Select (id => EntityWithVersion.Create (
              id,
              GenericComObjectWrapper.Create (
                  (ContactItem) _mapiNameSpace.GetItemFromID (id, _folderStoreId))))
          .ToArray();
    }

    public void Cleanup (IReadOnlyDictionary<string, GenericComObjectWrapper<ContactItem>> entities)
    {
      foreach (var appointmentItemWrapper in entities.Values)
        appointmentItemWrapper.Dispose();
    }

    public Task<EntityIdWithVersion<string, DateTime>> Update (string entityId, GenericComObjectWrapper<ContactItem> entityToUpdate, Func<GenericComObjectWrapper<ContactItem>, GenericComObjectWrapper<ContactItem>> entityModifier)
    {
      entityToUpdate = entityModifier (entityToUpdate);
      entityToUpdate.Inner.Save();
      return Task.FromResult (new EntityIdWithVersion<string, DateTime> (entityToUpdate.Inner.EntryID, entityToUpdate.Inner.LastModificationTime));
    }

    public Task Delete (string entityId)
    {
      var entityWithId = Get (new[] { entityId }).Result.SingleOrDefault();
      if (entityWithId == null)
        return Task.FromResult (0);

      using (var appointment = entityWithId.Entity)
      {
        appointment.Inner.Delete();
      }

      return Task.FromResult (0);
    }

    public Task<EntityIdWithVersion<string, DateTime>> Create (Func<GenericComObjectWrapper<ContactItem>, GenericComObjectWrapper<ContactItem>> entityInitializer)
    {
      GenericComObjectWrapper<ContactItem> newWrapper;

      using (var folderWrapper = CreateFolderWrapper())
      {
        newWrapper = GenericComObjectWrapper.Create ((ContactItem) folderWrapper.Inner.Items.Add (OlItemType.olContactItem));
      }

      using (newWrapper)
      {
        using (var initializedWrapper = entityInitializer (newWrapper))
        {
          initializedWrapper.Inner.Save();
          var result = new EntityIdWithVersion<string, DateTime> (initializedWrapper.Inner.EntryID, initializedWrapper.Inner.LastModificationTime);
          return Task.FromResult (result);
        }
      }
    }
  }
}