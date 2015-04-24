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
using CalDavSynchronizer.Generic.EntityMapping;
using CalDavSynchronizer.Generic.EntityRelationManagement;
using CalDavSynchronizer.Generic.EntityRepositories;
using CalDavSynchronizer.Generic.Synchronization.StateFactories;

namespace CalDavSynchronizer.Generic.Synchronization
{
  /// <summary>
  /// Environment for a SyncState
  /// NOTE: this is not the Context from the State-Pattern !!!
  /// </summary>
  public class EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
  {
    public IEntityMapper<TAtypeEntity, TBtypeEntity> Mapper { get; private set; }
    public IWriteOnlyEntityRepository<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> ARepository { get; private set; }
    public IWriteOnlyEntityRepository<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> BRepository { get; private set; }
    public IEntityRelationDataFactory<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> DataFactory { get; private set; }
    public IEntitySyncStateFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> StateFactory { get; private set; }

    public EntitySyncStateEnvironment (IEntityMapper<TAtypeEntity, TBtypeEntity> mapper, IWriteOnlyEntityRepository<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> aRepository, IWriteOnlyEntityRepository<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> bRepository, IEntityRelationDataFactory<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> dataFactory, IEntitySyncStateFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> stateFactory)
    {
      Mapper = mapper;
      ARepository = aRepository;
      BRepository = bRepository;
      DataFactory = dataFactory;
      StateFactory = stateFactory;
    }

  }
}