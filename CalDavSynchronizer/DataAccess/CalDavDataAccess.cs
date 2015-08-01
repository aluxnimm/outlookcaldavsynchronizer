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
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using GenSync;
using log4net;

namespace CalDavSynchronizer.DataAccess
{
  public class CalDavDataAccess : ICalDavDataAccess
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly Uri _calendarUrl;
    private readonly ICalDavWebClient _calDavWebClient;

    public CalDavDataAccess (Uri calendarUrl, ICalDavWebClient calDavWebClient)
    {
      if (calendarUrl == null)
        throw new ArgumentNullException ("calendarUrl");
      _calendarUrl = calendarUrl;
      _calDavWebClient = calDavWebClient;

      s_logger.DebugFormat ("Created with Url '{0}'", _calendarUrl);
    }

    public bool IsCalendarAccessSupported ()
    {
      var headers = _calDavWebClient.ExecuteCalDavRequestAndReturnResponseHeaders (
          _calendarUrl,
          request =>
          {
            request.Method = "OPTIONS";
          },
          null);

      var davHeader = headers["DAV"];

      return davHeader.Split (new[] { ',' })
          .Select (o => o.Trim())
          .Any (option => String.Compare (option, "calendar-access", StringComparison.OrdinalIgnoreCase) == 0);
    }

    public bool IsResourceCalender ()
    {
      var properties = GetAllProperties (_calendarUrl, 0);

      XmlNode calenderNode = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:resourcetype/C:calendar", properties.XmlNamespaceManager);

      return calenderNode != null;
    }

    public bool IsWriteableCalender ()
    {
      var properties = GetCurrentUserPrivileges (_calendarUrl, 0);

      XmlNode privilegeWriteContent = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-privilege-set/D:privilege/D:write-content", properties.XmlNamespaceManager);
      XmlNode privilegeBind = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-privilege-set/D:privilege/D:bind", properties.XmlNamespaceManager);
      XmlNode privilegeUnbind = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-privilege-set/D:privilege/D:unbind", properties.XmlNamespaceManager);

      return ((privilegeWriteContent != null) && (privilegeBind != null) && (privilegeUnbind != null));
    }

    private const string s_calDavDateTimeFormatString = "yyyyMMddThhmmssZ";

    public IReadOnlyList<EntityIdWithVersion<Uri, string>> GetEvents (DateTime? from, DateTime? to)
    {
      return GetEntities (from, to, "VEVENT");
    }

    public IReadOnlyList<EntityIdWithVersion<Uri, string>> GetTodos (DateTime? from, DateTime? to)
    {
      return GetEntities (from, to, "VTODO");
    }

