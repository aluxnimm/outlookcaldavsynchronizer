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
using System.Linq;
using CalDavSynchronizer.DataAccess;

namespace CalDavSynchronizer.Implementation.Contacts.VCardTypeSwitch
{
    public class VCardTypeCache : IVCardTypeCache
    {
        private readonly IVCardTypeCacheDataAccess _dataAccess;
        private readonly WeakReference<Dictionary<WebResourceName, VCardEntry>> _entriesReference = new WeakReference<Dictionary<WebResourceName, VCardEntry>>(new Dictionary<WebResourceName, VCardEntry>());
        private bool _areEntriesInitiallyLoaded;

        public VCardTypeCache(IVCardTypeCacheDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        public Dictionary<WebResourceName, VCardEntry> GetEntries()
        {
            Dictionary<WebResourceName, VCardEntry> entries;
            if (!(_areEntriesInitiallyLoaded && _entriesReference.TryGetTarget(out entries)))
            {
                entries = _dataAccess.Load().ToDictionary(e => e.Id);
                _entriesReference.SetTarget(entries);
                _areEntriesInitiallyLoaded = true;
            }

            return new Dictionary<WebResourceName, VCardEntry>(entries);
        }

        public void SetEntries(Dictionary<WebResourceName, VCardEntry> entries)
        {
            var copiedEntries = new Dictionary<WebResourceName, VCardEntry>(entries);
            _dataAccess.Save(copiedEntries.Values.ToArray());
            _entriesReference.SetTarget(copiedEntries);
            _areEntriesInitiallyLoaded = true;
        }
    }
}