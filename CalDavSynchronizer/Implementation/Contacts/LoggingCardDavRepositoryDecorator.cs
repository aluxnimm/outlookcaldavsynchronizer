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
using CalDavSynchronizer.DataAccess;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.Contacts
{
  public class LoggingCardDavRepositoryDecorator : IEntityRepository<WebResourceName, string, vCard, ICardDavRepositoryLogger>
  {
    private readonly IEntityRepository<WebResourceName, string, vCard, int> _inner;

    public LoggingCardDavRepositoryDecorator(IEntityRepository<WebResourceName, string, vCard, int> inner)
    {
      _inner = inner;
    }

    public async Task<bool> TryDelete(WebResourceName entityId, string version, ICardDavRepositoryLogger context, IEntitySynchronizationLogger logger)
    {
      var result = await _inner.TryDelete(entityId, version, 0, logger);
      context.LogEntityDeleted(entityId);
      return result;
    }

    public async Task<EntityVersion<WebResourceName, string>> TryUpdate(WebResourceName entityId, string version, vCard entityToUpdate, Func<vCard, Task<vCard>> entityModifier, ICardDavRepositoryLogger context, IEntitySynchronizationLogger logger)
    {
      var result = await _inner.TryUpdate(entityId, version, entityToUpdate, entityModifier, 0, logger);
      context.LogEntityExists(entityId, entityToUpdate);
      return result;
    }

    public async Task<EntityVersion<WebResourceName, string>> Create(Func<vCard, Task<vCard>> entityInitializer, ICardDavRepositoryLogger context)
    {
      vCard vCard = null;
      var result = await _inner.Create(
        async e =>
        {
          vCard = await entityInitializer(e);
          return vCard;
        },
        0);
      context.LogEntityExists(result.Id, vCard);
      return result;
    }

    public async Task<IEnumerable<EntityVersion<WebResourceName, string>>> GetVersions(IEnumerable<IdWithAwarenessLevel<WebResourceName>> idsOfEntitiesToQuery, ICardDavRepositoryLogger context, IGetVersionsLogger logger)
    {
      var result = await _inner.GetVersions(idsOfEntitiesToQuery, 0, logger);
      context.LogEntitiesExists(result.Select(e => e.Id));
      return result;
    }

    public async Task<IEnumerable<EntityVersion<WebResourceName, string>>> GetAllVersions(IEnumerable<WebResourceName> idsOfknownEntities, ICardDavRepositoryLogger context, IGetVersionsLogger logger)
    {
      var result = await _inner.GetAllVersions(idsOfknownEntities, 0, logger);
      context.LogEntitiesExists(result.Select(e => e.Id));
      return result;
    }

    public async Task<IEnumerable<EntityWithId<WebResourceName, vCard>>> Get(ICollection<WebResourceName> ids, ILoadEntityLogger logger, ICardDavRepositoryLogger context)
    {
      var result = await _inner.Get(ids, logger, 0);
      foreach (var entity in result)
        context.LogEntityExists(entity.Id, entity.Entity);
      return result;
    }

    public Task VerifyUnknownEntities(Dictionary<WebResourceName, string> unknownEntites, ICardDavRepositoryLogger context)
    {
      return _inner.VerifyUnknownEntities(unknownEntites, 0);
    }

    public void Cleanup(vCard entity)
    {
      _inner.Cleanup(entity);
    }

    public void Cleanup(IEnumerable<vCard> entities)
    {
      _inner.Cleanup(entities);
    }
  }
}