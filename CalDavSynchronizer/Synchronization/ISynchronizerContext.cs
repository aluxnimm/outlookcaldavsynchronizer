// This file is Part of CalDavSynchronizer (https://sourceforge.net/projects/outlookcaldavsynchronizer/)
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
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.EntityMapping;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.InitialEntityMatching;

namespace CalDavSynchronizer.Synchronization
{
  public interface ISynchronizerContext<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
    : IStorageDataAccess<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
  {
    IEntityMapper<TAtypeEntity, TBtypeEntity> EntityMapper { get; }
    EntityRepositoryBase<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> AtypeRepository { get; }
    EntityRepositoryBase<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> BtypeRepository { get; }
    DateTime From { get; }
    DateTime To { get; }

    InitialEntityMatcher<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> InitialEntityMatcher { get; }
  }
}