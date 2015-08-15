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
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml;
using GenSync;
using log4net;

namespace CalDavSynchronizer.DataAccess
{
  public class WebDavDataAccess
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    protected readonly Uri _serverUrl;
    protected readonly IWebDavClient _webDavClient;

    protected WebDavDataAccess (Uri serverUrl, IWebDavClient webDavClient)
    {
      if (serverUrl == null)
        throw new ArgumentNullException ("serverUrl");
      _serverUrl = serverUrl;
      _webDavClient = webDavClient;

      s_logger.DebugFormat ("Created with Url '{0}'", _serverUrl);
    }

    protected bool HasOption (string requiredOption)
    {
      var headers = _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (
          _serverUrl,
          request => { request.Method = "OPTIONS"; },
          null);

      var davHeader = headers["DAV"];

      return davHeader.Split (new[] { ',' })
          .Select (o => o.Trim())
          .Any (option => String.Compare (option, requiredOption, StringComparison.OrdinalIgnoreCase) == 0);
    }

    protected bool IsResourceType (string @namespace, string name)
    {
      var properties = GetAllProperties (_serverUrl, 0);

      XmlNode resourceTypeNode = properties.XmlDocument.SelectSingleNode (
          string.Format ("/D:multistatus/D:response/D:propstat/D:prop/D:resourcetype/{0}:{1}", @namespace, name),
          properties.XmlNamespaceManager);

      return resourceTypeNode != null;
    }

    public bool IsWriteable ()
    {
      var properties = GetCurrentUserPrivileges (_serverUrl, 0);

      XmlNode privilegeWriteContent = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-privilege-set/D:privilege/D:write-content", properties.XmlNamespaceManager);
      XmlNode privilegeBind = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-privilege-set/D:privilege/D:bind", properties.XmlNamespaceManager);
      XmlNode privilegeUnbind = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-privilege-set/D:privilege/D:unbind", properties.XmlNamespaceManager);

      return ((privilegeWriteContent != null) && (privilegeBind != null) && (privilegeUnbind != null));
    }

    private string GetEtag (Uri absoluteEntityUrl)
    {
      var headers = _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (absoluteEntityUrl, delegate { }, null);
      return headers["ETag"];
    }

    private XmlDocumentWithNamespaceManager GetAllProperties (Uri url, int depth)
    {
      return _webDavClient.ExecuteWebDavRequestAndReadResponse (
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

    protected bool DoesSupportsReportSet (Uri url, int depth, string reportSetNamespace, string reportSet)
    {
      var document = _webDavClient.ExecuteWebDavRequestAndReadResponse (
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
                            <D:supported-report-set/>
                          </D:prop>
                        </D:propfind>
                 "
          );

      XmlNode reportSetNode = document.XmlDocument.SelectSingleNode (
          string.Format (
              "/D:multistatus/D:response/D:propstat/D:prop/D:supported-report-set/D:supported-report/D:report/{0}:{1}",
              reportSetNamespace,
              reportSet),
          document.XmlNamespaceManager);

      return reportSetNode != null;
    }

    private XmlDocumentWithNamespaceManager GetCurrentUserPrivileges (Uri url, int depth)
    {
      return _webDavClient.ExecuteWebDavRequestAndReadResponse (
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

    public EntityIdWithVersion<Uri, string> UpdateEntity (EntityIdWithVersion<Uri, string> evt, string contents)
    {
      return UpdateEntity (evt.Id, evt.Version, contents);
    }

    public EntityIdWithVersion<Uri, string> UpdateEntity (Uri url, string contents)
    {
      return UpdateEntity (url, string.Empty, contents);
    }

    private EntityIdWithVersion<Uri, string> UpdateEntity (Uri url, string etag, string contents)
    {
      s_logger.DebugFormat ("Updating entity '{0}'", url);

      var absoluteEventUrl = new Uri (_serverUrl, url);

      s_logger.DebugFormat ("Absolute entity location: '{0}'", absoluteEventUrl);

      WebHeaderCollection responseHeaders;

      try
      {
        responseHeaders = _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (absoluteEventUrl,
            request =>
            {
              request.Method = "PUT";
              request.ContentType = "text/xml; charset=UTF-8";
              if (!string.IsNullOrEmpty (etag))
                request.Headers.Add ("If-Match", etag);
              request.ServicePoint.Expect100Continue = false;
            },
            contents);
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

      var newEtag = responseHeaders["ETag"];
      string version;
      if (newEtag != null)
      {
        version = newEtag;
      }
      else
      {
        version = GetEtag (absoluteEventUrl);
      }

      return new EntityIdWithVersion<Uri, string> (url, version);
    }

    protected EntityIdWithVersion<Uri, string> CreateEntity (string name, string content)
    {
      var eventUrl = new Uri (_serverUrl, name);

      s_logger.DebugFormat ("Creating entity '{0}'", eventUrl);

      WebHeaderCollection responseHeaders;

      try
      {
        responseHeaders = _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (eventUrl,
            request =>
            {
              request.Method = "PUT";
              request.Headers.Add ("If-None-Match", "*");
              request.ContentType = "text/calendar; charset=UTF-8";
              request.ServicePoint.Expect100Continue = false;
            },
            content);
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
        effectiveEventUrl = locationUrl.IsAbsoluteUri ? locationUrl : new Uri (_serverUrl, locationUrl);
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

    public bool DeleteEntity (EntityIdWithVersion<Uri, string> entity)
    {
      return DeleteEntity (entity.Id, entity.Version);
    }

    public bool DeleteEntity (Uri uri)
    {
      return DeleteEntity (uri, string.Empty);
    }

    private bool DeleteEntity (Uri uri, string etag)
    {
      s_logger.DebugFormat ("Deleting entity '{0}'", uri);

      var absoluteEventUrl = new Uri (_serverUrl, uri);

      s_logger.DebugFormat ("Absolute entity location: '{0}'", absoluteEventUrl);

      WebHeaderCollection responseHeaders;

      try
      {
        responseHeaders = _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (absoluteEventUrl,
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
        throw new Exception (string.Format ("Error deleting entity with url '{0}' and etag '{1}': {2}", uri, etag, error));

      return true;
    }

    /// <summary>
    /// Since Uri.Compare() cannot compare relative Uris and ignoring escapes, all Uris have to be unescaped when entering CalDavSynchronizer
    /// </summary>
    protected static class UriHelper
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