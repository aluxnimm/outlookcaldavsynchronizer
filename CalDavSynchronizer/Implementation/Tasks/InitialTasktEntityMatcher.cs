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
using CalDavSynchronizer.Generic.InitialEntityMatching;
using CalDavSynchronizer.Implementation.ComWrappers;
using DDay.iCal;

namespace CalDavSynchronizer.Implementation.Tasks
{
  internal class InitialTaskEntityMatcher : InitialEntityMatcherByPropertyGrouping<TaskItemWrapper, string, DateTime, string, ITodo, Uri, string, string>
  {
    protected override bool AreEqual (TaskItemWrapper atypeEntity, ITodo btypeEntity)
    {
      // TODO: find a rule, when two tasks are considered to be equal (maybe subject is not enough)

      return atypeEntity.Inner.Subject == btypeEntity.Summary;
    }

    protected override string GetAtypePropertyValue (TaskItemWrapper atypeEntity)
    {
      return (atypeEntity.Inner.Subject ?? string.Empty).ToLower();
    }

    protected override string GetBtypePropertyValue (ITodo btypeEntity)
    {
      return (btypeEntity.Summary ?? string.Empty).ToLower();
    }

    protected override string MapAtypePropertyValue (string value)
    {
      return value;
    }
  }
}