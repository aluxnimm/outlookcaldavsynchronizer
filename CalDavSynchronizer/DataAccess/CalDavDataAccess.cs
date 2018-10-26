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
using CalDavSynchronizer.Ui.ConnectionTests;
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

    public async Task<Uri> GetCalendarHomeSetUriOrNull (bool useWellKnownUrl)
    {
      var autodiscoveryUrl = useWellKnownUrl ? AutoDiscoveryUrl : _serverUrl;

      var currentUserPrincipalUrl = await GetCurrentUserPrincipalUrl (autodiscoveryUrl);
      if (currentUserPrincipalUrl != null)
      {
        var calendarHomeSetProperties = await GetCalendarHomeSet (currentUserPrincipalUrl);

        XmlNode homeSetNode = calendarHomeSetProperties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/C:calendar-home-set", calendarHomeSetProperties.XmlNamespaceManager);
        if (homeSetNode != null && homeSetNode.HasChildNodes)
        {
          foreach (XmlNode homeSetNodeHref in homeSetNode.ChildNodes)
          {
            if (!string.IsNullOrEmpty (homeSetNodeHref.InnerText))
            {
              var calendarHomeSetUri = Uri.IsWellFormedUriString (homeSetNodeHref.InnerText, UriKind.Absolute)
                ? new Uri(homeSetNodeHref.InnerText)
                : new Uri(calendarHomeSetProperties.DocumentUri.GetLeftPart (UriPartial.Authority) +
                          homeSetNodeHref.InnerText);
              return calendarHomeSetUri;
            }
          }
          
        }
      }
      return null;
    }

    public async Task<Uri> AddResource (string name, bool useRandomUri)
    {
      var homeSetUri = await GetCalendarHomeSetUriOrNull (ConnectionTester.RequiresAutoDiscovery (_serverUrl)) ?? _serverUrl;
      var resourceName = useRandomUri ? Guid.NewGuid() + "/" : name + "/";

      var newResourceUri = new Uri (homeSetUri, resourceName);
      await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (
        newResourceUri,
        "MKCOL",
        null,
        null,
        null,
        "application/xml",
          @"<?xml version='1.0'?>
                    <D:mkcol xmlns:D=""DAV:"" xmlns:C=""urn:ietf:params:xml:ns:caldav"">
                      <D:set>
                        <D:prop>
                          <D:resourcetype>
                            <D:collection/>
                            <C:calendar/>
                          </D:resourcetype>
                        </D:prop>
                      </D:set>
                    </D:mkcol>"
        );

      await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (
        newResourceUri,
        "PROPPATCH",
        0,
        null,
        null,
        "application/xml",
        string.Format (
           @"<?xml version='1.0'?>
                      <D:propertyupdate xmlns:D=""DAV:"">
                        <D:set>
                          <D:prop>
                              <D:displayname>{0}</D:displayname>
                          </D:prop>
                        </D:set>
                      </D:propertyupdate>
                 ", name)
        );
      return newResourceUri;
    }

    public async Task<CalDavResources> GetUserResourcesNoThrow (bool useWellKnownUrl)
    {
      try
      {
        var autodiscoveryUrl = useWellKnownUrl ? AutoDiscoveryUrl : _serverUrl;

        var currentUserPrincipalUrl = await GetCurrentUserPrincipalUrl (autodiscoveryUrl);

        var calendars = new List<CalendarData>();
        var taskLists = new List<TaskListData>();

        if (currentUserPrincipalUrl != null)
        {
          var calendarHomeSetProperties = await GetCalendarHomeSet (currentUserPrincipalUrl);

          XmlNode homeSetNode = calendarHomeSetProperties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/C:calendar-home-set", calendarHomeSetProperties.XmlNamespaceManager);
          if (homeSetNode != null && homeSetNode.HasChildNodes)
          {
            // https://tools.ietf.org/html/rfc6638#section-9.2
            XmlNode scheduleDefaultCalendarNode = calendarHomeSetProperties.XmlDocument.SelectSingleNode("/D:multistatus/D:response/D:propstat/D:prop/C:schedule-default-calendar-URL", calendarHomeSetProperties.XmlNamespaceManager);
            String scheduleDefaultCalendarPath = scheduleDefaultCalendarNode == null ? "" :
              scheduleDefaultCalendarNode.InnerText.EndsWith("/") ? scheduleDefaultCalendarNode.InnerText : scheduleDefaultCalendarNode.InnerText + "/";
            bool isFirstCalendar = true;
            foreach (XmlNode homeSetNodeHref in homeSetNode.ChildNodes)
            {  
              if (!string.IsNullOrEmpty (homeSetNodeHref.InnerText))
              {
                var calendarHomeSetUri = Uri.IsWellFormedUriString (homeSetNodeHref.InnerText, UriKind.Absolute) ?
                  new Uri (homeSetNodeHref.InnerText) :
                  new Uri (calendarHomeSetProperties.DocumentUri.GetLeftPart (UriPartial.Authority) + homeSetNodeHref.InnerText);

                var calendarDocument = await ListCalendars (calendarHomeSetUri);

                XmlNodeList responseNodes = calendarDocument.XmlDocument.SelectNodes ("/D:multistatus/D:response",calendarDocument.XmlNamespaceManager);

                foreach (XmlElement responseElement in responseNodes)
                {
                  var urlNode = responseElement.SelectSingleNode ("D:href", calendarDocument.XmlNamespaceManager);
                  var displayNameNode = responseElement.SelectSingleNode ("D:propstat/D:prop/D:displayname", calendarDocument.XmlNamespaceManager);
                  if (urlNode != null && displayNameNode != null)
                  {
                    XmlNode isCollection = responseElement.SelectSingleNode ("D:propstat/D:prop/D:resourcetype/C:calendar", calendarDocument.XmlNamespaceManager);
                    if (isCollection != null)
                    {
                      var calendarColorNode = responseElement.SelectSingleNode ("D:propstat/D:prop/E:calendar-color", calendarDocument.XmlNamespaceManager);
                      ArgbColor? calendarColor = null;
                      if (calendarColorNode != null && calendarColorNode.InnerText.Length >= 7)
                      {
                        calendarColor = ArgbColor.FromRgbaHexStringWithOptionalANoThrow (calendarColorNode.InnerText);
                      }

                      XmlNode supportedComponentsNode = responseElement.SelectSingleNode ("D:propstat/D:prop/C:supported-calendar-component-set", calendarDocument.XmlNamespaceManager);
                      if (supportedComponentsNode != null)
                      {
                        var path = urlNode.InnerText.EndsWith ("/") ? urlNode.InnerText : urlNode.InnerText + "/";
                        var uri = new Uri(calendarDocument.DocumentUri, path);
                        bool ro = await IsReadOnly(uri);

                        if (supportedComponentsNode.InnerXml.Contains ("VEVENT"))
                        {
                          bool isDefault = (scheduleDefaultCalendarNode != null && scheduleDefaultCalendarPath == path || scheduleDefaultCalendarNode == null && isFirstCalendar);
                          isFirstCalendar = false;
                          var displayName = string.IsNullOrEmpty (displayNameNode.InnerText) ? "Default Calendar" : displayNameNode.InnerText;
                          calendars.Add (new CalendarData (uri, displayName, calendarColor, ro, isDefault));
                        }
                        if (supportedComponentsNode.InnerXml.Contains ("VTODO"))
                        {
                          var displayName = string.IsNullOrEmpty (displayNameNode.InnerText) ? "Default Tasks" : displayNameNode.InnerText;
                          taskLists.Add (new TaskListData (uri.ToString(), displayName, ro));
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
        return new CalDavResources (calendars, taskLists);
      }
      catch (Exception x)
      {
        if (x.Message.Contains ("404") || x.Message.Contains ("405") || x is XmlException)
          return new CalDavResources (new CalendarData[] { }, new TaskListData[] {});
        else
          throw;
      }
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
                            <C:schedule-default-calendar-URL/>
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
      if (responseHeaders.LocationOrNull != null)
      {
        s_logger.DebugFormat ("Server sent new location: '{0}'", responseHeaders.LocationOrNull);
        effectiveEventUrl = responseHeaders.LocationOrNull.IsAbsoluteUri ? responseHeaders.LocationOrNull : new Uri (_serverUrl, responseHeaders.LocationOrNull);
        s_logger.DebugFormat ("New entity location: '{0}'", effectiveEventUrl);
      }
      else
      {
        effectiveEventUrl = eventUrl;
      }

      var etag = responseHeaders.ETagOrNull;
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
      if (responseHeaders.LocationOrNull != null)
      {
        s_logger.DebugFormat ("Server sent new location: '{0}'", responseHeaders.LocationOrNull);
        effectiveEventUrl = responseHeaders.LocationOrNull.IsAbsoluteUri ? responseHeaders.LocationOrNull : new Uri(_serverUrl, responseHeaders.LocationOrNull);
        s_logger.DebugFormat ("New entity location: '{0}'", effectiveEventUrl);
      }
      else
      {
        effectiveEventUrl = absoluteEventUrl;
      }

      var newEtag = responseHeaders.ETagOrNull;
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


        return ExtractVersions(responseXml);
      }
      catch (WebDavClientException x)
      {
        // Workaround for Synology NAS, which returns 404 insteaod of an empty response if no events are present
        if (x.StatusCode == HttpStatusCode.NotFound && await IsResourceCalender())
          return new EntityVersion<WebResourceName, string>[0]; ;

        throw;
      }
    }

    public async Task<IReadOnlyList<EntityWithId<WebResourceName, string>>> GetEntities (IEnumerable<WebResourceName> eventUrls)
    {
      s_logger.Debug("Entered GetEntities.");

      WebResourceName firstResourceNameOrNull = null;

      var requestBody = @"<?xml version=""1.0""?>
			                    <C:calendar-multiget xmlns:C=""urn:ietf:params:xml:ns:caldav"" xmlns:D=""DAV:"">
			                        <D:prop>
			                            <D:getetag/>
			                            <D:displayname/>
			                            <C:calendar-data/>
			                        </D:prop>
                                        " + 
                                        String.Join (
                                          "\r\n", 
                                          eventUrls.Select (
                                             u => 
                                             {
                                               if (s_logger.IsDebugEnabled)
                                                 s_logger.Debug($"Requesting: '{u}'");
                                               if (firstResourceNameOrNull == null)
                                                 firstResourceNameOrNull = u;
                                               return $"<D:href>{SecurityElement.Escape(u.OriginalAbsolutePath)}</D:href>";
                                             })) + @"
                                    </C:calendar-multiget>";

      var responseXml = await _webDavClient.ExecuteWebDavRequestAndReadResponse (
          UriHelper.AlignServerUrl (_serverUrl, firstResourceNameOrNull),
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
        if (urlNode != null && !string.IsNullOrEmpty (dataNode?.InnerText))
        {
          if (s_logger.IsDebugEnabled)
            s_logger.DebugFormat($"Got: '{urlNode.InnerText}'");
          entities.Add (EntityWithId.Create (new WebResourceName(urlNode.InnerText), dataNode.InnerText));
        }
      }

      s_logger.Debug("Exiting GetEntities.");
      return entities;
    }

    public async Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetVersions (IEnumerable<WebResourceName> eventUrls)
    {
      WebResourceName firstResourceNameOrNull = null;

      var requestBody = @"<?xml version=""1.0""?>
			                    <C:calendar-multiget xmlns:C=""urn:ietf:params:xml:ns:caldav"" xmlns:D=""DAV:"">
			                        <D:prop>
			                            <D:getetag/>
			                        </D:prop>
                                        " +
                        String.Join(
                          "\r\n",
                          eventUrls.Select(u =>
                          {
                            if (firstResourceNameOrNull == null)
                              firstResourceNameOrNull = u;
                            return $"<D:href>{SecurityElement.Escape(u.OriginalAbsolutePath)}</D:href>";
                          })) + @"
                                    </C:calendar-multiget>";

      var responseXml = await _webDavClient.ExecuteWebDavRequestAndReadResponse (
          UriHelper.AlignServerUrl (_serverUrl, firstResourceNameOrNull),
          "REPORT",
          1,
          null,
          null,
          "application/xml",
          requestBody
          );

      return ExtractVersions(responseXml);
    }


    private IReadOnlyList<EntityVersion<WebResourceName, string>> ExtractVersions(XmlDocumentWithNamespaceManager responseXml)
    {
      var responseNodes = responseXml.XmlDocument.SelectNodes("/D:multistatus/D:response", responseXml.XmlNamespaceManager);

      if (responseNodes == null)
        return new EntityVersion<WebResourceName, string>[0];

      var entities = new List<EntityVersion<WebResourceName, string>>();

      // ReSharper disable once LoopCanBeConvertedToQuery
      // ReSharper disable once PossibleNullReferenceException
      foreach (XmlElement responseElement in responseNodes)
      {
        var urlNode = responseElement.SelectSingleNode("D:href", responseXml.XmlNamespaceManager);
        var etagNode = responseElement.SelectSingleNode("D:propstat/D:prop/D:getetag", responseXml.XmlNamespaceManager);
        if (urlNode != null &&
            etagNode != null &&
            _serverUrl.AbsolutePath != UriHelper.DecodeUrlString(urlNode.InnerText))
        {
          var uri = new WebResourceName(urlNode.InnerText);
          var etag = HttpUtility.GetQuotedEtag(etagNode.InnerText);
          if (s_logger.IsDebugEnabled)
            s_logger.DebugFormat($"Got version '{uri}': '{etag}'");
          entities.Add(EntityVersion.Create(uri, etag));
        }
      }

      return entities;
    }
  }
}