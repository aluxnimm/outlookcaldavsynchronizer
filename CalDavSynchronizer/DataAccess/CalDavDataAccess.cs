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
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security;
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

    public async Task<IReadOnlyList<Tuple<Uri, string, string>>> GetUserCalendarsNoThrow (bool useWellKnownUrl)
    {
      try
      {
        var autodiscoveryUrl = useWellKnownUrl ? AutoDiscoveryUrl : _serverUrl;

        var properties = await GetCurrentUserPrincipal (autodiscoveryUrl);

        XmlNode principal = properties.XmlDocument.SelectSingleNode("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-principal", properties.XmlNamespaceManager);

        // changes to handle Zoho Calendar
        // patch from Suki Hirata <thirata@outlook.com>
        if (null == principal)
        {
          principal = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:principal-URL", properties.XmlNamespaceManager);
        }

        var cals = new List<Tuple<Uri, string, string>>();

        if (principal != null && !string.IsNullOrEmpty (principal.InnerText))
        {
          properties = await GetCalendarHomeSet (new Uri (autodiscoveryUrl.GetLeftPart (UriPartial.Authority) + principal.InnerText));

          XmlNode homeSet = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/C:calendar-home-set", properties.XmlNamespaceManager);

          if (homeSet != null && !string.IsNullOrEmpty (homeSet.InnerText))
          {
            properties = await ListCalendars (new Uri (autodiscoveryUrl.GetLeftPart (UriPartial.Authority) + homeSet.InnerText));

            XmlNodeList responseNodes = properties.XmlDocument.SelectNodes ("/D:multistatus/D:response", properties.XmlNamespaceManager);

            foreach (XmlElement responseElement in responseNodes)
            {
              var urlNode = responseElement.SelectSingleNode ("D:href", properties.XmlNamespaceManager);
              var displayNameNode = responseElement.SelectSingleNode ("D:propstat/D:prop/D:displayname", properties.XmlNamespaceManager);
              if (urlNode != null && displayNameNode != null)
              {
                XmlNode isCollection = responseElement.SelectSingleNode ("D:propstat/D:prop/D:resourcetype/C:calendar", properties.XmlNamespaceManager);
                if (isCollection != null)
                {
                  var calendarColorNode = responseElement.SelectSingleNode("D:propstat/D:prop/E:calendar-color", properties.XmlNamespaceManager);
                  string calendarColor=string.Empty;
                  if (calendarColorNode != null && calendarColorNode.InnerText.Length >=7)
                  {
                    calendarColor = calendarColorNode.InnerText;
                  }
                  var uri = UriHelper.UnescapeRelativeUri (autodiscoveryUrl, urlNode.InnerText);
                  cals.Add (Tuple.Create (uri, displayNameNode.InnerText, calendarColor));
                }
              }
            }
          }
        }
        return cals;
      }
      catch (Exception x)
      {
        if (x.Message.Contains ("404") || x.Message.Contains ("405") || x is XmlException)
          return new List<Tuple<Uri, string, string>>();
        else
          throw;
      }
    }

    private Uri AutoDiscoveryUrl
    {
      get 
      {
        return new Uri (_serverUrl.GetLeftPart (UriPartial.Authority) + "/.well-known/caldav/"); 
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

    public async Task<string> GetCalendarColorNoThrow ()
    {
      string calendarColor = string.Empty;

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
          calendarColor = calendarColorNode.InnerText;
        }
        return calendarColor;
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
        return calendarColor;
      }
    }

    public async Task<bool> SetCalendarColorNoThrow (string calendarColor)
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
                 ", calendarColor)
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
      return GetEntities (range, "VEVENT");
    }

    public Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetTodoVersions (DateTimeRange? range)
    {
      return GetEntities (range, "VTODO");
    }

    public Task<EntityVersion<WebResourceName, string>> CreateEntity (string iCalData, string uid)
    {
      return CreateNewEntity (string.Format ("{0:D}.ics", uid), iCalData);
    }

    protected async Task<EntityVersion<WebResourceName, string>> CreateNewEntity (string name, string content)
    {
      var eventUrl = new Uri (_serverUrl, name);

      s_logger.DebugFormat ("Creating entity '{0}'", eventUrl);

      IHttpHeaders responseHeaders;

      try
      {
        responseHeaders = await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders(
            eventUrl,
            "PUT",
            null,
            null,
            "*",
            "text/calendar",
            content);
      }
      catch (WebException x)
      {
        if (x.Response != null && ((HttpWebResponse)x.Response).StatusCode == HttpStatusCode.Forbidden)
          throw new Exception(string.Format("Error creating event with url '{0}' (Access denied)", eventUrl));
        else
          throw;
      }

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

    public async Task<EntityVersion<WebResourceName, string>> UpdateEntity (WebResourceName url, string etag, string contents)
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
      catch (WebException x)
      {
        if (x.Response != null && ((HttpWebResponse)x.Response).StatusCode == HttpStatusCode.Forbidden)
          throw new Exception (string.Format ("Error updating event with url '{0}' and etag '{1}' (Access denied)", absoluteEventUrl, etag));
        else
          throw;
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

    private async Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetEntities (DateTimeRange? range, string entityType)
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
          if (urlNode != null && etagNode != null)
          {
            var uri = new WebResourceName(urlNode.InnerText);
            entities.Add (EntityVersion.Create (uri, HttpUtility.GetQuotedEtag (etagNode.InnerText)));
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