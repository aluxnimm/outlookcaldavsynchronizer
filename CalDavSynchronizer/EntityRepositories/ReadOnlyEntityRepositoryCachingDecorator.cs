using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.EntityVersionManagement;
using log4net;

namespace CalDavSynchronizer.EntityRepositories
{
  class ReadOnlyEntityRepositoryCachingDecorator<TEntity, TEntityId, TEntityVersion> : IReadOnlyEntityRepository<TEntity, TEntityId, TEntityVersion>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod ().DeclaringType);
    
    private readonly IReadOnlyEntityRepository<TEntity, TEntityId, TEntityVersion> _inner;
    private IDictionary<TEntityId, TEntity> _cachedItems;
    public bool LogWarningOnCacheMisses { get; set; }

    public ReadOnlyEntityRepositoryCachingDecorator (IReadOnlyEntityRepository<TEntity, TEntityId, TEntityVersion> inner)
    {
      _inner = inner;
    }

    public IEnumerable<EntityIdWithVersion<TEntityId, TEntityVersion>> GetEntityVersions (DateTime @from, DateTime to)
    {
      // since this call is fast ( approx. 250ms for 900 events) there is no need to cache it
      return _inner.GetEntityVersions (@from, @to);
    }

    public IDictionary<TEntityId, TEntity> GetEntities (ICollection<TEntityId> sourceEntityIds)
    {
      if (_cachedItems == null)
      {
        if (LogWarningOnCacheMisses)
          s_logger.WarnFormat ("Possible performance penalty. Loading {0} Entitie(s).", sourceEntityIds.Count);

        var values = _inner.GetEntities (sourceEntityIds);
        _cachedItems = new Dictionary<TEntityId, TEntity> (values);
        return values;
      }
      else
      {
        List<TEntityId> entitiesToLoad = new List<TEntityId>();
        var result = new Dictionary<TEntityId, TEntity> ();

        foreach (var id in sourceEntityIds)
        {
          TEntity cachedEntity;
          if (_cachedItems.TryGetValue (id, out cachedEntity))
            result.Add (id, cachedEntity);
          else
            entitiesToLoad.Add (id);
        }

        if (entitiesToLoad.Any())
        {
          if(LogWarningOnCacheMisses)
            s_logger.WarnFormat ("Possible performance penalty. Loading {0} Entitie(s).",entitiesToLoad.Count);

          foreach (var loaded in _inner.GetEntities (entitiesToLoad))
          {
            _cachedItems.Add (loaded.Key, loaded.Value);
            result.Add (loaded.Key, loaded.Value);
          }
        }

        return result;
      }
    }

    public EntityDelta<TEntity, TEntityId> LoadDelta (VersionDelta<TEntityId, TEntityVersion> delta)
    {
      // since this call should utilize GetEntities there is no need to cache it
      return _inner.LoadDelta (delta);
    }
  }
}
