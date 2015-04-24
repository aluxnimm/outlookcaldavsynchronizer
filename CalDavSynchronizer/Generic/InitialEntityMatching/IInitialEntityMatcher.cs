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
using CalDavSynchronizer.Generic.EntityRelationManagement;

namespace CalDavSynchronizer.Generic.InitialEntityMatching
{
  /// <summary>
  /// Takes the content of two repositories and finds those pairs of A and B entities, such that the A and B part of the pair relate to the same logical entity 
  /// </summary>
  public interface IInitialEntityMatcher<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
  {
    List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> PopulateEntityRelationStorage (
        IEntityRelationDataFactory<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> relationFactory,
        IReadOnlyDictionary<TAtypeEntityId, TAtypeEntity> allAtypeEntities,
        IReadOnlyDictionary<TBtypeEntityId, TBtypeEntity> allBtypeEntities,
        IReadOnlyDictionary<TAtypeEntityId, TAtypeEntityVersion> atypeEntityVersions,
        IReadOnlyDictionary<TBtypeEntityId, TBtypeEntityVersion> btypeEntityVersions
        );
  }
}