    private IReadOnlyList<EntityIdWithVersion<Uri, string>> GetEntities (DateTime? from, DateTime? to, string entityType)
    {
      if (from.HasValue != to.HasValue)
        throw new ArgumentException ("Either both or no boundary has to be set");

      var entities = new List<EntityIdWithVersion<Uri, string>>();

      try
      {
        var responseXml = _calDavWebClient.ExecuteCalDavRequestAndReadResponse (
            _calendarUrl,
            request =>
            {
              request.Method = "REPORT";
              request.ContentType = "text/xml; charset=UTF-8";
              request.Headers.Add ("Depth", 1.ToString());
              request.ServicePoint.Expect100Continue = false;
            },
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
                from == null ? string.Empty : string.Format (@"<C:time-range start=""{0}"" end=""{1}""/>",
                    from.Value.ToString (s_calDavDateTimeFormatString),
                    to.Value.ToString (s_calDavDateTimeFormatString))
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
            var uri = UriHelper.UnescapeRelativeUri (_calendarUrl, urlNode.InnerText);
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

    private string GetEtag (Uri absoluteEntityUrl)
    {
      var headers = _calDavWebClient.ExecuteCalDavRequestAndReturnResponseHeaders (absoluteEntityUrl, delegate { }, null);
      return headers["ETag"];
    }


    private XmlDocumentWithNamespaceManager GetAllProperties (Uri url, int depth)
    {
      return _calDavWebClient.ExecuteCalDavRequestAndReadResponse (
          url,
          request =>
          {
            request.Method = "PROPFIND";
            request.ContentType = "text/xml; charset=UTF-8";
            request.ServicePoint.Expect100Continue = false;
            request.Headers["Depth"] = depth.ToString();
          },
          @"<?xml version='1.0'?>
                        <D:propfind xmlns:D=""DAV:"">
                            <D:allprop/>
                        </D:propfind>
                 "
          );
    }

    private XmlDocumentWithNamespaceManager GetCurrentUserPrivileges (Uri url, int depth)
    {
      return _calDavWebClient.ExecuteCalDavRequestAndReadResponse (
          url,
          request =>
          {
            request.Method = "PROPFIND";
            request.ContentType = "text/xml; charset=UTF-8";
            request.ServicePoint.Expect100Continue = false;
            request.Headers["Depth"] = depth.ToString();
          },
          @"<?xml version='1.0'?>
                        <D:propfind xmlns:D=""DAV:"">
                          <D:prop>
                            <D:current-user-privilege-set/>
                          </D:prop>
                        </D:propfind>
                 "
          );
    }

    public EntityIdWithVersion<Uri, string> UpdateEntity (EntityIdWithVersion<Uri, string> evt, string iCalData)
    {
      return UpdateEntity (evt.Id, evt.Version, iCalData);
    }

    public EntityIdWithVersion<Uri, string> UpdateEntity (Uri url, string iCalData)
    {
      return UpdateEntity (url, string.Empty, iCalData);
    }

    private EntityIdWithVersion<Uri, string> UpdateEntity (Uri url, string etag, string iCalData)
    {
      s_logger.DebugFormat ("Updating entity '{0}'", url);

      var absoluteEventUrl = new Uri (_calendarUrl, url);

      s_logger.DebugFormat ("Absolute entity location: '{0}'", absoluteEventUrl);

      WebHeaderCollection responseHeaders;

      try
      {
        responseHeaders = _calDavWebClient.ExecuteCalDavRequestAndReturnResponseHeaders (absoluteEventUrl,
            request =>
            {
              request.Method = "PUT";
              request.ContentType = "text/xml; charset=UTF-8";
              if (!string.IsNullOrEmpty (etag))
                request.Headers.Add ("If-Match", etag);
              request.ServicePoint.Expect100Continue = false;
            },
            iCalData);
      }
      catch (WebException x)
      {
        if (x.Response != null && ((HttpWebResponse) x.Response).StatusCode == HttpStatusCode.Forbidden)
          throw new Exception (string.Format ("Error updating event with url '{0}' and etag '{1}' (Access denied)", absoluteEventUrl, etag));
        else
          throw;
      }

      if (s_logger.IsDebugEnabled)
        s_logger.DebugFormat ("Updated entity. Server response header: '{0}'", responseHeaders.ToString().Replace ("\r\n", " <CR> "));

      return new EntityIdWithVersion<Uri, string> (url, responseHeaders["ETag"]);
    }

    public EntityIdWithVersion<Uri, string> CreateEntity (string iCalData)
    {
      var eventUrl = new Uri (_calendarUrl, string.Format ("{0:D}.ics", Guid.NewGuid()));

      s_logger.DebugFormat ("Creating entity '{0}'", eventUrl);

      WebHeaderCollection responseHeaders;

      try
      {
        responseHeaders = _calDavWebClient.ExecuteCalDavRequestAndReturnResponseHeaders (eventUrl,
            request =>
            {
              request.Method = "PUT";
              request.Headers.Add ("If-None-Match", "*");
              request.ContentType = "text/calendar; charset=UTF-8";
              request.ServicePoint.Expect100Continue = false;
            },
            iCalData);
      }
      catch (WebException x)
      {
        if (x.Response != null && ((HttpWebResponse) x.Response).StatusCode == HttpStatusCode.Forbidden)
          throw new Exception (string.Format ("Error creating event with url '{0}' (Access denied)", eventUrl));
        else
          throw;
      }

      Uri effectiveEventUrl;
      var location = responseHeaders["location"];
      if (!string.IsNullOrEmpty (location))
      {
        s_logger.DebugFormat ("Server sent new location: '{0}'", location);
        var locationUrl = new Uri (location, UriKind.RelativeOrAbsolute);
        effectiveEventUrl = locationUrl.IsAbsoluteUri ? locationUrl : new Uri (_calendarUrl, locationUrl);
        s_logger.DebugFormat ("New entity location: '{0}'", effectiveEventUrl);
      }
      else
      {
        effectiveEventUrl = eventUrl;
      }

      var etag = responseHeaders["ETag"];
      string version;
      if (etag != null)
      {
        version = etag;
      }
      else
      {
        version = GetEtag (effectiveEventUrl);
      }

      return new EntityIdWithVersion<Uri, string> (UriHelper.GetUnescapedPath (effectiveEventUrl), version);
    }

    public bool DeleteEntity (EntityIdWithVersion<Uri, string> evt)
    {
      return DeleteEntity (evt.Id, evt.Version);
    }

    public bool DeleteEntity (Uri uri)
    {
      return DeleteEntity (uri, string.Empty);
    }

    private bool DeleteEntity (Uri uri, string etag)
    {
      s_logger.DebugFormat ("Deleting entity '{0}'", uri);

      var absoluteEventUrl = new Uri (_calendarUrl, uri);

      s_logger.DebugFormat ("Absolute entity location: '{0}'", absoluteEventUrl);

      WebHeaderCollection responseHeaders;

      try
      {
        responseHeaders = _calDavWebClient.ExecuteCalDavRequestAndReturnResponseHeaders (absoluteEventUrl,
            request =>
            {
              request.Method = "DELETE";
              if (!string.IsNullOrEmpty (etag))
                request.Headers.Add ("If-Match", etag);
              request.ServicePoint.Expect100Continue = false;
            },
            string.Empty);
      }
      catch (WebException x)
      {
        if (x.Response != null)
        {
          var httpWebResponse = (HttpWebResponse) x.Response;

          if (httpWebResponse.StatusCode == HttpStatusCode.NotFound)
            return false;

          if (httpWebResponse.StatusCode == HttpStatusCode.Forbidden)
            throw new Exception (string.Format ("Error deleting event with url '{0}' and etag '{1}' (Access denied)", uri, etag));
        }

        throw;
      }

      var error = responseHeaders["X-Dav-Error"];
      if (error != null && error != "200 No error")
        throw new Exception (string.Format ("Error deleting event with url '{0}' and etag '{1}': {2}", uri, etag, error));

      return true;
    }

    public IReadOnlyList<EntityWithVersion<Uri, string>> GetEntities (IEnumerable<Uri> eventUrls)
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

      var responseXml = _calDavWebClient.ExecuteCalDavRequestAndReadResponse (
          _calendarUrl,
          request =>
          {
            request.Method = "REPORT";
            request.ContentType = "text/xml; charset=UTF-8";
            request.Headers.Add ("Depth", "1");
            request.ServicePoint.Expect100Continue = false;
            request.Headers.Add (HttpRequestHeader.AcceptCharset, "utf-8");
          },
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
          entities.Add (EntityWithVersion.Create (UriHelper.UnescapeRelativeUri (_calendarUrl, urlNode.InnerText), dataNode.InnerText));
        }
      }

      return entities;
    }

    /// <summary>
    /// Since Uri.Compare() cannot compare relative Uris and ignoring escapes, all Uris have to be unescaped when entering CalDavSynchronizer
    /// </summary>
    private static class UriHelper
    {
      public static Uri GetUnescapedPath (Uri absoluteUri)
      {
        return new Uri (absoluteUri.GetComponents (UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.Unescaped), UriKind.Relative);
      }

      public static Uri UnescapeRelativeUri (Uri baseUri, string relativeUriString)
      {
        var relativeUri = new Uri (relativeUriString, UriKind.Relative);
        var aboluteUri = new Uri (baseUri, relativeUri);
        var unescapedRelativeUri = new Uri (aboluteUri.GetComponents (UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.Unescaped), UriKind.Relative);
        return unescapedRelativeUri;
      }
    }

  }
}