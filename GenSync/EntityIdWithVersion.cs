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

namespace GenSync
{
  /// <summary>
  /// Represents the Id and the Version of an entity
  /// </summary>
  public class EntityIdWithVersion<TEntityId, TVersion>
  {
    public readonly TEntityId Id;
    public readonly TVersion Version;

    public EntityIdWithVersion (TEntityId id, TVersion version)
    {
      Id = id;
      Version = version;
    }
  }

  public class EntityIdWithVersion
  {
    public static EntityIdWithVersion<TEntityId, TEntity> Create<TEntityId, TEntity> (TEntityId id, TEntity entity)
    {
      return new EntityIdWithVersion<TEntityId, TEntity> (id, entity);
    }
  }
}