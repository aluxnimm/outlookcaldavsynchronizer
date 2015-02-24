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
using System.Linq;
using System.Reflection;
using CalDavSynchronizer.EntityMapping;
using CalDavSynchronizer.EntityRelationManagement;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.EntityVersionManagement;
using log4net;

namespace CalDavSynchronizer.Synchronization
{
  public abstract class SynchronizerBase<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
      : ISynchronizer
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly ISynchronizerContext<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> _synchronizerContext;
    protected EntityRepositoryBase<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> _atypeEntityRepository;
    protected EntityRepositoryBase<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> _btypeEntityRepository;
    protected IVersionStorage<TAtypeEntityId, TAtypeEntityVersion> _atypeVersions;
    protected IVersionStorage<TBtypeEntityId, TBtypeEntityVersion> _btypeVersions;
    protected IEntityRelationStorage<TAtypeEntityId, TBtypeEntityId> _atypeToBtypeEntityRelationStorage;
    protected IEntityRelationStorage<TBtypeEntityId, TAtypeEntityId> _btypeToAtypeEntityRelationStorage;
    protected VersionDelta<TAtypeEntityId, TAtypeEntityVersion> _atypeDelta;
    protected VersionDelta<TBtypeEntityId, TBtypeEntityVersion> _btypeDelta;
    protected IEntityMapper<TAtypeEntity, TBtypeEntity> _entityMapper;

    protected SynchronizerBase (ISynchronizerContext<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> synchronizerContext)
    {
      _synchronizerContext = synchronizerContext;
    }

    public void Synchronize ()
    {
      s_logger.DebugFormat ("Entered. Atype='{0}', Btype='{1}'", typeof (TAtypeEntity).Name, typeof (TBtypeEntity).Name);

      _atypeEntityRepository = _synchronizerContext.AtypeRepository;
      _btypeEntityRepository = _synchronizerContext.BtypeRepository;
      _entityMapper = _synchronizerContext.EntityMapper;

      bool cachesWereCreatedNew;
      var caches = _synchronizerContext.LoadOrCreateCaches (out cachesWereCreatedNew);

      _atypeVersions = caches.AtypeStorage;
      _btypeVersions = caches.BtypeStorage;

      var atypeVersions = _atypeEntityRepository.GetEntityVersions (_synchronizerContext.From, _synchronizerContext.To);
      var btypeVersions = _btypeEntityRepository.GetEntityVersions (_synchronizerContext.From, _synchronizerContext.To);
      _atypeToBtypeEntityRelationStorage = caches.EntityRelationStorage;

      if (cachesWereCreatedNew)
      {
        var result = _synchronizerContext.InitialEntityMatcher.PopulateEntityRelationStorage (
            _atypeToBtypeEntityRelationStorage,
            _atypeEntityRepository,
            _btypeEntityRepository,
            atypeVersions,
            btypeVersions,
            _atypeVersions,
            _btypeVersions);

        _atypeDelta = new VersionDelta<TAtypeEntityId, TAtypeEntityVersion> (result.Item1.ToList(), new EntityIdWithVersion<TAtypeEntityId, TAtypeEntityVersion>[] { }, new EntityIdWithVersion<TAtypeEntityId, TAtypeEntityVersion>[] { });
        _btypeDelta = new VersionDelta<TBtypeEntityId, TBtypeEntityVersion> (result.Item2.ToList(), new EntityIdWithVersion<TBtypeEntityId, TBtypeEntityVersion>[] { }, new EntityIdWithVersion<TBtypeEntityId, TBtypeEntityVersion>[] { });
      }
      else
      {
        _atypeDelta = _atypeVersions.SetNewVersions (atypeVersions);
        _btypeDelta = _btypeVersions.SetNewVersions (btypeVersions);
      }

      _btypeToAtypeEntityRelationStorage = new EntityRelationStorageSwitchRolesWrapper<TBtypeEntityId, TAtypeEntityId> (_atypeToBtypeEntityRelationStorage);

      s_logger.DebugFormat ("Atype delta: {0}", _atypeDelta);
      s_logger.DebugFormat ("Btype delta: {0}", _btypeDelta);

      SynchronizeOverride();

      _synchronizerContext.SaveChaches (caches);

      s_logger.DebugFormat ("Exiting.");
    }

    protected abstract void SynchronizeOverride ();
  }
}