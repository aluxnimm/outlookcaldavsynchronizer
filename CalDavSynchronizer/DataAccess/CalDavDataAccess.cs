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
using CalDavSynchronizer.Generic.EntityVersionManagement;
using log4net;

namespace CalDavSynchronizer.DataAccess
{
  public class CalDavDataAccess : ICalDavDataAccess
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly Uri _calendarUrl;
    private readonly string _username;
    // TODO: consider to use SecureString
    private readonly string _password;
    private readonly TimeSpan _connectTimeout;
    private readonly TimeSpan _readWriteTimeout;

    public CalDavDataAccess (Uri calendarUrl, string username, string password, TimeSpan connectTimeout, TimeSpan readWriteTimeout)
    {
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

      if (calendarUrl == null)
        throw new ArgumentNullException ("calendarUrl");
      _username = username;
      _password = password;
      _connectTimeout = connectTimeout;
      _readWriteTimeout = readWriteTimeout;
      _calendarUrl = calendarUrl;
    }


    private HttpWebRequest CreateRequest (Uri url)
    {
      var request = (HttpWebRequest) HttpWebRequest.Create (url);
      request.Timeout = (int) _connectTimeout.TotalMilliseconds;
      request.ReadWriteTimeout = (int) _readWriteTimeout.TotalMilliseconds;

      if (!string.IsNullOrEmpty (_username))
      {
        request.PreAuthenticate = true;
        request.Credentials = new NetworkCredential (_username, _password);
      }
      request.AllowAutoRedirect = false;
      return request;
    }

