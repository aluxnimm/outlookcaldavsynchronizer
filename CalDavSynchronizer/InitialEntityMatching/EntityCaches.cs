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
using CalDavSynchronizer.EntityRelationManagement;
using CalDavSynchronizer.EntityVersionManagement;

namespace CalDavSynchronizer.InitialEntityMatching
{
  public class EntityCaches<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
  {
    public readonly IVersionStorage<TAtypeEntityId, TAtypeEntityVersion> AtypeStorage;
    public readonly IVersionStorage<TBtypeEntityId, TBtypeEntityVersion> BtypeStorage;
    public readonly IEntityRelationStorage<TAtypeEntityId, TBtypeEntityId> EntityRelationStorage;

    public EntityCaches (IVersionStorage<TAtypeEntityId, TAtypeEntityVersion> atypeStorage, IVersionStorage<TBtypeEntityId, TBtypeEntityVersion> btypeStorage, IEntityRelationStorage<TAtypeEntityId, TBtypeEntityId> entityRelationStorage)
    {
      AtypeStorage = atypeStorage;
      BtypeStorage = btypeStorage;
      EntityRelationStorage = entityRelationStorage;
    }
  }
}