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
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using System.Xml;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using CalDavSynchronizer.Utilities;
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

    public async Task<IReadOnlyList<CalendarData>> GetUserCalendarsNoThrow (bool useWellKnownUrl)
    {
      try
      {
        var autodiscoveryUrl = useWellKnownUrl ? AutoDiscoveryUrl : _serverUrl;

        var currentUserPrincipalUrl = await GetCurrentUserPrincipalUrl (autodiscoveryUrl);

        var calendars = new List<CalendarData>();

        if (currentUserPrincipalUrl != null)
        {
          var calendarHomeSetProperties = await GetCalendarHomeSet (currentUserPrincipalUrl);

          XmlNode homeSetNode = calendarHomeSetProperties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/C:calendar-home-set", calendarHomeSetProperties.XmlNamespaceManager);

          if (homeSetNode != null && !string.IsNullOrEmpty (homeSetNode.InnerText))
          {
            var calendarDocument = await ListCalendars (new Uri (calendarHomeSetProperties.DocumentUri.GetLeftPart (UriPartial.Authority) + homeSetNode.InnerText));

            XmlNodeList responseNodes = calendarDocument.XmlDocument.SelectNodes ("/D:multistatus/D:response", calendarDocument.XmlNamespaceManager);

            foreach (XmlElement responseElement in responseNodes)
            {
              var urlNode = responseElement.SelectSingleNode ("D:href", calendarDocument.XmlNamespaceManager);
              var displayNameNode = responseElement.SelectSingleNode ("D:propstat/D:prop/D:displayname", calendarDocument.XmlNamespaceManager);
              if (urlNode != null && displayNameNode != null)
              {
                XmlNode isCollection = responseElement.SelectSingleNode ("D:propstat/D:prop/D:resourcetype/C:calendar", calendarDocument.XmlNamespaceManager);
                if (isCollection != null)
                {
                  var calendarColorNode = responseElement.SelectSingleNode("D:propstat/D:prop/E:calendar-color", calendarDocument.XmlNamespaceManager);
                  ArgbColor? calendarColor = null;
                  if (calendarColorNode != null && calendarColorNode.InnerText.Length >=7)
                  {
                    calendarColor = ArgbColor.FromRgbaHexStringWithOptionalANoThrow(calendarColorNode.InnerText);
                  }

                  calendars.Add (new CalendarData (new Uri(calendarDocument.DocumentUri, urlNode.InnerText), displayNameNode.InnerText, calendarColor));
                }
              }
            }
          }
        }
        return calendars;
      }
      catch (Exception x)
      {
        if (x.Message.Contains ("404") || x.Message.Contains ("405") || x is XmlException)
          return new CalendarData[] { };
        else
          throw;
      }
    }

    private async Task<Uri> GetCurrentUserPrincipalUrl (Uri calenderUrl)
    {
      var principalProperties = await GetCurrentUserPrincipal (calenderUrl);

      XmlNode principalUrlNode = principalProperties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-principal", principalProperties.XmlNamespaceManager) ??
                                 principalProperties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:principal-URL", principalProperties.XmlNamespaceManager);

      if (principalUrlNode != null && !string.IsNullOrEmpty (principalUrlNode.InnerText))
        return new Uri (principalProperties.DocumentUri.GetLeftPart (UriPartial.Authority) + principalUrlNode.InnerText);
      else
        return null;
    }


    private Uri AutoDiscoveryUrl
    {
      get 
      {
        return new Uri (_serverUrl.GetLeftPart (UriPartial.Authority) + "/.well-known/caldav"); 
      }
    }

    private Task<XmlDocumentWithNamespaceManager> GetCalendarHomeSet (Uri url)
    {
      return _webDavClient.ExecuteWebDavRequestAndReadResponse (
          url,
          "PROPFIND",
          0,
          null,
          null,
          "application/xml",
          @"<?xml version='1.0'?>
                        <D:propfind xmlns:D=""DAV:"" xmlns:C=""urn:ietf:params:xml:ns:caldav"">
                          <D:prop>
                            <C:calendar-home-set/>
                          </D:prop>
                        </D:propfind>
                 "
          );
    }

    private Task<XmlDocumentWithNamespaceManager> ListCalendars (Uri url)
    {
      return _webDavClient.ExecuteWebDavRequestAndReadResponse (
          url,
          "PROPFIND",
          1,
          null,
          null,
          "application/xml",
          @"<?xml version='1.0'?>
                        <D:propfind xmlns:D=""DAV:"" xmlns:C=""urn:ietf:params:xml:ns:caldav"" xmlns:E=""http://apple.com/ns/ical/"">
                          <D:prop>
                              <D:resourcetype />
                              <D:displayname />
                              <E:calendar-color />
                              <C:supported-calendar-component-set />
                          </D:prop>
                        </D:propfind>
                 "
          );
    }

    public async Task<ArgbColor?> GetCalendarColorNoThrow ()
    {
      try
      {
        var document = await _webDavClient.ExecuteWebDavRequestAndReadResponse(
            _serverUrl,
            "PROPFIND",
            0,
            null,
            null,
            "application/xml",
            @"<?xml version='1.0'?>
                      <D:propfind xmlns:D=""DAV:"" xmlns:C=""urn:ietf:params:xml:ns:caldav"" xmlns:E=""http://apple.com/ns/ical/"">
                          <D:prop>
                              <E:calendar-color />
                          </D:prop>
                      </D:propfind>
                 "
            );

        var calendarColorNode = document.XmlDocument.SelectSingleNode("/D:multistatus/D:response/D:propstat/D:prop/E:calendar-color", document.XmlNamespaceManager);
        if (calendarColorNode != null && calendarColorNode.InnerText.Length >= 7)
        {
          return ArgbColor.FromRgbaHexStringWithOptionalANoThrow (calendarColorNode.InnerText);
        }
        return null;
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
        return null;
      }
    }
    
    

    public async Task<bool> SetCalendarColorNoThrow (ArgbColor color)
    {
      try
      {
        await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders(
            _serverUrl,
            "PROPPATCH",
            0,
            null,
            null,
            "application/xml",
            string.Format(
                  @"<?xml version='1.0'?>
                      <D:propertyupdate xmlns:D=""DAV:"" xmlns:C=""urn:ietf:params:xml:ns:caldav"" xmlns:E=""http://apple.com/ns/ical/"">
                        <D:set>
                          <D:prop>
                              <E:calendar-color >{0}</E:calendar-color>
                          </D:prop>
                        </D:set>
                      </D:propertyupdate>
                 ", color.ToRgbaHexString())
            );
        return true;
      }
      catch (Exception x)
      {
        s_logger.Error(null, x);
        return false;
      }
    }

    private const string s_calDavDateTimeFormatString = "yyyyMMddTHHmmssZ";

    public Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetEventVersions (DateTimeRange? range)
    {
      return GetVersions (range, "VEVENT");
    }

    public Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetTodoVersions (DateTimeRange? range)
    {
      return GetVersions (range, "VTODO");
    }

    public Task<EntityVersion<WebResourceName, string>> CreateEntity (string iCalData, string uid)
    {
      return CreateNewEntity (string.Format ("{0:D}.ics", uid), iCalData);
    }

    protected async Task<EntityVersion<WebResourceName, string>> CreateNewEntity (string name, string content)
    {
      var eventUrl = new Uri (_serverUrl, name);

      s_logger.DebugFormat ("Creating entity '{0}'", eventUrl);

      IHttpHeaders responseHeaders = await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders(
        eventUrl,
        "PUT",
        null,
        null,
        "*",
        "text/calendar",
        content);

      Uri effectiveEventUrl;
      if (responseHeaders.Location != null)
      {
        s_logger.DebugFormat ("Server sent new location: '{0}'", responseHeaders.Location);
        effectiveEventUrl = responseHeaders.Location.IsAbsoluteUri ? responseHeaders.Location : new Uri (_serverUrl, responseHeaders.Location);
        s_logger.DebugFormat ("New entity location: '{0}'", effectiveEventUrl);
      }
      else
      {
        effectiveEventUrl = eventUrl;
      }

      var etag = responseHeaders.ETag;
      string version;
      if (etag != null)
      {
        version = etag;
      }
      else
      {
        version = await GetEtag (effectiveEventUrl);
      }

      return new EntityVersion<WebResourceName, string> (new WebResourceName(effectiveEventUrl), version);
    }

    public async Task<EntityVersion<WebResourceName, string>> TryUpdateEntity (WebResourceName url, string etag, string contents)
    {
      s_logger.DebugFormat ("Updating entity '{0}'", url);

      var absoluteEventUrl = new Uri(_serverUrl, url.OriginalAbsolutePath);

      s_logger.DebugFormat ("Absolute entity location: '{0}'", absoluteEventUrl);

      IHttpHeaders responseHeaders;

      try
      {
      responseHeaders = await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders(
          absoluteEventUrl,
          "PUT",
          null,
          etag,
          null,
          "text/calendar",
          contents);
      }
      catch (WebDavClientException x) when (x.StatusCode == HttpStatusCode.NotFound || x.StatusCode == HttpStatusCode.PreconditionFailed)
      {
        return null;
      }

      if (s_logger.IsDebugEnabled)
        s_logger.DebugFormat ("Updated entity. Server response header: '{0}'", responseHeaders.ToString().Replace ("\r\n", " <CR> "));

      Uri effectiveEventUrl;
      if (responseHeaders.Location != null)
      {
        s_logger.DebugFormat ("Server sent new location: '{0}'", responseHeaders.Location);
        effectiveEventUrl = responseHeaders.Location.IsAbsoluteUri ? responseHeaders.Location : new Uri(_serverUrl, responseHeaders.Location);
        s_logger.DebugFormat ("New entity location: '{0}'", effectiveEventUrl);
      }
      else
      {
        effectiveEventUrl = absoluteEventUrl;
      }

      var newEtag = responseHeaders.ETag;
      string version;
      if (newEtag != null)
      {
        version = newEtag;
      }
      else
      {
        version = await GetEtag (effectiveEventUrl);
      }

      return new EntityVersion<WebResourceName, string> (new WebResourceName(effectiveEventUrl), version);
    }

    private async Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetVersions (DateTimeRange? range, string entityType)
    {
      var entities = new List<EntityVersion<WebResourceName, string>>();

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
          if (urlNode != null &&
              etagNode != null &&
              _serverUrl.AbsolutePath != UriHelper.DecodeUrlString (urlNode.InnerText))
          {
            var uri = new WebResourceName (urlNode.InnerText);
            entities.Add (EntityVersion.Create (uri, HttpUtility.GetQuotedEtag (etagNode.InnerText)));
          }
        }
      }
      catch (WebDavClientException x)
      {
        // Workaround for Synology NAS, which returns 404 insteaod of an empty response if no events are present
        if (x.StatusCode == HttpStatusCode.NotFound && await IsResourceCalender())
          return entities;

        throw;
      }

      return entities;
    }

    public async Task<IReadOnlyList<EntityWithId<WebResourceName, string>>> GetEntities (IEnumerable<WebResourceName> eventUrls)
    {
      var requestBody = @"<?xml version=""1.0""?>
			                    <C:calendar-multiget xmlns:C=""urn:ietf:params:xml:ns:caldav"" xmlns:D=""DAV:"">
			                        <D:prop>
			                            <D:getetag/>
			                            <D:displayname/>
			                            <C:calendar-data/>
			                        </D:prop>
                                        " + String.Join ("\r\n", eventUrls.Select (u => string.Format ("<D:href>{0}</D:href>", SecurityElement.Escape(u.OriginalAbsolutePath)))) + @"
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

      var entities = new List<EntityWithId<WebResourceName, string>>();

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
          entities.Add (EntityWithId.Create (new WebResourceName(urlNode.InnerText), dataNode.InnerText));
        }
      }

      return entities;
    }

    public async Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetVersions (IEnumerable<WebResourceName> eventUrls)
    {
      var requestBody = @"<?xml version=""1.0""?>
			                    <C:calendar-multiget xmlns:C=""urn:ietf:params:xml:ns:caldav"" xmlns:D=""DAV:"">
			                        <D:prop>
			                            <D:getetag/>
			                        </D:prop>
                                        " + String.Join ("\r\n", eventUrls.Select (u => string.Format ("<D:href>{0}</D:href>", SecurityElement.Escape(u.OriginalAbsolutePath)))) + @"
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

      var entities = new List<EntityVersion<WebResourceName, string>>();

      if (responseNodes == null)
        return entities;

      // ReSharper disable once LoopCanBeConvertedToQuery
      // ReSharper disable once PossibleNullReferenceException
      foreach (XmlElement responseElement in responseNodes)
      {
        var urlNode = responseElement.SelectSingleNode ("D:href", responseXml.XmlNamespaceManager);
        var etagNode = responseElement.SelectSingleNode ("D:propstat/D:prop/D:getetag", responseXml.XmlNamespaceManager);
        if (urlNode != null && etagNode != null)
        {
          entities.Add (EntityVersion.Create (new WebResourceName (urlNode.InnerText),
                                              HttpUtility.GetQuotedEtag (etagNode.InnerText)));
        }
      }

      return entities;
    }
  }
}