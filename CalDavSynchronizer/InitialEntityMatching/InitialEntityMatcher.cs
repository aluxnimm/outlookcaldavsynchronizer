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
using System.Collections.Generic;
using CalDavSynchronizer.EntityRelationManagement;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.EntityVersionManagement;

namespace CalDavSynchronizer.InitialEntityMatching
{
  public interface InitialEntityMatcher<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
  {
    Tuple<IEnumerable<EntityIdWithVersion<TAtypeEntityId, TAtypeEntityVersion>>, IEnumerable<EntityIdWithVersion<TBtypeEntityId, TBtypeEntityVersion>>> PopulateEntityRelationStorage (
      IEntityRelationStorage<TAtypeEntityId, TBtypeEntityId> relationStorageToPopulate,
      EntityRepositoryBase<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> atypeEntityRepository,
      EntityRepositoryBase<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> btypeEntityRepository,
      IEnumerable<EntityIdWithVersion<TAtypeEntityId, TAtypeEntityVersion>> atypeEntityVersions,
      IEnumerable<EntityIdWithVersion<TBtypeEntityId, TBtypeEntityVersion>> btypeEntityVersions,
      IVersionStorage<TAtypeEntityId,TAtypeEntityVersion> atypeVersionStorage,
      IVersionStorage<TBtypeEntityId,TBtypeEntityVersion> btypeVersionStorage
      );
  }
}
