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

namespace CalDavSynchronizer.EntityVersionManagement
{
  public class EntityIdWithVersionByIdComparer<TEntityId, TVersion> : IEqualityComparer<EntityIdWithVersion<TEntityId, TVersion>>
  {
    private readonly IEqualityComparer<TEntityId> _idComparer;

    public EntityIdWithVersionByIdComparer (IEqualityComparer<TEntityId> idComparer)
    {
      if (idComparer == null)
        throw new ArgumentNullException ("idComparer");
      _idComparer = idComparer;
    }

    public bool Equals (EntityIdWithVersion<TEntityId, TVersion> x, EntityIdWithVersion<TEntityId, TVersion> y)
    {
      return _idComparer.Equals (x.Id, y.Id);
    }

    public int GetHashCode (EntityIdWithVersion<TEntityId, TVersion> obj)
    {
      return _idComparer.GetHashCode (obj.Id);
    }
  }
}