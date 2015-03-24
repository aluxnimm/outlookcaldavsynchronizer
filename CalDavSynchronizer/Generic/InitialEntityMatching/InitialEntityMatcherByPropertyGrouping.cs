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
using CalDavSynchronizer.Generic.EntityRelationManagement;
using log4net;

namespace CalDavSynchronizer.Generic.InitialEntityMatching
{
  public abstract class InitialEntityMatcherByPropertyGrouping<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TAtypeProperty, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeProperty>
      : IInitialEntityMatcher<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    public List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> PopulateEntityRelationStorage (
        IEntityRelationDataFactory<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> relationFactory,
        IReadOnlyDictionary<TAtypeEntityId, TAtypeEntity> allAtypeEntities,
        IReadOnlyDictionary<TBtypeEntityId, TBtypeEntity> allBtypeEntities,
        IReadOnlyDictionary<TAtypeEntityId, TAtypeEntityVersion> atypeEntityVersions,
        IReadOnlyDictionary<TBtypeEntityId, TBtypeEntityVersion> btypeEntityVersions
        )
    {
      var atypeEntityIdsGroupedByProperty = allAtypeEntities.GroupBy (a => GetAtypePropertyValue (a.Value)).ToDictionary (g => g.Key, g => g.Select (o => o.Key).ToList());
      var btypeEntityIdsGroupedByProperty = allBtypeEntities.GroupBy (b => GetBtypePropertyValue (b.Value)).ToDictionary (g => g.Key, g => g.ToDictionary (o => o.Key, o => true));

      List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> foundRelations = new List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>();

      foreach (var atypeEntityGroup in atypeEntityIdsGroupedByProperty)
      {
        foreach (var atypeEntityId in atypeEntityGroup.Value)
        {
          Dictionary<TBtypeEntityId,bool> btypeEntityGroup;

          var btypeEntityGroupKey = MapAtypePropertyValue (atypeEntityGroup.Key);
          if (btypeEntityIdsGroupedByProperty.TryGetValue (btypeEntityGroupKey, out btypeEntityGroup))
          {
            foreach (var btypeEntityId in btypeEntityGroup.Keys)
            {
              if (AreEqual (allAtypeEntities[atypeEntityId], allBtypeEntities[btypeEntityId]))
              {
                s_logger.DebugFormat ("Found matching entities: '{0}' == '{1}'", atypeEntityId, btypeEntityId);

                foundRelations.Add (relationFactory.Create (atypeEntityId, atypeEntityVersions[atypeEntityId], btypeEntityId, btypeEntityVersions[btypeEntityId]));
                // b has to be removed from the Group, because b is iterated in the inner loop an if an second match would occour this will lead to an exception
                // not required to remove a from the group, because in the aouter loop every group is just iterated once
                btypeEntityGroup.Remove (btypeEntityId);
                break;
              }
            }

            // throwing away the empty dictionaries is not neccessary. Just to make dubugging easier, because there is less crap the dictionary. 
            if (btypeEntityGroup.Count == 0)
              btypeEntityIdsGroupedByProperty.Remove (btypeEntityGroupKey);
          }
        }
      }


      return foundRelations;
    }

    protected abstract bool AreEqual (TAtypeEntity atypeEntity, TBtypeEntity btypeEntity);

    protected abstract TAtypeProperty GetAtypePropertyValue (TAtypeEntity atypeEntity);
    protected abstract TBtypeProperty GetBtypePropertyValue (TBtypeEntity btypeEntity);
    protected abstract TBtypeProperty MapAtypePropertyValue (TAtypeProperty value);
  }
}