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
using System.Text;

namespace CalDavSynchronizer.EntityVersionManagement
{
  public struct VersionDelta<TEntityId, TVersion>
  {
    public readonly IReadOnlyList<EntityIdWithVersion<TEntityId, TVersion>> Added;
    public readonly IReadOnlyList<EntityIdWithVersion<TEntityId, TVersion>> Deleted;
    public readonly IReadOnlyList<EntityIdWithVersion<TEntityId, TVersion>> Changed;

    public VersionDelta (IReadOnlyList<EntityIdWithVersion<TEntityId, TVersion>> added, IReadOnlyList<EntityIdWithVersion<TEntityId, TVersion>> deleted, IReadOnlyList<EntityIdWithVersion<TEntityId, TVersion>> changed)
        : this()
    {
      if (added == null)
        throw new ArgumentNullException ("added");
      if (deleted == null)
        throw new ArgumentNullException ("deleted");
      if (changed == null)
        throw new ArgumentNullException ("changed");

      Added = added;
      Deleted = deleted;
      Changed = changed;
    }

    public override string ToString ()
    {
      return ToString (false);
    }

    public string ToString (bool includeIdLists)
    {
      if (includeIdLists)
      {
        var stringBuilder = new StringBuilder();

        AppendEvents (stringBuilder, "ADDED", Added);
        AppendEvents (stringBuilder, "DELETED", Deleted);
        AppendEvents (stringBuilder, "CHANGED", Changed);

        return stringBuilder.ToString();
      }
      else
      {
        return string.Format ("Added: {0} , Deleted {1} ,  Changed {2}", Added.Count, Deleted.Count, Changed.Count);
      }
    }

    private void AppendEvents (StringBuilder stringBuilder, string caption, IReadOnlyList<EntityIdWithVersion<TEntityId, TVersion>> events)
    {
      if (events.Count > 0)
      {
        stringBuilder.AppendLine (caption);
        foreach (var evt in events)
          stringBuilder.AppendLine (evt.Id.ToString());
      }
    }
  }
}