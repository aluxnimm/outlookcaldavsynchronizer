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
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.DDayICalWorkaround;
using CalDavSynchronizer.Implementation.ComWrappers;
using DDay.iCal;
using GenSync.EntityRelationManagement;
using GenSync.Synchronization;
using GenSync.Synchronization.States;

namespace CalDavSynchronizer.Implementation.Events
{
    internal class OutlookCaldavEventUpdateFromNewerToOlder
        : UpdateFromNewerToOlder<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext>
    {
        public OutlookCaldavEventUpdateFromNewerToOlder(EntitySyncStateEnvironment<AppointmentId, DateTime, IAppointmentItemWrapper, WebResourceName, string, IICalendar, IEventSynchronizationContext> environment, IEntityRelationData<AppointmentId, DateTime, WebResourceName, string> knownData, DateTime newA, string newB)
            : base(environment, knownData, newA, newB)
        {
        }

        protected override DateTime ModificationTimeA => _aEntity.Inner.LastModificationTime.ToUniversalTime();
        protected override DateTime? ModificationTimeB => _bEntity.Events[0].LastModified?.AsUtc();
    }
}