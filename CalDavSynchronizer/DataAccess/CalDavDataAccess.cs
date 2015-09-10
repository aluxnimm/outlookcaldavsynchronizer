// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using GenSync;
using log4net;

namespace CalDavSynchronizer.DataAccess
{
  public class CalDavDataAccess : WebDavDataAccess, ICalDavDataAccess
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    public CalDavDataAccess (Uri calendarUrl, IWebDavClient calDavWebClient)
        : base (calendarUrl, calDavWebClient)
    {
    }

    public Task<bool> IsCalendarAccessSupported ()
    {
      return HasOption ("calendar-access");
    }

    public Task<bool> IsResourceCalender ()
    {
      return IsResourceType ("C", "calendar");
    }

    public Task<bool> DoesSupportCalendarQuery ()
    {
      return DoesSupportsReportSet (_serverUrl, 0, "C", "calendar-query");
    }

    private const string s_calDavDateTimeFormatString = "yyyyMMddThhmmssZ";

    public Task<IReadOnlyList<EntityIdWithVersion<Uri, string>>> GetEvents (DateTimeRange? range)
    {
      return GetEntities (range, "VEVENT");
    }

    public Task<IReadOnlyList<EntityIdWithVersion<Uri, string>>> GetTodos (DateTimeRange? range)
    {
      return GetEntities (range, "VTODO");
    }

    public Task<EntityIdWithVersion<Uri, string>> CreateEntity (string iCalData)
    {
      return CreateEntity (string.Format ("{0:D}.ics", Guid.NewGuid()), iCalData);
    }

    private async Task<IReadOnlyList<EntityIdWithVersion<Uri, string>>> GetEntities (DateTimeRange? range, string entityType)
    {
      var entities = new List<EntityIdWithVersion<Uri, string>>();

      try
      {
        var responseXml = await _webDavClient.ExecuteWebDavRequestAndReadResponse (
            _serverUrl,
            "REPORT",
            1,
            null,
            null,
            "application/xml",
            string.Format (
                @"<?xml version=""1.0""?>
                    <C:calendar-query xmlns:C=""urn:ietf:params:xml:ns:caldav"">
                        <D:prop xmlns:D=""DAV:"">
                            <D:getetag/>
                        </D:prop>
                        <C:filter>
                            <C:comp-filter name=""VCALENDAR"">
                                <C:comp-filter name=""{0}"">
                                  {1}
                                </C:comp-filter>
                            </C:comp-filter>
                        </C:filter>
                    </C:calendar-query>
                    ",
                entityType,
                range == null ? string.Empty : string.Format (@"<C:time-range start=""{0}"" end=""{1}""/>",
                    range.Value.From.ToString (s_calDavDateTimeFormatString),
                    range.Value.To.ToString (s_calDavDateTimeFormatString))
                ));


        XmlNodeList responseNodes = responseXml.XmlDocument.SelectNodes ("/D:multistatus/D:response", responseXml.XmlNamespaceManager);

        // ReSharper disable once LoopCanBeConvertedToQuery
        // ReSharper disable once PossibleNullReferenceException
        foreach (XmlElement responseElement in responseNodes)
        {
          var urlNode = responseElement.SelectSingleNode ("D:href", responseXml.XmlNamespaceManager);
          var etagNode = responseElement.SelectSingleNode ("D:propstat/D:prop/D:getetag", responseXml.XmlNamespaceManager);
          if (urlNode != null && etagNode != null)
          {
            var uri = UriHelper.UnescapeRelativeUri (_serverUrl, urlNode.InnerText);
            entities.Add (EntityIdWithVersion.Create (uri, etagNode.InnerText));
          }
        }
      }
      catch (WebException x)
      {
        if (x.Response != null)
        {
          var httpWebResponse = (HttpWebResponse) x.Response;

          if (httpWebResponse.StatusCode == HttpStatusCode.NotFound)
            return entities;
        }

        throw;
      }
      return entities;
    }

    public async Task<IReadOnlyList<EntityWithVersion<Uri, string>>> GetEntities (IEnumerable<Uri> eventUrls)
    {
      var requestBody = @"<?xml version=""1.0""?>
			                    <C:calendar-multiget xmlns:C=""urn:ietf:params:xml:ns:caldav"" xmlns:D=""DAV:"">
			                        <D:prop>
			                            <D:getetag/>
			                            <D:displayname/>
			                            <C:calendar-data/>
			                        </D:prop>
                                        " + String.Join (Environment.NewLine, eventUrls.Select (u => string.Format ("<D:href>{0}</D:href>", u))) + @"
                                    </C:calendar-multiget>";

      var responseXml = await _webDavClient.ExecuteWebDavRequestAndReadResponse (
          _serverUrl,
          "REPORT",
          1,
          null,
          null,
          "application/xml",
          requestBody
          );

      XmlNodeList responseNodes = responseXml.XmlDocument.SelectNodes ("/D:multistatus/D:response", responseXml.XmlNamespaceManager);

      var entities = new List<EntityWithVersion<Uri, string>>();

      if (responseNodes == null)
        return entities;

      // ReSharper disable once LoopCanBeConvertedToQuery
      // ReSharper disable once PossibleNullReferenceException
      foreach (XmlElement responseElement in responseNodes)
      {
        var urlNode = responseElement.SelectSingleNode ("D:href", responseXml.XmlNamespaceManager);
        var dataNode = responseElement.SelectSingleNode ("D:propstat/D:prop/C:calendar-data", responseXml.XmlNamespaceManager);
        if (urlNode != null && dataNode != null)
        {
          entities.Add (EntityWithVersion.Create (UriHelper.UnescapeRelativeUri (_serverUrl, urlNode.InnerText), dataNode.InnerText));
        }
      }

      return entities;
    }
  }
}