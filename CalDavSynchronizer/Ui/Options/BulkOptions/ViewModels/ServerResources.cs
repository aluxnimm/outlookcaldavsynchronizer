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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.using System;

using System;
using System.Collections.Generic;
using CalDavSynchronizer.DataAccess;

namespace CalDavSynchronizer.Ui.Options.BulkOptions.ViewModels
{
    public struct ServerResources
    {
        public IReadOnlyList<CalendarData> Calendars { get; }
        public IReadOnlyList<AddressBookData> AddressBooks { get; }
        public IReadOnlyList<TaskListData> TaskLists { get; }

        public ServerResources(
            IReadOnlyList<CalendarData> calendars,
            IReadOnlyList<AddressBookData> addressBooks,
            IReadOnlyList<TaskListData> taskLists)
        {
            if (calendars == null)
                throw new ArgumentNullException(nameof(calendars));
            if (addressBooks == null)
                throw new ArgumentNullException(nameof(addressBooks));
            if (taskLists == null)
                throw new ArgumentNullException(nameof(taskLists));

            Calendars = calendars;
            AddressBooks = addressBooks;
            TaskLists = taskLists;
        }

        public bool ContainsResources => Calendars.Count > 0 || AddressBooks.Count > 0 || TaskLists.Count > 0;
    }
}