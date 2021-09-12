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
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.ComWrappers
{
    // Use explicit delegates, since Func<,> cannot be used, because that would mean that IComWrapperFactory 
    // would use a generic with an Com-Class ( Func<string,AppointmentItem> ). But classes containing a generic with an Com-Class
    // cannot be used over assembly boundaries
    public delegate AppointmentItem LoadAppointmentItemDelegate(string id);

    public delegate ContactItem LoadContactItemDelegate(string id);

    public delegate TaskItem LoadTaskItemDelegate(string id);

    public interface IComWrapperFactory
    {
        IAppointmentItemWrapper Create(AppointmentItem inner, LoadAppointmentItemDelegate load);
        IContactItemWrapper Create(ContactItem inner, LoadContactItemDelegate load);
        ITaskItemWrapper Create(TaskItem inner, LoadTaskItemDelegate load);
        IDistListItemWrapper Create(DistListItem inner);
    }
}