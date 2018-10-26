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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using CalDavSynchronizer.Ui.ConnectionTests;
using GenSync;
using GenSync.Logging;
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
      IHttpHeaders headers;
      try
      {

        headers = await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders(
          _serverUrl,
          "OPTIONS",
          null,
          null,
          null,
          null,
          null);
      }
      catch (WebDavClientException x) when (x.StatusCode == HttpStatusCode.NotFound)
      {
        // iCloud and Google CardDav return with 404 and OPTIONS doesn't work for the resource uri
        return true;
      }
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
      var properties = await GetResourceType (_serverUrl);

      XmlNode resourceTypeNode = properties.XmlDocument.SelectSingleNode (
          string.Format ("/D:multistatus/D:response/D:propstat/D:prop/D:resourcetype/{0}:{1}", @namespace, name),
          properties.XmlNamespaceManager);

      return resourceTypeNode != null;
    }

    public async Task<AccessPrivileges> GetPrivileges ()
    {
      var properties = await GetCurrentUserPrivileges (_serverUrl, 0);

      XmlNode privilegeWriteContent = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-privilege-set/D:privilege/D:write-content", properties.XmlNamespaceManager);
      XmlNode privilegeBind = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-privilege-set/D:privilege/D:bind", properties.XmlNamespaceManager);
      XmlNode privilegeUnbind = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-privilege-set/D:privilege/D:unbind", properties.XmlNamespaceManager);
      XmlNode privilegeWrite = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-privilege-set/D:privilege/D:write", properties.XmlNamespaceManager);

      if (privilegeWrite != null)
        return AccessPrivileges.All;

      var privileges = AccessPrivileges.None;
      if (privilegeWriteContent != null) privileges |= AccessPrivileges.Modify;
      if (privilegeBind !=null) privileges |= AccessPrivileges.Create;
      if (privilegeUnbind != null) privileges |= AccessPrivileges.Delete;
      return privileges;
    }

    // Only consider resource as read-only if we are sure. That is:
    // - we can query ACL property
    // - we see a READ privilage, but no WRITE privilege
    public async Task<bool> IsReadOnly(Uri resourceUrl)
    {
      try
      {
        var document = await _webDavClient.ExecuteWebDavRequestAndReadResponse(
            resourceUrl,
            "PROPFIND",
            0,
            null,
            null,
            "application/xml",
            @"<?xml version='1.0'?>
                      <D:propfind xmlns:D=""DAV:"" xmlns:C=""urn:ietf:params:xml:ns:caldav"" xmlns:E=""http://apple.com/ns/ical/"">
                          <D:prop>
                              <D:acl />
                          </D:prop>
                      </D:propfind>
                 "
            );

        if (
          document.XmlDocument.SelectSingleNode("/D:multistatus/D:response/D:propstat/D:prop/D:acl/D:ace/D:grant/D:privilege/D:read", document.XmlNamespaceManager) != null
          && document.XmlDocument.SelectSingleNode("/D:multistatus/D:response/D:propstat/D:prop/D:acl/D:ace/D:grant/D:privilege/D:write", document.XmlNamespaceManager) == null
          )
        {
          return true;
        }
        else
        {
          return false;
        }
      }
      catch (Exception x)
      {
        s_logger.Error(null, x);
        return false;
      }
    }

    protected async Task<string> GetEtag (Uri absoluteEntityUrl)
    {
      var headers = await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (absoluteEntityUrl, "GET", null, null, null, null, null);
      if (headers.ETagOrNull != null)
      {
        return headers.ETagOrNull;
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

    private Task<XmlDocumentWithNamespaceManager> GetResourceType (Uri url)
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
                              <D:resourcetype/>
                           </D:prop>
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

    public Task<bool> DoesSupportWebDavCollectionSync()
    {
      return DoesSupportsReportSet(_serverUrl, 0, "D", "sync-collection");
    }

    public async Task<(string SyncToken, IReadOnlyList<(WebResourceName Id, string Version)> ChangedOrAddedItems, IReadOnlyList<WebResourceName> DeletedItems)>
      CollectionSync(string syncTokenOrNull, IGetVersionsLogger logger)
    {
      try
      {
        var document = await _webDavClient.ExecuteWebDavRequestAndReadResponse(
          _serverUrl,
          "REPORT",
          0,
          null,
          null,
          "application/xml",
          $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
           <D:sync-collection xmlns:D=""DAV:"">
             {(syncTokenOrNull != null ? $"<D:sync-token>{syncTokenOrNull}</D:sync-token>" : "<D:sync-token/>")}
             <D:sync-level>1</D:sync-level>
             <D:prop>
               <D:getetag/>
             </D:prop>
           </D:sync-collection>
                 ");

        var syncTokenNode = document.XmlDocument.SelectSingleNode("/D:multistatus/D:sync-token", document.XmlNamespaceManager) ?? throw new Exception("Sync token missing");

        var extractedItems = ExtractCollectionItems(document, logger);
        return (syncTokenNode.InnerText, extractedItems.ChangedOrAddedItems, extractedItems.DeletedItems);
      }
      catch (WebDavClientException x) when (x.StatusCode == HttpStatusCode.Forbidden && syncTokenOrNull != null)
      {
        var isSyncTokenInvalid  = false;

        try
        {
          var xmlDocument = new XmlDocument();
          xmlDocument.LoadXml(x.ResponseMessage);
          var document = WebDavClientBase.CreateXmlDocumentWithNamespaceManager(_serverUrl, xmlDocument);
          isSyncTokenInvalid = document.XmlDocument.SelectSingleNode("/D:error/D:valid-sync-token", document.XmlNamespaceManager) != null;
        }
        catch(Exception xt)
        {
          s_logger.Info("Error while trying to Check response for invalid token message", xt);
        }

        if (isSyncTokenInvalid)
        {
          s_logger.Info("Detected invalid sync-token. Retrying CollectionSync.");
          return await CollectionSync(null, logger);
        }
        else
        {
          throw;
        }
      }
    }

    private (IReadOnlyList<(WebResourceName, string)> ChangedOrAddedItems, IReadOnlyList<WebResourceName> DeletedItems) 
      ExtractCollectionItems(XmlDocumentWithNamespaceManager responseXml, IGetVersionsLogger logger)
    {
      var responseNodes = responseXml.XmlDocument.SelectNodes("/D:multistatus/D:response", responseXml.XmlNamespaceManager);

      if (responseNodes == null)
        return (new (WebResourceName, string)[0], new WebResourceName[0]);

      var deletedEntites = new List<WebResourceName>();
      var changedOrAdded = new List<(WebResourceName, string)>();

      // ReSharper disable once LoopCanBeConvertedToQuery
      // ReSharper disable once PossibleNullReferenceException
      foreach (XmlElement responseElement in responseNodes)
      {
        var urlNode = responseElement.SelectSingleNode("D:href", responseXml.XmlNamespaceManager);
        var etagNode = responseElement.SelectSingleNode("D:propstat/D:prop/D:getetag", responseXml.XmlNamespaceManager);
        var statusNode = responseElement.SelectSingleNode("D:status", responseXml.XmlNamespaceManager);
        if (urlNode != null &&
            _serverUrl.AbsolutePath != UriHelper.DecodeUrlString(urlNode.InnerText))
        {
          var uri = new WebResourceName(urlNode.InnerText);

          if (etagNode != null)
          {
            // NOTE: according to RFC statusNote MUST be null, but sogo returns it
            var etag = HttpUtility.GetQuotedEtag(etagNode.InnerText);
            changedOrAdded.Add((uri, etag));
            if (s_logger.IsDebugEnabled)
              s_logger.DebugFormat($"Got version (HTTP 200): '{uri}': '{etag}'");
          }
          else if (statusNode != null)
          {
            // rfc2616.txt
            // Status-Line = HTTP-Version SP Status-Code SP Reason-Phrase CRLF
            // HTTP-Version= "HTTP" "/" 1*DIGIT "." 1*DIGIT
            const string statusLineRegex = @"HTTP/\d.\d (?<statusCode>\d{3}) ";
            var statusMatch = Regex.Match(statusNode.InnerText, statusLineRegex);
            if (statusMatch.Success)
            {
              var httpStatusCode = (HttpStatusCode)int.Parse(statusMatch.Groups["statusCode"].Value);
              if (httpStatusCode == HttpStatusCode.NotFound)
              {
                deletedEntites.Add(uri);
                if (s_logger.IsDebugEnabled)
                  s_logger.DebugFormat($"Got version (HTTP 404): '{uri}'");
              }
              else
              {
                throw new Exception($"Invalid status '{statusNode.InnerText}' in server response.");
              }
            }
            else
            {
              s_logger.Error($"Received invalid status line '{statusNode.InnerText}' for entity '{urlNode.InnerText}'");
              logger.LogError(uri, $"Received invalid status line '{statusNode.InnerText}'");
            }
          }
        }
      }
      
      return (changedOrAdded,deletedEntites);
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

      public static Uri AlignServerUrl (Uri configuredServerUrl, WebResourceName referenceResourceNameOrNull)
      {
        if (referenceResourceNameOrNull != null)
        {
          var filename = Path.GetFileName (referenceResourceNameOrNull.OriginalAbsolutePath);
          if (!string.IsNullOrEmpty (filename))
          {
            var filenameIndex = referenceResourceNameOrNull.OriginalAbsolutePath.LastIndexOf (filename);
            if (filenameIndex != -1)
            {
              var resourcePath = referenceResourceNameOrNull.OriginalAbsolutePath.Remove (filenameIndex);
              var newUri = new Uri (configuredServerUrl, resourcePath);

              // Only if new aligned Uri has a different encoded AbsolutePath but is identical when decoded return newUri
              // else we assume filename truncation didn't work and return the original configuredServerUrl
              if (newUri.AbsolutePath != configuredServerUrl.AbsolutePath)
              {
                if (DecodeUrlString (newUri.AbsolutePath) == DecodeUrlString (configuredServerUrl.AbsolutePath))
                {
                  return newUri;
                }
                s_logger.DebugFormat ("Aligned decoded resource uri path '{0}' different from server uri '{1}'", newUri.AbsolutePath, configuredServerUrl.AbsolutePath);
              }
            }
          }
        }
        return configuredServerUrl;
      }

    }
  }
}