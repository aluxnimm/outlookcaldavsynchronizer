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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Scheduling;
using DDay.iCal;
using log4net;

namespace CalDavSynchronizer.Implementation.Events
{
  public class TimeZoneMapper
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private const string TZURL_FULL = "http://tzurl.org/zoneinfo/";
    private const string TZURL_OUTLOOK = "http://tzurl.org/zoneinfo-outlook/";

    private readonly bool _includeHistoricalData;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, ITimeZone> _tzMap;

    public TimeZoneMapper (ProxyOptions proxyOptions, bool includeHistoricalData)
    {
      var proxy = (proxyOptions != null) ? SynchronizerFactory.CreateProxy (proxyOptions) : null;
      var httpClientHandler = new HttpClientHandler
      {
        Proxy = proxy,
        UseProxy = (proxy != null)
      };

      _httpClient = new HttpClient (httpClientHandler);

      _tzMap = new Dictionary<string, ITimeZone>();
      _includeHistoricalData = includeHistoricalData;
    }

    public async Task <ITimeZone> GetByTzIdOrNull (string tzId)
    {
      if (_tzMap.ContainsKey (tzId))
        return _tzMap[tzId];
      else
      {
        var baseurl = _includeHistoricalData ? TZURL_FULL : TZURL_OUTLOOK;
        var uri = new Uri (baseurl + tzId + ".ics");
        var col = await LoadFromUriOrNull (uri);
        if (col != null)
        {
          var tz = col[0].TimeZones[0];
          _tzMap.Add (tzId, tz);
          return tz;
        }
        return null;
      }
    }

    private async Task <IICalendarCollection> LoadFromUriOrNull (Uri uri)
    {

      using (var response = await _httpClient.GetAsync (uri))
      {
        try
        {
          response.EnsureSuccessStatusCode();
        }
        catch (Exception)
        {
          s_logger.ErrorFormat ("Can't access timezone data from '{0}'", uri);
          return null;
        }

        try
        {
          var result = await response.Content.ReadAsStringAsync();
          using (var reader = new StringReader (result))
          {
            var collection = iCalendar.LoadFromStream (reader);
            return collection;
          }
        }
        catch (Exception)
        {
          s_logger.ErrorFormat ("Can't parse timezone data from '{0}'", uri);
          return null;
        }
      }
    }

    public static string IanaToWindows (string ianaZoneId)
    {
      var utcZones = new[] { "Etc/UTC", "Etc/UCT", "Etc/GMT" };
      if (utcZones.Contains (ianaZoneId, StringComparer.Ordinal))
        return "UTC";

      var tzdbSource = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default;

      // resolve any link, since the CLDR doesn't necessarily use canonical IDs
      var links = tzdbSource.CanonicalIdMap
          .Where (x => x.Value.Equals (ianaZoneId, StringComparison.Ordinal))
          .Select (x => x.Key);

      // resolve canonical zones, and include original zone as well
      var possibleZones = tzdbSource.CanonicalIdMap.ContainsKey (ianaZoneId)
          ? links.Concat (new[] { tzdbSource.CanonicalIdMap[ianaZoneId], ianaZoneId })
          : links;

      // map the windows zone
      var mappings = tzdbSource.WindowsMapping.MapZones;
      var item = mappings.FirstOrDefault (x => x.TzdbIds.Any (possibleZones.Contains));
      if (item == null)
        return null;
      return item.WindowsId;
    }

    // This will return the "primary" IANA zone that matches the given windows zone.
    // If the primary zone is a link, it then resolves it to the canonical ID.
    public static string WindowsToIana (string windowsZoneId)
    {
      if (windowsZoneId.Equals ("UTC", StringComparison.Ordinal))
        return "Etc/UTC";

      var tzdbSource = NodaTime.TimeZones.TzdbDateTimeZoneSource.Default;
      var tzi = TimeZoneInfo.FindSystemTimeZoneById (windowsZoneId);
      if (tzi == null)
        return null;
      var tzid = tzdbSource.MapTimeZoneId (tzi);
      if (tzid == null)
        return null;
      return tzdbSource.CanonicalIdMap[tzid];
    }
  }
}