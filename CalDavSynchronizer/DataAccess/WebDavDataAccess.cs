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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
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

    protected async Task<bool> HasOption (string requiredOption)
    {
      var headers = await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (
          _serverUrl,
          "OPTIONS",
          null,
          null,
          null,
          null,
          null);

      IEnumerable<string> davValues;
      if (headers.TryGetValues ("DAV", out davValues))
      {
        return davValues.Any (
            value => value.Split (new[] { ',' })
                .Select (o => o.Trim())
                .Any (option => String.Compare (option, requiredOption, StringComparison.OrdinalIgnoreCase) == 0));
      }
      else
      {
        return false;
      }
    }

    protected async Task<bool> IsResourceType (string @namespace, string name)
    {
      var properties = await GetAllProperties (_serverUrl, 0);

      XmlNode resourceTypeNode = properties.XmlDocument.SelectSingleNode (
          string.Format ("/D:multistatus/D:response/D:propstat/D:prop/D:resourcetype/{0}:{1}", @namespace, name),
          properties.XmlNamespaceManager);

      return resourceTypeNode != null;
    }

    public async Task<bool> IsWriteable ()
    {
      var properties = await GetCurrentUserPrivileges (_serverUrl, 0);

      XmlNode privilegeWriteContent = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-privilege-set/D:privilege/D:write-content", properties.XmlNamespaceManager);
      XmlNode privilegeBind = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-privilege-set/D:privilege/D:bind", properties.XmlNamespaceManager);
      XmlNode privilegeUnbind = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-privilege-set/D:privilege/D:unbind", properties.XmlNamespaceManager);

      return ((privilegeWriteContent != null) && (privilegeBind != null) && (privilegeUnbind != null));
    }

    private async Task<string> GetEtag (Uri absoluteEntityUrl)
    {
      var headers = await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (absoluteEntityUrl, "GET", null, null, null, null, null);
      return headers.ETag.Tag;
    }

    private Task<XmlDocumentWithNamespaceManager> GetAllProperties (Uri url, int depth)
    {
      return _webDavClient.ExecuteWebDavRequestAndReadResponse (
          url,
          "PROPFIND",
          depth,
          null,
          null,
          "application/xml",
          @"<?xml version='1.0'?>
                        <D:propfind xmlns:D=""DAV:"">
                            <D:allprop/>
                        </D:propfind>
                 "
          );
    }

    protected async Task<bool> DoesSupportsReportSet (Uri url, int depth, string reportSetNamespace, string reportSet)
    {
      var document = await _webDavClient.ExecuteWebDavRequestAndReadResponse (
          url,
          "PROPFIND",
          depth,
          null,
          null,
          "application/xml",
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

    private Task<XmlDocumentWithNamespaceManager> GetCurrentUserPrivileges (Uri url, int depth)
    {
      return _webDavClient.ExecuteWebDavRequestAndReadResponse (
          url,
          "PROPFIND",
          depth,
          null,
          null,
          "application/xml",
          @"<?xml version='1.0'?>
                        <D:propfind xmlns:D=""DAV:"">
                          <D:prop>
                            <D:current-user-privilege-set/>
                          </D:prop>
                        </D:propfind>
                 "
          );
    }

    protected Task<XmlDocumentWithNamespaceManager> GetCurrentUserPrincipal(Uri url)
    {
      return _webDavClient.ExecuteWebDavRequestAndReadResponse(
          url,
          "PROPFIND",
          0,
          null,
          null,
          "application/xml",
          @"<?xml version='1.0'?>
                        <D:propfind xmlns:D=""DAV:"">
                          <D:prop>
                            <D:current-user-principal/>
                            <D:principal-URL/>
                            <D:resourcetype/>
                          </D:prop>
                        </D:propfind>
                 "
          );
    }

    public Task<EntityIdWithVersion<Uri, string>> UpdateEntity (Uri url, string contents)
    {
      return UpdateEntity (url, string.Empty, contents);
    }

    private async Task<EntityIdWithVersion<Uri, string>> UpdateEntity (Uri url, string etag, string contents)
    {
      s_logger.DebugFormat ("Updating entity '{0}'", url);

      var absoluteEventUrl = new Uri (_serverUrl, url);

      s_logger.DebugFormat ("Absolute entity location: '{0}'", absoluteEventUrl);

      IHttpHeaders responseHeaders;

      try
      {
        responseHeaders = await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (
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
        if (x.Response != null && ((HttpWebResponse) x.Response).StatusCode == HttpStatusCode.Forbidden)
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
        effectiveEventUrl = responseHeaders.Location.IsAbsoluteUri ? responseHeaders.Location : new Uri (_serverUrl, responseHeaders.Location);
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
        version = newEtag.Tag;
      }
      else
      {
        version = await GetEtag (effectiveEventUrl);
      }

      return new EntityIdWithVersion<Uri, string> (UriHelper.GetUnescapedPath (effectiveEventUrl), version);
    }

    protected async Task<EntityIdWithVersion<Uri, string>> CreateEntity (string name, string content)
    {
      var eventUrl = new Uri (_serverUrl, name);

      s_logger.DebugFormat ("Creating entity '{0}'", eventUrl);

      IHttpHeaders responseHeaders;

      try
      {
        responseHeaders = await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (
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
        if (x.Response != null && ((HttpWebResponse) x.Response).StatusCode == HttpStatusCode.Forbidden)
          throw new Exception (string.Format ("Error creating event with url '{0}' (Access denied)", eventUrl));
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
        version = etag.Tag;
      }
      else
      {
        version = await GetEtag (effectiveEventUrl);
      }

      return new EntityIdWithVersion<Uri, string> (UriHelper.GetUnescapedPath (effectiveEventUrl), version);
    }

    public Task DeleteEntity (Uri uri)
    {
      return DeleteEntity (uri, string.Empty);
    }

    private async Task DeleteEntity (Uri uri, string etag)
    {
      s_logger.DebugFormat ("Deleting entity '{0}'", uri);

      var absoluteEventUrl = new Uri (_serverUrl, uri);

      s_logger.DebugFormat ("Absolute entity location: '{0}'", absoluteEventUrl);

      IHttpHeaders responseHeaders;

      try
      {
        responseHeaders = await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (
            absoluteEventUrl,
            "DELETE",
            null,
            etag,
            null,
            null,
            string.Empty);
      }
      catch (WebException x)
      {
        if (x.Response != null)
        {
          var httpWebResponse = (HttpWebResponse) x.Response;


          if (httpWebResponse.StatusCode == HttpStatusCode.Forbidden)
            throw new Exception (string.Format ("Error deleting event with url '{0}' and etag '{1}' (Access denied)", uri, etag));
        }

        throw;
      }

      IEnumerable<string> errorValues;
      if (responseHeaders.TryGetValues ("X-Dav-Error", out errorValues))
      {
        var errorList = errorValues.ToList();
        if (errorList.Any (v => v != "200 No error"))
          throw new Exception (string.Format ("Error deleting entity with url '{0}' and etag '{1}': {2}", uri, etag, string.Join (",", errorList)));
      }
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