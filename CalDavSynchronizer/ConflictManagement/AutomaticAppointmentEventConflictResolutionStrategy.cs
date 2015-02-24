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
using DDay.iCal;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.ConflictManagement
{
  internal class AutomaticAppointmentEventConflictResolutionStrategy : IConflictResolutionStrategy<AppointmentItem, IEvent>
  {
    public GenericConflictResolution ResolveModifiedConflict (AppointmentItem atypeEntity, IEvent btypeEntity)
    {
      if (atypeEntity.LastModificationTime.ToUniversalTime() >= btypeEntity.LastModified.UTC)
        return GenericConflictResolution.AWins;
      else
        return GenericConflictResolution.BWins;
    }

    public GenericConflictResolution ResolveDeletionInAConflict (IEvent btypeEntity)
    {
      return GenericConflictResolution.AWins;
    }

    public GenericConflictResolution ResolveDeletionInBConflict (AppointmentItem btypeEntity)
    {
      return GenericConflictResolution.BWins;
    }
  }
}