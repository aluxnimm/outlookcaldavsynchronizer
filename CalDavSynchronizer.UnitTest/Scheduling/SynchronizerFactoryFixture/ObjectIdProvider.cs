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
using System.Runtime.CompilerServices;

namespace CalDavSynchronizer.UnitTest.Scheduling.SynchronizerFactoryFixture
{
    public class ObjectIdProvider
    {
        private readonly Dictionary<Type, Dictionary<int, int>> _objectIdsByHashCodeByType = new Dictionary<Type, Dictionary<int, int>>();

        public int GetId(object o)
        {
            var type = o.GetType();
            if (type.IsValueType || type.IsPrimitive) //  Id doesn't make sense for valuetypes or strings
                return 1;

            return GetObjectId(GetObjectIdsByHashCode(type), RuntimeHelpers.GetHashCode(o));
        }

        Dictionary<int, int> GetObjectIdsByHashCode(Type type)
        {
            Dictionary<int, int> value;
            if (!_objectIdsByHashCodeByType.TryGetValue(type, out value))
            {
                value = new Dictionary<int, int>();
                _objectIdsByHashCodeByType.Add(type, value);
            }

            return value;
        }

        int GetObjectId(Dictionary<int, int> objectIdsByHashCode, int hashCode)
        {
            int objectId;
            if (!objectIdsByHashCode.TryGetValue(hashCode, out objectId))
            {
                objectId = objectIdsByHashCode.Count + 1;
                objectIdsByHashCode.Add(hashCode, objectId);
            }

            return objectId;
        }
    }
}