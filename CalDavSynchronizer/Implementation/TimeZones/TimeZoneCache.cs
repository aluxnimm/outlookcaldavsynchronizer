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
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DDay.iCal;
using log4net;

namespace CalDavSynchronizer.Implementation.TimeZones
{
    public class TimeZoneCache : ITimeZoneCache
    {
        private readonly bool _includeHistoricalData;
        private readonly HttpClient _httpClient;
        private readonly GlobalTimeZoneCache _globalTimeZoneCache;

        public TimeZoneCache(HttpClient httpClient, bool includeHistoricalData, GlobalTimeZoneCache globalTimeZoneCache)
        {
            _httpClient = httpClient;
            _includeHistoricalData = includeHistoricalData;
            _globalTimeZoneCache = globalTimeZoneCache;
        }

        public async Task<ITimeZone> GetByTzIdOrNull(string tzId)
        {
            return await _globalTimeZoneCache.GetTimeZoneById(tzId, _includeHistoricalData, _httpClient);
        }
    }
}