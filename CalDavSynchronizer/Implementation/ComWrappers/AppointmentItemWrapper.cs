// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
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
using System.Threading;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.ComWrappers
{
  public class AppointmentItemWrapper : IAppointmentItemWrapper
  {
    public AppointmentItem Inner => _inner ?? throw new InvalidOperationException("Cannot access a disposed object!");

    private AppointmentItem _inner;
    private LoadAppointmentItemDelegate _load;

    public AppointmentItemWrapper(AppointmentItem inner, LoadAppointmentItemDelegate load)
    {
      _load = load;
      _inner = inner;
    }

    public void SaveAndReload ()
    {
      Inner.Save();
      var entryId = Inner.EntryID;
      DisposeInner();
      Thread.MemoryBarrier();
      _inner = _load (entryId);
    }

    private void DisposeInner ()
    {
      Marshal.FinalReleaseComObject (Inner);
      _inner = null;
    }

    public void Dispose ()
    {
      if (_inner != null)
      {
        DisposeInner();
        _load = null;
      }
    }

    public void Replace (AppointmentItem inner)
    {
      if (inner != Inner)
      {
        DisposeInner();
        _inner = inner;
      }
    }
  }
}