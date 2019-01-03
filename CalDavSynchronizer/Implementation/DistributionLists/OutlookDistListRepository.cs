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
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.DistributionLists
{
  public class OutlookDistListRepository<Tcontext> : IEntityRepository<string, DateTime, IDistListItemWrapper, Tcontext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodInfo.GetCurrentMethod ().DeclaringType);

    private readonly IOutlookSession _session;
    private readonly string _folderId;
    private readonly string _folderStoreId;
    private readonly IDaslFilterProvider _daslFilterProvider;
    private readonly IQueryOutlookDistListItemFolderStrategy _queryFolderStrategy;
    private readonly IComWrapperFactory _comWrapperFactory;

    public OutlookDistListRepository (IOutlookSession session, string folderId, string folderStoreId, IDaslFilterProvider daslFilterProvider, IQueryOutlookDistListItemFolderStrategy queryFolderStrategy, IComWrapperFactory comWrapperFactory)
    {
      if (session == null)
        throw new ArgumentNullException (nameof (session));
      if (daslFilterProvider == null)
        throw new ArgumentNullException (nameof (daslFilterProvider));
      if (queryFolderStrategy == null) throw new ArgumentNullException(nameof(queryFolderStrategy));
      if (comWrapperFactory == null) throw new ArgumentNullException(nameof(comWrapperFactory));

      _session = session;
      _folderId = folderId;
      _folderStoreId = folderStoreId;
      _daslFilterProvider = daslFilterProvider;
      _queryFolderStrategy = queryFolderStrategy;
      _comWrapperFactory = comWrapperFactory;
    }

    private const string c_entryIdColumnName = "EntryID";


    private GenericComObjectWrapper<Folder> CreateFolderWrapper ()
    {
      return GenericComObjectWrapper.Create ((Folder) _session.GetFolderFromId (_folderId, _folderStoreId));
    }

    public Task<IEnumerable<EntityVersion<string, DateTime>>> GetVersions (IEnumerable<IdWithAwarenessLevel<string>> idsOfEntitiesToQuery, Tcontext context, IGetVersionsLogger logger)
    {
      return Task.FromResult<IEnumerable<EntityVersion<string, DateTime>>> (
          idsOfEntitiesToQuery
              .Select (id => _session.GetDistListItemOrNull (id.Id, _folderId, _folderStoreId))
              .Where (e => e != null)
              .ToSafeEnumerable ()
              .Select (c => EntityVersion.Create (c.EntryID, c.LastModificationTime))
              .ToList ());
    }

    public Task<IEnumerable<EntityVersion<string, DateTime>>> GetAllVersions (IEnumerable<string> idsOfknownEntities, Tcontext context, IGetVersionsLogger logger)
    {
      using (var addressbookFolderWrapper = CreateFolderWrapper ())
      {
        bool isInstantSearchEnabled = false;

        try
        {
          using (var store = GenericComObjectWrapper.Create (addressbookFolderWrapper.Inner.Store))
          {
            if (store.Inner != null)
              isInstantSearchEnabled = store.Inner.IsInstantSearchEnabled;
          }
        }
        catch (COMException)
        {
          s_logger.Info ("Can't access IsInstantSearchEnabled property of store, defaulting to false.");
        }
        var filter = _daslFilterProvider.GetDistListFilter (isInstantSearchEnabled);

        return Task.FromResult<IEnumerable<EntityVersion<string, DateTime>>> (_queryFolderStrategy.QueryDistListFolder (_session, addressbookFolderWrapper.Inner, _folderId, filter, logger));
      }
    }

#pragma warning disable 1998
    public async Task<IEnumerable<EntityWithId<string, IDistListItemWrapper>>> Get (ICollection<string> ids, ILoadEntityLogger logger, Tcontext context)
#pragma warning restore 1998
    {
      return ids
        .Select(id => EntityWithId.Create(
          id,
          _comWrapperFactory.Create(
            _session.GetDistListItem(id, _folderStoreId))));
    }

    public Task VerifyUnknownEntities (Dictionary<string, DateTime> unknownEntites, Tcontext context)
    {
      return Task.FromResult (0);
    }
    
    public void Cleanup(IDistListItemWrapper entity)
    {
      entity.Dispose();
    }

    public void Cleanup(IEnumerable<IDistListItemWrapper> entities)
    {
      foreach (var contactItemWrapper in entities)
        contactItemWrapper.Dispose();
    }

    public async Task<EntityVersion<string, DateTime>> TryUpdate (
      string entityId,
      DateTime entityVersion,
      IDistListItemWrapper entityToUpdate,
      Func<IDistListItemWrapper, Task<IDistListItemWrapper>> entityModifier,
      Tcontext context,
      IEntitySynchronizationLogger logger)
    {
      entityToUpdate = await entityModifier (entityToUpdate);
      entityToUpdate.Inner.Save ();
      return new EntityVersion<string, DateTime> (entityToUpdate.Inner.EntryID, entityToUpdate.Inner.LastModificationTime);
    }

    public Task<bool> TryDelete (
      string entityId,
      DateTime version,
      Tcontext context,
      IEntitySynchronizationLogger logger)
    {
      var entityWithId = Get (new[] { entityId }, NullLoadEntityLogger.Instance, default (Tcontext)).Result.SingleOrDefault ();
      if (entityWithId == null)
        return Task.FromResult (true);

      using (var entity = entityWithId.Entity)
      {
        entity.Inner.Delete();
      }

      return Task.FromResult (true);
    }

    public async Task<EntityVersion<string, DateTime>> Create (Func<IDistListItemWrapper, Task<IDistListItemWrapper>> entityInitializer, Tcontext context)
    {
      IDistListItemWrapper newWrapper;

      using (var folderWrapper = CreateFolderWrapper ())
      {
        newWrapper = _comWrapperFactory.Create (
          (DistListItem) folderWrapper.Inner.Items.Add (OlItemType.olDistributionListItem));
      }

      using (newWrapper)
      {
        using (var initializedWrapper = await entityInitializer (newWrapper))
        {
          initializedWrapper.Inner.Save();
          var result = new EntityVersion<string, DateTime> (initializedWrapper.Inner.EntryID, initializedWrapper.Inner.LastModificationTime);
          return result;
        }
      }
    }
  }
}