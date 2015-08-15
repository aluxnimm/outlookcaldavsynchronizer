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

namespace GenSync.UnitTests.InitialEntityMatching
{
  public struct Identifier<T>
  {
    public readonly T Value;

    public Identifier (T value)
    {
      Value = value;
    }

    // Override equals and return false, to verify if EqualityComparer is used
    public override bool Equals (object obj)
    {
      return false;
    }

    public override int GetHashCode ()
    {
      return 0;
    }
  }
}