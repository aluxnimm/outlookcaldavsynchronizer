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

namespace GenSync.Synchronization
{
    /// <summary>
    /// A helper class for creating pretty log messages
    /// </summary>
    public class VersionDeltaLoginInformation
    {
        private int _added;
        private int _deleted;
        private int _changed;
        private int _unchanged;

        public void IncUnchanged()
        {
            _unchanged++;
        }

        public void IncAdded(int value)
        {
            _added += value;
        }

        public void IncDeleted()
        {
            _deleted++;
        }

        public void IncChanged()
        {
            _changed++;
        }


        public override string ToString()
        {
            return string.Format("Unchanged: {0} , Added: {1} , Deleted {2} ,  Changed {3}", _unchanged, _added, _deleted, _changed);
        }
    }
}