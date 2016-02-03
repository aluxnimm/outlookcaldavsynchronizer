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
using CalDavSynchronizer.Implementation.ComWrappers;
using DDay.iCal;
using GenSync.InitialEntityMatching;
using Google.Apis.Tasks.v1.Data;

namespace CalDavSynchronizer.Implementation.GoogleTasks
{
  internal class InitialGoogleTasktEntityMatcher : InitialEntityMatcherByPropertyGrouping<TaskItemWrapper, string, DateTime, string, Task, string, string, string>
  {
    public InitialGoogleTasktEntityMatcher (IEqualityComparer<string> btypeIdEqualityComparer)
        : base (btypeIdEqualityComparer)
    {
    }

    protected override bool AreEqual (TaskItemWrapper atypeEntity, Task btypeEntity)
    {
      return atypeEntity.Inner.Subject == btypeEntity.Title;
      // TODO: check also date
    }

    protected override string GetAtypePropertyValue (TaskItemWrapper atypeEntity)
    {
      return (atypeEntity.Inner.Subject ?? string.Empty).ToLower();
    }

    protected override string GetBtypePropertyValue (Task btypeEntity)
    {
      return (btypeEntity.Title ?? string.Empty).ToLower();
    }

    protected override string MapAtypePropertyValue (string value)
    {
      return value;
    }
  }
}