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
  class TypeFilteringVCardRepositoryDecorator : IEntityRepository<WebResourceName, string, vCard, int>
  {
    private readonly IEntityRepository<WebResourceName, string, vCard, int> _decorated;
    private readonly VCardType _typeToFilter;
    private readonly IVCardTypeDetector _typeDetector;

    public TypeFilteringVCardRepositoryDecorator (IEntityRepository<WebResourceName, string, vCard, int> decorated, VCardType typeToFilter, IVCardTypeDetector typeDetector)
    {
      if (decorated == null)
        throw new ArgumentNullException (nameof (decorated));
      if (typeDetector == null)
        throw new ArgumentNullException (nameof (typeDetector));

      _decorated = decorated;
      _typeToFilter = typeToFilter;
      _typeDetector = typeDetector;
    }

    public async Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetAllVersions (IEnumerable<WebResourceName> idsOfknownEntities, int context)
    {
      var versions = await _decorated.GetAllVersions (idsOfknownEntities, context).ConfigureAwait (false);
      return (await _typeDetector.GetVCardTypesAndCleanupCache (versions))
        .Where (t => t.Type == _typeToFilter)
        .Select (t => t.Id)
        .ToArray ();
    }

    public Task<bool> TryDelete (WebResourceName entityId, string version, int context)
    {
      return _decorated.TryDelete (entityId, version, context);
    }

    public Task<EntityVersion<WebResourceName, string>> TryUpdate (WebResourceName entityId, string version, vCard entityToUpdate, Func<vCard, Task<vCard>> entityModifier, int context)
    {
      return _decorated.TryUpdate (entityId, version, entityToUpdate, entityModifier, context);
    }

    public Task<EntityVersion<WebResourceName, string>> Create (Func<vCard, Task<vCard>> entityInitializer, int context)
    {
      return _decorated.Create (entityInitializer, context);
    }

    public Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetVersions (IEnumerable<IdWithAwarenessLevel<WebResourceName>> idsOfEntitiesToQuery, int context)
    {
      return _decorated.GetVersions (idsOfEntitiesToQuery, context);
    }

    public Task<IReadOnlyList<EntityWithId<WebResourceName, vCard>>> Get (ICollection<WebResourceName> ids, ILoadEntityLogger logger, int context)
    {
      return _decorated.Get (ids, logger, context);
    }

    public Task VerifyUnknownEntities (Dictionary<WebResourceName, string> unknownEntites, int context)
    {
      return _decorated.VerifyUnknownEntities (unknownEntites, context);
    }

    public void Cleanup (IReadOnlyDictionary<WebResourceName, vCard> entities)
    {
      _decorated.Cleanup (entities);
    }
  }
}