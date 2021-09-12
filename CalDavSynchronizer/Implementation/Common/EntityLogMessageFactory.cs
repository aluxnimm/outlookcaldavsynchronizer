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

using System.Linq;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.DistributionLists.Sogo;
using CalDavSynchronizer.Implementation.GoogleContacts;
using DDay.iCal;
using GenSync.Logging;
using Google.Apis.Tasks.v1.Data;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.Common
{
    class EntityLogMessageFactory :
        IEntityLogMessageFactory<IAppointmentItemWrapper, IICalendar>,
        IEntityLogMessageFactory<ITaskItemWrapper, IICalendar>,
        IEntityLogMessageFactory<IContactItemWrapper, vCard>,
        IEntityLogMessageFactory<ITaskItemWrapper, Task>,
        IEntityLogMessageFactory<IContactItemWrapper, GoogleContactWrapper>,
        IEntityLogMessageFactory<IDistListItemWrapper, DistributionList>,
        IEntityLogMessageFactory<IDistListItemWrapper, vCard>
    {
        public static readonly EntityLogMessageFactory Instance = new EntityLogMessageFactory();

        private EntityLogMessageFactory()
        {
        }

        public string GetADisplayNameOrNull(IAppointmentItemWrapper entity)
        {
            return entity.Inner.Subject;
        }

        public string GetADisplayNameOrNull(ITaskItemWrapper entity)
        {
            return entity.Inner.Subject;
        }

        public string GetBDisplayNameOrNull(Task entity)
        {
            return entity.Title;
        }

        public string GetBDisplayNameOrNull(IICalendar entity)
        {
            return entity.Calendar.Events.FirstOrDefault()?.Summary ?? entity.Calendar.Todos.FirstOrDefault()?.Summary;
        }

        public string GetADisplayNameOrNull(IContactItemWrapper entity)
        {
            return entity.Inner.FullName;
        }

        public string GetBDisplayNameOrNull(GoogleContactWrapper entity)
        {
            return entity.Contact.Name.FullName;
        }

        public string GetBDisplayNameOrNull(vCard entity)
        {
            return entity.FormattedName;
        }

        public string GetADisplayNameOrNull(IDistListItemWrapper entity)
        {
            return entity.Inner.DLName;
        }

        public string GetBDisplayNameOrNull(DistributionList entity)
        {
            return entity.Name;
        }
    }
}