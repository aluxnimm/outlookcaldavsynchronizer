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
using System.Linq;
using System.Reflection;
using CalDavSynchronizer.EntityRelationManagement;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.EntityVersionManagement;
using log4net;

namespace CalDavSynchronizer.InitialEntityMatching
{
  public abstract class InitialEntityMatcherByPropertyGrouping<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TAtypeProperty, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeProperty> 
    : InitialEntityMatcher<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod ().DeclaringType);

    public Tuple<IEnumerable<EntityIdWithVersion<TAtypeEntityId, TAtypeEntityVersion>>,IEnumerable<EntityIdWithVersion<TBtypeEntityId, TBtypeEntityVersion>>> PopulateEntityRelationStorage (
      IEntityRelationStorage<TAtypeEntityId,TBtypeEntityId> relationStorageToPopulate,
      EntityRepositoryBase<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> atypeEntityRepository,
      EntityRepositoryBase<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> btypeEntityRepository,
      IEnumerable<EntityIdWithVersion<TAtypeEntityId, TAtypeEntityVersion>> atypeEntityVersions, 
      IEnumerable<EntityIdWithVersion<TBtypeEntityId, TBtypeEntityVersion>> btypeEntityVersions,
      IVersionStorage<TAtypeEntityId, TAtypeEntityVersion> atypeVersionStorage,
      IVersionStorage<TBtypeEntityId, TBtypeEntityVersion> btypeVersionStorage)
    {
      atypeVersionStorage.SetNewVersions (atypeEntityVersions);
      btypeVersionStorage.SetNewVersions (btypeEntityVersions);

      Dictionary<TAtypeEntityId, EntityIdWithVersion<TAtypeEntityId, TAtypeEntityVersion>> notMatchingAtypeEntityVersionsById = atypeEntityVersions.ToDictionary (v => v.Id);
      Dictionary<TBtypeEntityId, EntityIdWithVersion<TBtypeEntityId, TBtypeEntityVersion>> notMatchingbtypeEntityVersionsById = btypeEntityVersions.ToDictionary (v => v.Id);

      var atypeEntities = atypeEntityRepository.GetEntities (notMatchingAtypeEntityVersionsById.Keys);
      var btypeEntities = btypeEntityRepository.GetEntities (notMatchingbtypeEntityVersionsById.Keys);

      var atypeEntityIdsGroupedByProperty = atypeEntities.GroupBy (a => GetAtypePropertyValue(a.Value)).ToDictionary (g => g.Key, g => g.Select (o => o.Key).ToList ());
      var btypeEntityIdsGroupedByProperty = btypeEntities.GroupBy (b => GetBtypePropertyValue(b.Value)).ToDictionary (g => g.Key, g => g.Select (o => o.Key).ToList ());

      foreach (var atypeEntityGroup in atypeEntityIdsGroupedByProperty)
      {
        foreach (var atypeEntityId in atypeEntityGroup.Value)
        {
          List<TBtypeEntityId> btypeEntityGroup;
          if (btypeEntityIdsGroupedByProperty.TryGetValue (MapAtypePropertyValue (atypeEntityGroup.Key), out btypeEntityGroup))
          {
            foreach (var btypeEntityId in btypeEntityGroup.ToArray())
            {
              if (AreEqual (atypeEntities[atypeEntityId], btypeEntities[btypeEntityId]))
              {
                s_logger.DebugFormat ("Found matching entities: '{0}' == '{1}'", atypeEntityId, btypeEntityId);

                relationStorageToPopulate.AddRelation (atypeEntityId, btypeEntityId);
                notMatchingAtypeEntityVersionsById.Remove (atypeEntityId);
                notMatchingbtypeEntityVersionsById.Remove (btypeEntityId);

                break; 
              }
            }
          }
        }
      }


      return Tuple.Create<IEnumerable<EntityIdWithVersion<TAtypeEntityId, TAtypeEntityVersion>>, IEnumerable<EntityIdWithVersion<TBtypeEntityId, TBtypeEntityVersion>>> (notMatchingAtypeEntityVersionsById.Values, notMatchingbtypeEntityVersionsById.Values);
    }

    protected abstract bool AreEqual (TAtypeEntity atypeEntity, TBtypeEntity btypeEntity);

    protected abstract TAtypeProperty GetAtypePropertyValue (TAtypeEntity atypeEntity);
    protected abstract TBtypeProperty GetBtypePropertyValue (TBtypeEntity btypeEntity);
    protected abstract TBtypeProperty MapAtypePropertyValue (TAtypeProperty value);

  }
}
