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
using GenSync.EntityMapping;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.Synchronization.StateFactories;

namespace GenSync.Synchronization
{
  /// <summary>
  /// Environment for a SyncState
  /// NOTE: this is not the Context from the State-Pattern !!!
  /// </summary>
  public class EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
  {
    public IEntityMapper<TAtypeEntity, TBtypeEntity> Mapper { get; private set; }
    public IEntityRelationDataFactory<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> DataFactory { get; private set; }
    public IEntitySyncStateFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> StateFactory { get; private set; }
    public IExceptionLogger ExceptionLogger { get; private set; }

    public EntitySyncStateEnvironment (IEntityMapper<TAtypeEntity, TBtypeEntity> mapper, IEntityRelationDataFactory<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> dataFactory, IEntitySyncStateFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> stateFactory, IExceptionLogger exceptionLogger)
    {
      ExceptionLogger = exceptionLogger;
      Mapper = mapper;
      DataFactory = dataFactory;
      StateFactory = stateFactory;
    }
  }
}