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
using System.Reflection;
using System.Runtime.Serialization;
using log4net;

namespace CalDavSynchronizer.EntityRelationManagement
{
  [Serializable]
  public class EntityRelationStorage<TEntityOneId,TEntityTwoId> : IEntityRelationStorage<TEntityOneId, TEntityTwoId>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod ().DeclaringType);
    
    private readonly Dictionary<TEntityOneId, TEntityTwoId> _entityTwoByEntityOne = new Dictionary<TEntityOneId, TEntityTwoId> ();
    private readonly Dictionary<TEntityTwoId, TEntityOneId> _entityOneByEntityTwo = new Dictionary<TEntityTwoId, TEntityOneId> ();

    public void AddRelation (TEntityOneId id1, TEntityTwoId id2)
    {
      _entityTwoByEntityOne.Add (id1, id2);
      _entityOneByEntityTwo.Add (id2, id1);
    }


    public bool TryGetEntity1ByEntity2 (TEntityTwoId id2, out TEntityOneId id1)
    {
      return _entityOneByEntityTwo.TryGetValue (id2, out id1);
    }

    public bool TryGetEntity2ByEntity1 (TEntityOneId id1, out TEntityTwoId id2)
    {
      return _entityTwoByEntityOne.TryGetValue (id1, out id2);
    }

    public bool TryRemoveByEntity2 (TEntityTwoId id2, out TEntityOneId id1)
    {
      if (_entityOneByEntityTwo.TryGetValue (id2, out id1))
      {
        _entityOneByEntityTwo.Remove (id2);
        _entityTwoByEntityOne.Remove (id1);
        return true;
      }
      else
      {
        return false;
      }
    }

    public bool TryRemoveByEntity1 (TEntityOneId id1, out TEntityTwoId id2)
    {
      if (_entityTwoByEntityOne.TryGetValue (id1, out id2))
      {
        _entityOneByEntityTwo.Remove (id2);
        _entityTwoByEntityOne.Remove (id1);
        return true;
      }
      else
      {
        return false;
      }
   }

  }
}
