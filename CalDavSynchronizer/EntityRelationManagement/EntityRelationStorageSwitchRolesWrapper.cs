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

namespace CalDavSynchronizer.EntityRelationManagement
{
  [Serializable]
  public class EntityRelationStorageSwitchRolesWrapper<TEntityOneId, TEntityTwoId> : IEntityRelationStorage<TEntityOneId, TEntityTwoId>
  {
    private readonly IEntityRelationStorage<TEntityTwoId, TEntityOneId> _inner;

    public EntityRelationStorageSwitchRolesWrapper (IEntityRelationStorage<TEntityTwoId, TEntityOneId> inner)
    {
      _inner = inner;
    }

    public IEntityRelationStorage<TEntityTwoId, TEntityOneId> Inner
    {
      get { return _inner; }
    }

    public void AddRelation (TEntityOneId id1, TEntityTwoId id2)
    {
      Inner.AddRelation (id2, id1);
    }

    public bool TryGetEntity1ByEntity2 (TEntityTwoId id2, out TEntityOneId id1)
    {
      return Inner.TryGetEntity2ByEntity1 (id2, out id1);
    }

    public bool TryGetEntity2ByEntity1 (TEntityOneId id1, out TEntityTwoId id2)
    {
      return Inner.TryGetEntity1ByEntity2 (id1, out id2);
    }

    public bool TryRemoveByEntity2 (TEntityTwoId id2, out TEntityOneId id1)
    {
      return Inner.TryRemoveByEntity1 (id2, out id1);
    }

    public bool TryRemoveByEntity1 (TEntityOneId id1, out TEntityTwoId id2)
    {
      return Inner.TryRemoveByEntity2 (id1, out id2);
    }
  }
}