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
using System.Threading.Tasks;
using GenSync.Logging;

namespace GenSync.EntityRepositories.Decorators
{
  class ReadOnlyEntityRepositoryRunInBackgroundDecorator<TEntityId, TEntityVersion, TEntity, TContext> : IReadOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext>
  {
    private readonly IReadOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> _decorated;

    public ReadOnlyEntityRepositoryRunInBackgroundDecorator(IReadOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> decorated)
    {
      _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
    }

    public async Task<IEnumerable<EntityVersion<TEntityId, TEntityVersion>>> GetVersions(IEnumerable<IdWithAwarenessLevel<TEntityId>> idsOfEntitiesToQuery, TContext context, IGetVersionsLogger logger)
    {
      return await Task.Run(() => _decorated.GetVersions(idsOfEntitiesToQuery, context, logger));
    }

    public async Task VerifyUnknownEntities(Dictionary<TEntityId, TEntityVersion> unknownEntites, TContext context)
    {
      await Task.Run(() => _decorated.VerifyUnknownEntities(unknownEntites, context));
    }

    public async Task<IEnumerable<EntityWithId<TEntityId, TEntity>>> Get(ICollection<TEntityId> ids, ILoadEntityLogger logger, TContext context)
    {
      return await Task.Run(() => _decorated.Get(ids, logger, context));
    }

    public void Cleanup(TEntity entity)
    {
      _decorated.Cleanup(entity);
    }

    public void Cleanup(IEnumerable<TEntity> entities)
    {
      _decorated.Cleanup(entities);
    }
  }
}