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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Diagnostics;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using log4net;
using Thought.vCards;
using CalDavSynchronizer.ThoughtvCardWorkaround;
using CalDavSynchronizer.Utilities;
using GenSync.Utilities;

namespace CalDavSynchronizer.Implementation.Contacts
{
  public abstract class CardDavEntityRepository<TEntity,TDeserializationThreadLocal, TContext> : IEntityRepository<WebResourceName, string, TEntity, TContext>, IStateAwareEntityRepository<WebResourceName, string, TContext, string>
    where TEntity : new()
    where TDeserializationThreadLocal : new()
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly ICardDavDataAccess _cardDavDataAccess;
    private readonly IEqualityComparer<string> _versionComparer;

    public CardDavEntityRepository (ICardDavDataAccess cardDavDataAccess, IEqualityComparer<string> versionComparer)
    {
      _cardDavDataAccess = cardDavDataAccess;
      _versionComparer = versionComparer ?? throw new ArgumentNullException(nameof(versionComparer));
    }

    public async Task<IEnumerable<EntityVersion<WebResourceName, string>>> GetVersions (IEnumerable<IdWithAwarenessLevel<WebResourceName>> idsOfEntitiesToQuery, TContext context, IGetVersionsLogger logger)
    {
      return await _cardDavDataAccess.GetVersions (idsOfEntitiesToQuery.Select (i => i.Id));
    }

    public async Task<IEnumerable<EntityVersion<WebResourceName, string>>> GetAllVersions (IEnumerable<WebResourceName> idsOfknownEntities, TContext context, IGetVersionsLogger logger)
    {
      using (AutomaticStopwatch.StartInfo (s_logger, "CardDavRepository.GetVersions"))
      {
        return await _cardDavDataAccess.GetAllVersions();
      }
    }

    public async Task<IEnumerable<EntityWithId<WebResourceName, TEntity>>> Get (ICollection<WebResourceName> ids, ILoadEntityLogger logger, TContext context)
    {
      if (ids.Count == 0)
        return new EntityWithId<WebResourceName, TEntity>[] { };

      using (AutomaticStopwatch.StartInfo(s_logger, string.Format("CardDavRepository.Get ({0} entitie(s))", ids.Count)))
      {
        var entities = await _cardDavDataAccess.GetEntities(ids);
        return ParallelDeserialize(entities, logger);
      }
    }
    
    public Task VerifyUnknownEntities (Dictionary<WebResourceName, string> unknownEntites, TContext context)
    {
      return Task.FromResult (0);
    }

    public void Cleanup(TEntity entity)
    {
      // nothing to do
    }

    public void Cleanup(IEnumerable<TEntity> entities)
    {
      // nothing to do
    }
    
    private IReadOnlyList<EntityWithId<WebResourceName, TEntity>> ParallelDeserialize (IReadOnlyList<EntityWithId<WebResourceName, string>> serializedEntities, ILoadEntityLogger logger)
    {
      var result = new List<EntityWithId<WebResourceName, TEntity>>();

      Parallel.ForEach (
          serializedEntities,
          () => Tuple.Create (new TDeserializationThreadLocal (), new List<Tuple<WebResourceName, TEntity>>()),
          (serialized, loopState, threadLocal) =>
          {
            TEntity entity;

            if (TryDeserialize (serialized.Entity, out entity, serialized.Id, threadLocal.Item1, logger))
              threadLocal.Item2.Add (Tuple.Create (serialized.Id, entity));
            return threadLocal;
          },
          threadLocal =>
          {
            lock (result)
            {
              foreach (var card in threadLocal.Item2)
                result.Add (EntityWithId.Create (card.Item1, card.Item2));
            }
          });

      return result;
    }
    
    public async Task<bool> TryDelete (WebResourceName entityId, string version, TContext context, IEntitySynchronizationLogger logger)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        return await _cardDavDataAccess.TryDeleteEntity (entityId, version);
      }
    }

    public async Task<EntityVersion<WebResourceName, string>> TryUpdate (
        WebResourceName entityId,
        string entityVersion,
        TEntity entityToUpdate,
        Func<TEntity, Task<TEntity>> entityModifier,
        TContext context, 
        IEntitySynchronizationLogger logger)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        var updatedEntity = await entityModifier (entityToUpdate);
        if (string.IsNullOrEmpty (GetUid (updatedEntity)))
          SetUid (updatedEntity, Guid.NewGuid().ToString());
        return await _cardDavDataAccess.TryUpdateEntity (entityId, entityVersion, Serialize (updatedEntity));
      }
    }

    public async Task<EntityVersion<WebResourceName, string>> Create(Func<TEntity, Task<TEntity>> entityInitializer, TContext context)
    {
      using (AutomaticStopwatch.StartDebug(s_logger))
      {
        TEntity newEntity = new TEntity();
        var uid = Guid.NewGuid().ToString();
        SetUid(newEntity, uid);
        var initializedVcard = await entityInitializer(newEntity);
        return await _cardDavDataAccess.CreateEntity(Serialize(initializedVcard), uid);
      }
    }
    
    public async Task<(IEntityStateCollection<WebResourceName, string> States, string NewToken)> GetFullRepositoryState(IEnumerable<WebResourceName> idsOfknownEntities, string stateToken, TContext context, IGetVersionsLogger logger)
    {
      var collectionSyncResult = await _cardDavDataAccess.CollectionSync(stateToken, logger);
      return (new WebDavCollectionSyncEntityStates(collectionSyncResult.ChangedOrAddedItems, collectionSyncResult.DeletedItems, _versionComparer), collectionSyncResult.SyncToken);
    }

    protected abstract void SetUid(TEntity entity, string uid);
    protected abstract string GetUid (TEntity entity);

    protected abstract string Serialize(TEntity vcard);

    protected abstract bool TryDeserialize(
      string vcardData,
      out TEntity vcard,
      WebResourceName uriOfAddressbookForLogging,
      TDeserializationThreadLocal deserializationThreadLocal,
      ILoadEntityLogger logger);

  }
}