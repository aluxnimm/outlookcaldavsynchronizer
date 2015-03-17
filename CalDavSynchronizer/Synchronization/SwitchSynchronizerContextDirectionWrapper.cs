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
using CalDavSynchronizer.EntityMapping;
using CalDavSynchronizer.EntityRelationManagement;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.InitialEntityMatching;

namespace CalDavSynchronizer.Synchronization
{
  internal class SwitchSynchronizerContextDirectionWrapper<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
      : ISynchronizerContext<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
  {
    private ISynchronizerContext<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> _inner;


    public SwitchSynchronizerContextDirectionWrapper (ISynchronizerContext<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> inner)
    {
      _inner = inner;
    }


    public IEntityMapper<TAtypeEntity, TBtypeEntity> EntityMapper
    {
      get { return new EntityMapperSwitchDirectionWrapper<TAtypeEntity, TBtypeEntity> (_inner.EntityMapper); }
    }

    public EntityRepositoryBase<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> AtypeRepository
    {
      get { return _inner.BtypeRepository; }
    }

    public EntityRepositoryBase<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> BtypeRepository
    {
      get { return _inner.AtypeRepository; }
    }


    public DateTime From
    {
      get { return _inner.From; }
    }

    public DateTime To
    {
      get { return _inner.To; }
    }

    public InitialEntityMatcher<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> InitialEntityMatcher
    {
      get { return new SwitchDirectionInitialEntityMapper<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> (_inner.InitialEntityMatcher); }
    }

    public void SaveChaches (EntityCaches<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> caches)
    {
      var switchedCaches = new EntityCaches<TBtypeEntityId, TBtypeEntityVersion, TAtypeEntityId, TAtypeEntityVersion> (
          caches.BtypeStorage,
          caches.AtypeStorage,
          ((EntityRelationStorageSwitchRolesWrapper<TAtypeEntityId, TBtypeEntityId>) caches.EntityRelationStorage).Inner
          );
      _inner.SaveChaches (switchedCaches);
    }

    public void DeleteCaches ()
    {
      _inner.DeleteCaches();
    }

    public EntityCaches<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> LoadOrCreateCaches (out bool cachesWereCreatedNew)
    {
      var caches = _inner.LoadOrCreateCaches (out cachesWereCreatedNew);
      return new EntityCaches<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> (
          caches.BtypeStorage,
          caches.AtypeStorage,
          new EntityRelationStorageSwitchRolesWrapper<TAtypeEntityId, TBtypeEntityId> (caches.EntityRelationStorage)
          );
    }
  }
}