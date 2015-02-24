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
using System.Text;
using System.Xml;
using CalDavSynchronizer.EntityVersionManagement;

namespace CalDavSynchronizer.DataAccess
{
  public class CalDavDataAccess : ICalDavDataAccess
  {
    private readonly Uri _calendarUrl;
    private readonly string _username;
    // TODO: consider to use SecureString
    private readonly string _password;

    public CalDavDataAccess (Uri calendarUrl, string username, string password)
    {
      if (calendarUrl == null)
        throw new ArgumentNullException ("calendarUrl");
      _username = username;
      _password = password;
      _calendarUrl = calendarUrl;
    }


    private HttpWebRequest CreateRequest (Uri url)
    {
      var request = (HttpWebRequest) HttpWebRequest.Create (url);

      if (!string.IsNullOrEmpty (_username))
      {
        request.PreAuthenticate = true;
        request.Credentials = new NetworkCredential (_username, _password);
      }

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

    private const string s_calDavDateTimeFormatString = "yyyyMMddThhmmssZ";

    public IEnumerable<EntityIdWithVersion<Uri, string>> GetEvents (DateTime? from, DateTime? to)
    {
      if (from.HasValue != to.HasValue)
        throw new ArgumentException ("Either both or no boundary has to be set");

      var responseXml = ExecuteCalDavRequestAndReadResponse (
          _calendarUrl,
          request =>
          {
            request.Method = "REPORT";
            request.ContentType = "text/xml";
            request.Headers.Add ("Depth", 1.ToString());
            request.ServicePoint.Expect100Continue = false;
          },
          string.Format (
              @"<?xml version=""1.0""?>
                    <C:calendar-query xmlns:C=""urn:ietf:params:xml:ns:caldav"">
                        <D:prop xmlns:D=""DAV:"">
                            <D:getetag/>
                            <D:displayname/>
                        </D:prop>
                        <C:filter>
                            <C:comp-filter name=""VCALENDAR"">
                                <C:comp-filter name=""VEVENT"">
                                  {0}
                                </C:comp-filter>
                            </C:comp-filter>
                        </C:filter>
                    </C:calendar-query>
                    ",
              from == null ? string.Empty : string.Format (@"<C:time-range start=""{0}"" end=""{1}""/>",
                  from.Value.ToString (s_calDavDateTimeFormatString),
                  to.Value.ToString (s_calDavDateTimeFormatString))
              ));


      XmlNodeList responseNodes = responseXml.XmlDocument.SelectNodes ("/D:multistatus/D:response", responseXml.XmlNamespaceManager);


      var entities = new List<EntityIdWithVersion<Uri, string>>();

      // ReSharper disable once LoopCanBeConvertedToQuery
      // ReSharper disable once PossibleNullReferenceException
      foreach (XmlElement responseElement in responseNodes)
      {
        var urlNode = responseElement.SelectSingleNode ("D:href", responseXml.XmlNamespaceManager);
        var etagNode = responseElement.SelectSingleNode ("D:propstat/D:prop/D:getetag", responseXml.XmlNamespaceManager);
        if (urlNode != null && etagNode != null)
          entities.Add (new EntityIdWithVersion<Uri, string> (new Uri (urlNode.InnerText, UriKind.Relative), etagNode.InnerText));
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
            request.ContentType = "text/xml";
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

    public EntityIdWithVersion<Uri, string> UpdateEvent (EntityIdWithVersion<Uri, string> evt, string iCalData)
    {
      return UpdateEvent (evt.Id, evt.Version, iCalData);
    }

    public EntityIdWithVersion<Uri, string> UpdateEvent (Uri url, string iCalData)
    {
      return UpdateEvent (url, string.Empty, iCalData);
    }

    private EntityIdWithVersion<Uri, string> UpdateEvent (Uri url, string etag, string iCalData)
    {
      var absoluteEventUrl = new Uri (_calendarUrl, url);

      var response = ExecuteCalDavRequest (absoluteEventUrl,
          request =>
          {
            request.Method = "PUT";
            request.ContentType = "text/xml";
            if (!string.IsNullOrEmpty (etag))
              request.Headers.Add ("If-Match", etag);
            request.ServicePoint.Expect100Continue = false;
          },
          iCalData);

      return new EntityIdWithVersion<Uri, string> (url, response.Headers["ETag"]);
    }

    public EntityIdWithVersion<Uri, string> CreateEvent (string iCalData)
    {
      var eventUrl = new Uri (_calendarUrl, string.Format ("{0:D}.ics", Guid.NewGuid()));

      var response = ExecuteCalDavRequest (eventUrl,
          request =>
          {
            request.Method = "PUT";
            request.Headers.Add ("If-None-Match", "*");
            request.ContentType = "text/calendar";
            request.ServicePoint.Expect100Continue = false;
          },
          iCalData);

      return new EntityIdWithVersion<Uri, string> (new Uri (eventUrl.AbsolutePath, UriKind.Relative), response.Headers["ETag"]);
    }

    public bool DeleteEvent (EntityIdWithVersion<Uri, string> evt)
    {
      return DeleteEvent (evt.Id, evt.Version);
    }

    public bool DeleteEvent (Uri uri)
    {
      return DeleteEvent (uri, string.Empty);
    }

    private bool DeleteEvent (Uri uri, string etag)
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
        else
          throw;
      }

      var error = response.Headers["X-Dav-Error"];
      if (error != "200 No error")
        throw new Exception (string.Format ("Error deleting event with url '{0}' and etag '{1}': {2}", uri, etag, error));

      return true;
    }


    public Dictionary<Uri, string> GetEvents (IEnumerable<Uri> eventUrls)
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
            request.ContentType = "text/xml";
            request.Headers.Add ("Depth", "1");
            request.ServicePoint.Expect100Continue = false;
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
        var requestBodyAsBytes = Encoding.ASCII.GetBytes (requestBody);

        using (var requestStream = request.GetRequestStream())
        {
          requestStream.Write (requestBodyAsBytes, 0, requestBodyAsBytes.Length);
        }
      }

      return request.GetResponse();
    }

    private static XmlDocumentWithNamespaceManager CreateCalDavXmlDocument (Stream calDavXmlStream)
    {
      XmlDocument responseBody = new XmlDocument();
      responseBody.Load (calDavXmlStream);

      XmlNamespaceManager namespaceManager = new XmlNamespaceManager (responseBody.NameTable);
      //currNsmgr.AddNamespace(String.Empty, "urn:ietf:params:xml:ns:caldav");
      namespaceManager.AddNamespace ("C", "urn:ietf:params:xml:ns:caldav");
      namespaceManager.AddNamespace ("D", "DAV:");

      return new XmlDocumentWithNamespaceManager (responseBody, namespaceManager);
    }
  }
}