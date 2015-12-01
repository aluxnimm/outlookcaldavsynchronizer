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
using CalDavSynchronizer.Implementation.ComWrappers;
using DDay.iCal;
using GenSync.EntityRelationManagement;
using GenSync.Synchronization;
using GenSync.Synchronization.States;

namespace CalDavSynchronizer.Implementation.Events
{
  internal class OutlookCaldavEventUpdateFromNewerToOlder
      : UpdateFromNewerToOlder<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar>
  {
    public OutlookCaldavEventUpdateFromNewerToOlder (EntitySyncStateEnvironment<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> environment, IEntityRelationData<string, DateTime, Uri, string> knownData, DateTime newA, string newB)
        : base (environment, knownData, newA, newB)
    {
    }

    protected override bool AIsNewerThanB
    {
      get
      {
        var evt = _bEntity.Events[0];

        // Assume that no modification means, that the item is never modified. Therefore it must be new. 
        if (evt.LastModified == null)
          return false;

        return _aEntity.Inner.LastModificationTime.ToUniversalTime() >= evt.LastModified.UTC;
      }
    }
  }
}