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
using System.Collections.Generic;
using CalDavSynchronizer.DataAccess;

namespace CalDavSynchronizer.Implementation.Events
{
  public class AppointmentId
  {
    public string EntryId {get; set; }
    public string GlobalAppointmentId {get; set; }

    public AppointmentId(string entryId, string globalAppointmentId)
    {
      EntryId = entryId;
      GlobalAppointmentId = globalAppointmentId;
    }

    public AppointmentId ()
    {
      
    }

    public override string ToString ()
    {
      return EntryId;
    }

    public override int GetHashCode ()
    {
      return Comparer.GetHashCode (this);
    }

    public override bool Equals (object obj)
    {
      return Comparer.Equals (this, obj as AppointmentId);
    }

    public static readonly IEqualityComparer<AppointmentId> Comparer = new AppointmentIdEqualityComparer ();

    private class AppointmentIdEqualityComparer : IEqualityComparer<AppointmentId>
    {
      private static readonly IEqualityComparer<string> s_stringComparer = StringComparer.Ordinal;

      public bool Equals (AppointmentId x, AppointmentId y)
      {
        if (x == null)
          return y == null;

        if (y == null)
          return false;

        return s_stringComparer.Equals (x.EntryId, y.EntryId);
      }

      public int GetHashCode (AppointmentId obj)
      {
        return s_stringComparer.GetHashCode (obj.EntryId);
      }
    }
  }
}