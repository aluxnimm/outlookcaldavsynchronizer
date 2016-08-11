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
      XmlNode privilegeWrite = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-privilege-set/D:privilege/D:write", properties.XmlNamespaceManager);

      return ((privilegeWrite != null) || ((privilegeWriteContent != null) && (privilegeBind != null) && (privilegeUnbind != null)));
    }

    protected async Task<string> GetEtag (Uri absoluteEntityUrl)
    {
      var headers = await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (absoluteEntityUrl, "GET", null, null, null, null, null);
      if (headers.ETag != null)
      {
        return headers.ETag;
      }
      else
      {
        return await GetEtagViaPropfind (absoluteEntityUrl);
      }
    }

    private async Task<string> GetEtagViaPropfind (Uri url)
    {
      var document = await _webDavClient.ExecuteWebDavRequestAndReadResponse (
          url,
          "PROPFIND",
          0,
          null,
          null,
          "application/xml",
          @"<?xml version='1.0'?>
                        <D:propfind xmlns:D=""DAV:"">
                          <D:prop>
                            <D:getetag/>
                          </D:prop>
                        </D:propfind>
                 "
          );

      XmlNode eTagNode = document.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:getetag", document.XmlNamespaceManager);

      return HttpUtility.GetQuotedEtag(eTagNode.InnerText);
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

    protected Task<XmlDocumentWithNamespaceManager> GetCurrentUserPrincipal (Uri url)
    {
      return _webDavClient.ExecuteWebDavRequestAndReadResponse (
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

    public async Task<bool> TryDeleteEntity (WebResourceName uri, string etag)
    {
      s_logger.DebugFormat ("Deleting entity '{0}'", uri);

      var absoluteEventUrl = new Uri (_serverUrl, uri.OriginalAbsolutePath);

      s_logger.DebugFormat ("Absolute entity location: '{0}'", absoluteEventUrl);

      IHttpHeaders responseHeaders = null;

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
      catch (WebDavClientException x) when (x.StatusCode == HttpStatusCode.NotFound  || x.StatusCode == HttpStatusCode.PreconditionFailed)
      {
          return false;
      }

      IEnumerable<string> errorValues;
      if (responseHeaders.TryGetValues ("X-Dav-Error", out errorValues))
      {
        var errorList = errorValues.ToList();
        if (errorList.Any (v => v != "200 No error"))
          throw new Exception (string.Format ("Error deleting entity with url '{0}' and etag '{1}': {2}", uri, etag, string.Join (",", errorList)));
      }

      return true;
    }

    protected async Task<Uri> GetCurrentUserPrincipalUrl (Uri calenderUrl)
    {
      var principalProperties = await GetCurrentUserPrincipal (calenderUrl);

      XmlNode principalUrlNode = principalProperties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-principal", principalProperties.XmlNamespaceManager) ??
                                 principalProperties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:principal-URL", principalProperties.XmlNamespaceManager);

      if (principalUrlNode != null && !string.IsNullOrEmpty (principalUrlNode.InnerText))
        return new Uri (principalProperties.DocumentUri.GetLeftPart (UriPartial.Authority) + principalUrlNode.InnerText);
      else
        return null;
    }


    /// <summary>
    /// Since Uri.Compare() cannot compare relative Uris and ignoring escapes, all Uris have to be unescaped when entering CalDavSynchronizer
    /// </summary>
    protected static class UriHelper
    {
      public static Uri UnescapeRelativeUri (Uri baseUri, string relativeUriString)
      {

        var relativeUri = new Uri (DecodeUrlString (relativeUriString) , UriKind.Relative);
        var aboluteUri = new Uri (baseUri, relativeUri);
        var unescapedRelativeUri = new Uri (aboluteUri.GetComponents (UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.Unescaped), UriKind.Relative);
        return unescapedRelativeUri;
      }

      public static string DecodeUrlString (string url)
      {
        string newUrl;
        while ((newUrl = Uri.UnescapeDataString (url)) != url)
          url = newUrl;
        return newUrl;
      }

      public static Uri AlignServerUrl(Uri configuredServerUrl, WebResourceName referenceResourceNameOrNull)
      {
        // TODO: implement alignment
        return configuredServerUrl;
      }

    }
  }
}