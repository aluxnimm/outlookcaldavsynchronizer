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
        IDictionary<TAtypeEntityId, TAtypeEntity> allAtypeEntities,
        IDictionary<TBtypeEntityId, TBtypeEntity> allBtypeEntities,
        Dictionary<TAtypeEntityId, TAtypeEntityVersion> atypeEntityVersionsToWork,
        Dictionary<TBtypeEntityId, TBtypeEntityVersion> btypeEntityVersionsToWork
        )
    {
      var atypeEntityIdsGroupedByProperty = allAtypeEntities.GroupBy (a => GetAtypePropertyValue (a.Value)).ToDictionary (g => g.Key, g => g.Select (o => o.Key).ToList());
      var btypeEntityIdsGroupedByProperty = allBtypeEntities.GroupBy (b => GetBtypePropertyValue (b.Value)).ToDictionary (g => g.Key, g => g.Select (o => o.Key).ToList());

      List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> foundRelations = new List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>();

      foreach (var atypeEntityGroup in atypeEntityIdsGroupedByProperty)
      {
        foreach (var atypeEntityId in atypeEntityGroup.Value)
        {
          List<TBtypeEntityId> btypeEntityGroup;
          if (btypeEntityIdsGroupedByProperty.TryGetValue (MapAtypePropertyValue (atypeEntityGroup.Key), out btypeEntityGroup))
          {
            foreach (var btypeEntityId in btypeEntityGroup.ToArray())
            {
              if (AreEqual (allAtypeEntities[atypeEntityId], allBtypeEntities[btypeEntityId]))
              {
                s_logger.DebugFormat ("Found matching entities: '{0}' == '{1}'", atypeEntityId, btypeEntityId);

                foundRelations.Add (relationFactory.Create (atypeEntityId, atypeEntityVersionsToWork[atypeEntityId], btypeEntityId, btypeEntityVersionsToWork[btypeEntityId]));
                atypeEntityVersionsToWork.Remove (atypeEntityId);
                btypeEntityVersionsToWork.Remove (btypeEntityId);

                break;
              }
            }
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