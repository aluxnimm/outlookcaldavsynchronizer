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
using CalDavSynchronizer.DataAccess;
using System.Linq;
using System.Threading.Tasks;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.Contacts.VCardTypeSwitch
{
  public class VCardTypeDetector : IVCardTypeDetector
  {
    private readonly IReadOnlyEntityRepository<WebResourceName, string, vCard, int> _cardDavRepository;
    private readonly IVCardTypeCache _vcardTypeCache;


    public VCardTypeDetector (IReadOnlyEntityRepository<WebResourceName, string, vCard, int> cardDavRepository, IVCardTypeCache vcardTypeCache)
    {
      if (cardDavRepository == null)
        throw new ArgumentNullException (nameof (cardDavRepository));
      if (vcardTypeCache == null)
        throw new ArgumentNullException (nameof (vcardTypeCache));

      _cardDavRepository = cardDavRepository;
      _vcardTypeCache = vcardTypeCache;
    }

    public async Task<IEnumerable<IdWithType<TId>>> GetVCardTypesAndCleanupCache<TId> (IEnumerable<TId> allIdsInRepository) 
      where TId : IEntity<WebResourceName>
    {
      var cachedTypesById = _vcardTypeCache.GetEntries ();
      Dictionary<WebResourceName, VCardEntry> touchedTypesById = new Dictionary<WebResourceName, VCardEntry>();

      var result = new List<IdWithType<TId>> ();

      var unknownIds = Resolve (allIdsInRepository, result, cachedTypesById, touchedTypesById);

      if (!unknownIds.Any())
      {
        _vcardTypeCache.SetEntries(touchedTypesById);
        return result;
      }

      foreach (var entity in await _cardDavRepository.Get (unknownIds.Select(i => i.Id).ToArray(), NullLoadEntityLogger.Instance, 0))
      {
        var newEntry = new VCardEntry { Id = entity.Id, Type = entity.Entity.Kind == vCardKindType.Group ? VCardType.Group : VCardType.Contact };
        cachedTypesById.Add (entity.Id, newEntry);
      }
      var stillUnknownIds = Resolve (unknownIds, result, cachedTypesById, touchedTypesById);

      _vcardTypeCache.SetEntries (touchedTypesById);
        
      if (stillUnknownIds.Any ())
        throw new Exception ("VCard does not exist on server anymore."); // TODO: throw specialized type that causes just a warning

      return result;
    }


    static List<TId> Resolve<TId> (IEnumerable<TId> ids, List<IdWithType<TId>> resultCollector, Dictionary<WebResourceName, VCardEntry> typesById, Dictionary<WebResourceName, VCardEntry> usedTypesById) 
      where TId : IEntity<WebResourceName>
    {
      var unknownIds = new List<TId> ();

      foreach (var id in ids)
      {
        VCardEntry entry;
        if (typesById.TryGetValue(id.Id, out entry))
        {
          resultCollector.Add(new IdWithType<TId>(id, entry.Type));
          usedTypesById.Add(id.Id, entry);
        }
        else
        {
          unknownIds.Add(id);
        }
      }

      return unknownIds;
    }


  }
}
