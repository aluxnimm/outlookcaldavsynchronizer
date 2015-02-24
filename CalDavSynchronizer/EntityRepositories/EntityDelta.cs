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

namespace CalDavSynchronizer.EntityRepositories
{
  public class EntityDelta<TEntity,TEntityId>
  {
    public IDictionary<TEntityId, TEntity> Added { get; private set; }
    public IDictionary<TEntityId, TEntity> Changed { get; private set; }
    public IDictionary<TEntityId, bool> Deleted { get; private set; }

    public EntityDelta (IDictionary<TEntityId, TEntity> added, IDictionary<TEntityId, TEntity> changed, IDictionary<TEntityId, bool> deleted)
    {
      Added = added;
      Changed = changed;
      Deleted = deleted;
    }

    public EntityDelta ()
      : this(new Dictionary<TEntityId, TEntity>(),new Dictionary<TEntityId, TEntity>(),new Dictionary<TEntityId, bool> ())
    {
    }
  }
}