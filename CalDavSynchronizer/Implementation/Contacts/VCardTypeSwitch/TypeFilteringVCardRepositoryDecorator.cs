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

namespace CalDavSynchronizer.Implementation.Contacts.VCardTypeSwitch
{
  class TypeFilteringVCardRepositoryDecorator<TContext> : IEntityRepository<WebResourceName, string, vCard, TContext>
  {
    private readonly IEntityRepository<WebResourceName, string, vCard, TContext> _decorated;
    private readonly VCardType _typeToFilter;
    private readonly IVCardTypeDetector _typeDetector;

    public TypeFilteringVCardRepositoryDecorator (IEntityRepository<WebResourceName, string, vCard, TContext> decorated, VCardType typeToFilter, IVCardTypeDetector typeDetector)
    {
      if (decorated == null)
        throw new ArgumentNullException (nameof (decorated));
      if (typeDetector == null)
        throw new ArgumentNullException (nameof (typeDetector));

      _decorated = decorated;
      _typeToFilter = typeToFilter;
      _typeDetector = typeDetector;
    }

    public async Task<IEnumerable<EntityVersion<WebResourceName, string>>> GetAllVersions (IEnumerable<WebResourceName> idsOfknownEntities, TContext context, IGetVersionsLogger logger)
    {
      var versions = await _decorated.GetAllVersions (idsOfknownEntities, context, logger).ConfigureAwait (false);
      return (await _typeDetector.GetVCardTypesAndCleanupCache (versions))
        .Where (t => t.Type == _typeToFilter)
        .Select (t => t.Id)
        .ToArray ();
    }

    public Task<bool> TryDelete (WebResourceName entityId, string version, TContext context, IEntitySynchronizationLogger logger)
    {
      return _decorated.TryDelete (entityId, version, context, logger);
    }

    public Task<EntityVersion<WebResourceName, string>> TryUpdate (WebResourceName entityId, string version, vCard entityToUpdate, Func<vCard, Task<vCard>> entityModifier, TContext context, IEntitySynchronizationLogger logger)
    {
      return _decorated.TryUpdate (entityId, version, entityToUpdate, entityModifier, context, logger);
    }

    public async Task<EntityVersion<WebResourceName, string>> Create (Func<vCard, Task<vCard>> entityInitializer, TContext context)
    {
      // TODO: to prevent the entity from being loaded in the next sync run, since it is unknown to the cache, it should be added here to the cache of the _typeDetector
      return await _decorated.Create (
        async vCard =>
        {
          var initialized = await entityInitializer(vCard);
          if (_typeToFilter == VCardType.Group)
            initialized.Kind = vCardKindType.Group;
          return initialized;
        },
        context);
    }

    public Task<IEnumerable<EntityVersion<WebResourceName, string>>> GetVersions (IEnumerable<IdWithAwarenessLevel<WebResourceName>> idsOfEntitiesToQuery, TContext context, IGetVersionsLogger logger)
    {
      return _decorated.GetVersions (idsOfEntitiesToQuery, context, logger);
    }

    public async Task<IEnumerable<EntityWithId<WebResourceName, vCard>>> Get (ICollection<WebResourceName> ids, ILoadEntityLogger logger, TContext context)
    {
      return (await _decorated.Get (ids, logger, context)).Where(c => _typeDetector.GetVCardType(c.Entity) == _typeToFilter);
    }

    public Task VerifyUnknownEntities (Dictionary<WebResourceName, string> unknownEntites, TContext context)
    {
      return _decorated.VerifyUnknownEntities (unknownEntites, context);
    }

    public void Cleanup(vCard entity)
    {
      _decorated.Cleanup(entity);
    }

    public void Cleanup(IEnumerable<vCard> entities)
    {
      _decorated.Cleanup(entities);
    }
  }
}