    public bool IsCalendarAccessSupported ()
    {
      var request = CreateRequest (_calendarUrl);
      request.Method = "OPTIONS";
      // FIXME: Redirects don't authenticate.
      var response = request.GetResponse();


      string davHeader = response.Headers["DAV"].ToString();

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


    public Dictionary<Uri, string> GetEvents (DateTime? from, DateTime? to)
    {
      return GetEntities (from, to, "VEVENT");
    }

    public Dictionary<Uri, string> GetTodos (DateTime? from, DateTime? to)
    {
      return GetEntities (from, to, "VTODO");
    }


    private Dictionary<Uri, string> GetEntities (DateTime? from, DateTime? to, string entityType)
    {
      if (from.HasValue != to.HasValue)
        throw new ArgumentException ("Either both or no boundary has to be set");

      var responseXml = ExecuteCalDavRequestAndReadResponse (
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


      var entities = new Dictionary<Uri, string>();

      // ReSharper disable once LoopCanBeConvertedToQuery
      // ReSharper disable once PossibleNullReferenceException
      foreach (XmlElement responseElement in responseNodes)
      {
        var urlNode = responseElement.SelectSingleNode ("D:href", responseXml.XmlNamespaceManager);
        var etagNode = responseElement.SelectSingleNode ("D:propstat/D:prop/D:getetag", responseXml.XmlNamespaceManager);
        if (urlNode != null && etagNode != null)
        {
          var uri = new Uri (urlNode.InnerText, UriKind.Relative);
          if (!entities.ContainsKey (uri))
            entities.Add (uri, etagNode.InnerText);
          else
            s_logger.WarnFormat ("Entitiy '{0}' was contained multiple times in server response. Ignoring redundant entity", uri);
        }
      }

      return entities;
    }

    private XmlDocumentWithNamespaceManager GetAllProperties (Uri url, int depth)
    {
      return ExecuteCalDavRequestAndReadResponse (
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
      return ExecuteCalDavRequestAndReadResponse (
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
      var absoluteEventUrl = new Uri (_calendarUrl, url);

      WebResponse response;

      try
      {
        response = ExecuteCalDavRequest (absoluteEventUrl,
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
        if (((HttpWebResponse) x.Response).StatusCode == HttpStatusCode.Forbidden)
          throw new Exception (string.Format ("Error updating event with url '{0}' and etag '{1}' (Access denied)", absoluteEventUrl, etag));
        else
          throw;
      }

      return new EntityIdWithVersion<Uri, string> (url, response.Headers["ETag"]);
    }

    public EntityIdWithVersion<Uri, string> CreateEntity (string iCalData)
    {
      var eventUrl = new Uri (_calendarUrl, string.Format ("{0:D}.ics", Guid.NewGuid()));

      WebResponse response;

      try
      {
        response = ExecuteCalDavRequest (eventUrl,
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
        if (((HttpWebResponse) x.Response).StatusCode == HttpStatusCode.Forbidden)
          throw new Exception (string.Format ("Error creating event with url '{0}' (Access denied)", eventUrl));
        else
          throw;
      }

      Uri effectiveEventUrl;
      var location = response.Headers["location"];
      if (!string.IsNullOrEmpty (location))
        effectiveEventUrl = new Uri (location);
      else
        effectiveEventUrl = eventUrl;

      return new EntityIdWithVersion<Uri, string> (new Uri (effectiveEventUrl.AbsolutePath, UriKind.Relative), response.Headers["ETag"]);
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
      var absoluteEventUrl = new Uri (_calendarUrl, uri);

      WebResponse response;

      try
      {
        response = ExecuteCalDavRequest (absoluteEventUrl,
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
        if (((HttpWebResponse) x.Response).StatusCode == HttpStatusCode.NotFound)
          return false;
        else if (((HttpWebResponse) x.Response).StatusCode == HttpStatusCode.Forbidden)
          throw new Exception (string.Format ("Error deleting event with url '{0}' and etag '{1}' (Access denied)", uri, etag));
        else
          throw;
      }

      var error = response.Headers["X-Dav-Error"];
      if (error != null && error != "200 No error")
        throw new Exception (string.Format ("Error deleting event with url '{0}' and etag '{1}': {2}", uri, etag, error));

      return true;
    }


    public Dictionary<Uri, string> GetEntities (IEnumerable<Uri> eventUrls)
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

      var responseXml = ExecuteCalDavRequestAndReadResponse (
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

      var entities = new Dictionary<Uri, string>();

      if (responseNodes == null)
        return entities;


      // ReSharper disable once LoopCanBeConvertedToQuery
      // ReSharper disable once PossibleNullReferenceException
      foreach (XmlElement responseElement in responseNodes)
      {
        var urlNode = responseElement.SelectSingleNode ("D:href", responseXml.XmlNamespaceManager);
        var dataNode = responseElement.SelectSingleNode ("D:propstat/D:prop/C:calendar-data", responseXml.XmlNamespaceManager);
        if (urlNode != null && dataNode != null)
          entities.Add (new Uri (urlNode.InnerText, UriKind.Relative), dataNode.InnerText);
      }

      return entities;
    }

    private XmlDocumentWithNamespaceManager ExecuteCalDavRequestAndReadResponse (Uri url, Action<HttpWebRequest> modifier, string requestBody)
    {
      var response = ExecuteCalDavRequest (url, modifier, requestBody);
      return CreateCalDavXmlDocument (response.GetResponseStream());
    }

    private WebResponse ExecuteCalDavRequest (Uri url, Action<HttpWebRequest> modifier, string requestBody)
    {
      var request = CreateRequest (url);
      modifier (request);
      if (requestBody != string.Empty)
      {
        var requestBodyAsBytes = Encoding.UTF8.GetBytes (requestBody);

        using (var requestStream = request.GetRequestStream())
        {
          requestStream.Write (requestBodyAsBytes, 0, requestBodyAsBytes.Length);
        }
      }

      WebResponse response = request.GetResponse();
      if (((HttpWebResponse) response).StatusCode == HttpStatusCode.Moved || ((HttpWebResponse) response).StatusCode == HttpStatusCode.Redirect)
      {
        if (!string.IsNullOrEmpty (response.Headers["Location"]))
        {
          return ExecuteCalDavRequest (new Uri (response.Headers["Location"]), modifier, requestBody);
        }
        else
        {
          s_logger.Warn ("Ignoring Redirection without Location header.");
        }
      }
      return response;
    }

    private static XmlDocumentWithNamespaceManager CreateCalDavXmlDocument (Stream calDavXmlStream)
    {
      using (var reader = new StreamReader (calDavXmlStream, Encoding.UTF8))
      {
        XmlDocument responseBody = new XmlDocument();


        responseBody.Load (reader);

        XmlNamespaceManager namespaceManager = new XmlNamespaceManager (responseBody.NameTable);
        //currNsmgr.AddNamespace(String.Empty, "urn:ietf:params:xml:ns:caldav");
        namespaceManager.AddNamespace ("C", "urn:ietf:params:xml:ns:caldav");
        namespaceManager.AddNamespace ("D", "DAV:");

        return new XmlDocumentWithNamespaceManager (responseBody, namespaceManager);
      }
    }
  }
}