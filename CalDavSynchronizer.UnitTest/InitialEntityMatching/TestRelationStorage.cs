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

namespace CalDavSynchronizer.UnitTest.InitialEntityMatching
{
  public class TestRelationStorage : IEntityRelationStorage<int, string>
  {
    private readonly Dictionary<int, string> _abyB = new Dictionary<int, string> ();
    private readonly Dictionary<string, int> _bbyA = new Dictionary<string, int> ();

    public Dictionary<int, string> BbyA
    {
      get { return _abyB; }
    }

    public Dictionary<string, int> AbyB
    {
      get { return _bbyA; }
    }

    public void AddRelation (int id1, string id2)
    {
      BbyA.Add (id1, id2);
      AbyB.Add (id2, id1);
    }

    public bool TryGetEntity1ByEntity2 (string id2, out int id1)
    {
      throw new NotImplementedException ();
    }

    public bool TryGetEntity2ByEntity1 (int id1, out string id2)
    {
      throw new NotImplementedException ();
    }

    public bool TryRemoveByEntity2 (string id2, out int id1)
    {
      throw new NotImplementedException ();
    }

    public bool TryRemoveByEntity1 (int id1, out string id2)
    {
      throw new NotImplementedException ();
    }
  }
}