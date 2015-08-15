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
using GenSync.UnitTests.InitialEntityMatching;

namespace GenSync.UnitTests.Synchronization
{
  internal class IdentifierEqualityComparer : IEqualityComparer<Identifier>
  {
    public static readonly IEqualityComparer<Identifier> Instance = new IdentifierEqualityComparer();

    private IdentifierEqualityComparer ()
    {
      
    }

    public bool Equals (Identifier x, Identifier y)
    {
      return StringComparer.InvariantCultureIgnoreCase.Compare (x.Value, y.Value) == 0;
    }

    public int GetHashCode (Identifier obj)
    {
      return obj.Value.ToLower().GetHashCode();
    }
  }
}