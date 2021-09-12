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
using GenSync;

namespace CalDavSynchronizer.Implementation.Contacts.VCardTypeSwitch
{
    public struct IdWithType<TId>
        where TId : IEntity<WebResourceName>
    {
        public readonly TId Id;
        public readonly VCardType Type;

        public IdWithType(TId id, VCardType type)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            Id = id;
            Type = type;
        }
    }
}