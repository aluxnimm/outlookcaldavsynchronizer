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
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DDay.iCal;
using log4net;

namespace CalDavSynchronizer.Implementation.TimeZones
{
    public class GlobalTimeZoneCache
    {
        private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        private const string TZURL_FULL = "http://tzurl.org/zoneinfo/";
        private const string TZURL_OUTLOOK = "http://tzurl.org/zoneinfo-outlook/";

        private readonly Dictionary<string, ITimeZone> _tzOutlookMap;
        private readonly Dictionary<string, ITimeZone> _tzHistoricalMap;

        public GlobalTimeZoneCache()
        {
            _tzOutlookMap = new Dictionary<string, ITimeZone>();
            _tzHistoricalMap = new Dictionary<string, ITimeZone>();
        }

        public async Task<ITimeZone> GetTimeZoneById(string tzId, bool includeHistoricalData, HttpClient httpClient)
        {
            ITimeZone tz = GetTzOrNull(tzId, includeHistoricalData);
            if (tz == null)
            {
                var baseurl = includeHistoricalData ? TZURL_FULL : TZURL_OUTLOOK;
                var uri = new Uri(baseurl + tzId + ".ics");
                var col = await LoadFromUriOrNull(httpClient, uri);
                if (col != null)
                {
                    tz = col[0].TimeZones[0];
                    AddTz(tzId, tz, includeHistoricalData);
                }
            }

            return tz;
        }

        private ITimeZone GetTzOrNull(string tzId, bool includeHistoricalData)
        {
            if (includeHistoricalData)
            {
                return _tzHistoricalMap.ContainsKey(tzId) ? _tzHistoricalMap[tzId] : null;
            }
            else
            {
                return _tzOutlookMap.ContainsKey(tzId) ? _tzOutlookMap[tzId] : null;
            }
        }

        private void AddTz(string tzId, ITimeZone timeZone, bool includeHistoricalData)
        {
            if (includeHistoricalData)
            {
                _tzHistoricalMap.Add(tzId, timeZone);
            }
            else
            {
                _tzOutlookMap.Add(tzId, timeZone);
            }
        }

        private async Task<IICalendarCollection> LoadFromUriOrNull(HttpClient httpClient, Uri uri)
        {

            using (var response = await httpClient.GetAsync(uri))
            {
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception)
                {
                    s_logger.ErrorFormat("Can't access timezone data from '{0}'", uri);
                    return null;
                }

                try
                {
                    var result = await response.Content.ReadAsStringAsync();
                    using (var reader = new StringReader(result))
                    {
                        var collection = iCalendar.LoadFromStream(reader);
                        return collection;
                    }
                }
                catch (Exception)
                {
                    s_logger.ErrorFormat("Can't parse timezone data from '{0}'", uri);
                    return null;
                }
            }
        }
    }
}
