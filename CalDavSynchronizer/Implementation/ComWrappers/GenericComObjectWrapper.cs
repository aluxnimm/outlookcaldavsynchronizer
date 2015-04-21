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
using System.Runtime.InteropServices;

namespace CalDavSynchronizer.Implementation.ComWrappers
{
  internal class GenericComObjectWrapper<T> : IDisposable
      where T : class
  {
    private T _inner;

    public GenericComObjectWrapper (T inner)
    {
      _inner = inner;
    }

    public T Inner
    {
      get { return _inner; }
    }

    public void Dispose ()
    {
      if (_inner != null)
      {
        Marshal.FinalReleaseComObject (_inner);
        _inner = null;
      }
    }
  }

  internal static class GenericComObjectWrapper
  {
    public static GenericComObjectWrapper<T> Create<T> (T inner) where T : class
    {
      return new GenericComObjectWrapper<T> (inner);
    }
  }
}