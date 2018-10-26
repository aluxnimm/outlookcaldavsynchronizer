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
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using System.Xml;
using CalDavSynchronizer.Ui.ConnectionTests;
using GenSync;
using log4net;

namespace CalDavSynchronizer.DataAccess
{
  public class CardDavDataAccess : WebDavDataAccess, ICardDavDataAccess
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly Predicate<string> _contentTypeReadPredicate;
    private readonly string _writeContentType;

    public CardDavDataAccess (Uri serverUrl, IWebDavClient webDavClient, string writeContentType, Predicate<string> contentTypeReadPredicate)
        : base (serverUrl, webDavClient)
    {
      if (writeContentType == null) throw new ArgumentNullException(nameof(writeContentType));
      if (contentTypeReadPredicate == null) throw new ArgumentNullException(nameof(contentTypeReadPredicate));
      _contentTypeReadPredicate = contentTypeReadPredicate;
      _writeContentType = writeContentType;
    }

    public Task<bool> IsAddressBookAccessSupported ()
    {
      return HasOption ("addressbook");
    }

    public Task<bool> IsResourceAddressBook ()
    {
      return IsResourceType ("A", "addressbook");
    }

    public async Task<Uri> GetCalendarHomeSetUriOrNull(bool useWellKnownUrl)
    {
      var autodiscoveryUrl = useWellKnownUrl ? AutoDiscoveryUrl : _serverUrl;

      var currentUserPrincipalUrl = await GetCurrentUserPrincipalUrl (autodiscoveryUrl);
      if (currentUserPrincipalUrl != null)
      {
        var addressBookHomeSetProperties = await GetAddressBookHomeSet (currentUserPrincipalUrl);

        XmlNode homeSetNode = addressBookHomeSetProperties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/A:addressbook-home-set", addressBookHomeSetProperties.XmlNamespaceManager);

        if (homeSetNode != null && homeSetNode.HasChildNodes)
        {
          foreach (XmlNode homeSetNodeHref in homeSetNode.ChildNodes)
          {
            if (!string.IsNullOrEmpty (homeSetNodeHref.InnerText))
            {
              var addressBookHomeSetUri = Uri.IsWellFormedUriString (homeSetNodeHref.InnerText, UriKind.Absolute)
                ? new Uri(homeSetNodeHref.InnerText)
                : new Uri(addressBookHomeSetProperties.DocumentUri.GetLeftPart (UriPartial.Authority) + homeSetNodeHref.InnerText);
              return addressBookHomeSetUri;
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
                    <D:mkcol xmlns:D=""DAV:""  xmlns:A=""urn:ietf:params:xml:ns:carddav"">
                      <D:set>
                        <D:prop>
                          <D:resourcetype>
                            <D:collection/>
                            <A:addressbook/>
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
    public async Task<IReadOnlyList<AddressBookData>> GetUserAddressBooksNoThrow (bool useWellKnownUrl)
    {
      try
      {
        var autodiscoveryUrl = useWellKnownUrl ? AutoDiscoveryUrl : _serverUrl;

        var currentUserPrincipalUrl = await GetCurrentUserPrincipalUrl (autodiscoveryUrl);

        var addressbooks = new List<AddressBookData>();

        if (currentUserPrincipalUrl != null)
        {
          var addressBookHomeSetProperties = await GetAddressBookHomeSet (currentUserPrincipalUrl);

          XmlNode homeSetNode = addressBookHomeSetProperties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/A:addressbook-home-set", addressBookHomeSetProperties.XmlNamespaceManager);

          if (homeSetNode != null && homeSetNode.HasChildNodes)
          {
            foreach (XmlNode homeSetNodeHref in homeSetNode.ChildNodes)
            {
              if (!string.IsNullOrEmpty (homeSetNodeHref.InnerText))
              {
                var addressBookHomeSetUri = Uri.IsWellFormedUriString (homeSetNodeHref.InnerText, UriKind.Absolute) ? 
                  new Uri (homeSetNodeHref.InnerText) : 
                  new Uri (addressBookHomeSetProperties.DocumentUri.GetLeftPart (UriPartial.Authority) + homeSetNodeHref.InnerText);

                var addressBookDocument = await ListAddressBooks (addressBookHomeSetUri);

                XmlNodeList responseNodes = addressBookDocument.XmlDocument.SelectNodes ("/D:multistatus/D:response", addressBookDocument.XmlNamespaceManager);

                foreach (XmlElement responseElement in responseNodes)
                {
                  var urlNode = responseElement.SelectSingleNode ("D:href", addressBookDocument.XmlNamespaceManager);
                  var displayNameNode = responseElement.SelectSingleNode ("D:propstat/D:prop/D:displayname", addressBookDocument.XmlNamespaceManager);
                  if (urlNode != null && displayNameNode != null)
                  {
                    XmlNode isCollection = responseElement.SelectSingleNode ("D:propstat/D:prop/D:resourcetype/A:addressbook", addressBookDocument.XmlNamespaceManager);
                    if (isCollection != null)
                    {
                      var path = urlNode.InnerText.EndsWith ("/") ? urlNode.InnerText : urlNode.InnerText + "/";
                      var displayName = string.IsNullOrEmpty (displayNameNode.InnerText) ? "Default Addressbook" : displayNameNode.InnerText;
                      var uri = new Uri(addressBookDocument.DocumentUri, path);
                      bool ro = await IsReadOnly(uri);
                      addressbooks.Add (new AddressBookData (uri, displayName, ro));
                    }
                  }
                }
              }
            }
          }
        }
        return addressbooks;
      }
      catch (Exception x)
      {
        if (x.Message.Contains ("404") || x.Message.Contains ("405") || x is XmlException)
          return new AddressBookData[] { };
        else
          throw;
      }
    }

    private Uri AutoDiscoveryUrl
    {
      get { return new Uri (_serverUrl.GetLeftPart (UriPartial.Authority) + "/.well-known/carddav"); }
    }

    private Task<XmlDocumentWithNamespaceManager> GetAddressBookHomeSet (Uri url)
    {
      return _webDavClient.ExecuteWebDavRequestAndReadResponse (
          url,
          "PROPFIND",
          0,
          null,
          null,
          "application/xml",
          @"<?xml version='1.0'?>
                        <D:propfind xmlns:D=""DAV:"" xmlns:A=""urn:ietf:params:xml:ns:carddav"">
                          <D:prop>
                            <A:addressbook-home-set/>
                          </D:prop>
                        </D:propfind>
                 "
          );
    }

    private Task<XmlDocumentWithNamespaceManager> ListAddressBooks (Uri url)
    {
      return _webDavClient.ExecuteWebDavRequestAndReadResponse (
          url,
          "PROPFIND",
          1,
          null,
          null,
          "application/xml",
          @"<?xml version='1.0'?>
                        <D:propfind xmlns:D=""DAV:"">
                          <D:prop>
                              <D:resourcetype />
                              <D:displayname />
                          </D:prop>
                        </D:propfind>
                 "
          );
    }

    public Task<EntityVersion<WebResourceName, string>> CreateEntity (string vCardData, string uid)
    {
      return CreateNewEntity (string.Format ("{0:D}.vcf", uid), vCardData);
    }

    protected async Task<EntityVersion<WebResourceName, string>> CreateNewEntity (string name, string content)
    {
      var contactUrl = new Uri (_serverUrl, name);

      s_logger.DebugFormat ("Creating entity '{0}'", contactUrl);

      IHttpHeaders responseHeaders = await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (
        contactUrl,
        "PUT",
        null,
        null,
        "*",
        _writeContentType,
        content);

      Uri effectiveContactUrl;
      if (responseHeaders.LocationOrNull != null)
      {
        s_logger.DebugFormat ("Server sent new location: '{0}'", responseHeaders.LocationOrNull);
        effectiveContactUrl = responseHeaders.LocationOrNull.IsAbsoluteUri ? responseHeaders.LocationOrNull : new Uri (_serverUrl, responseHeaders.LocationOrNull);
        s_logger.DebugFormat ("New entity location: '{0}'", effectiveContactUrl);
      }
      else
      {
        effectiveContactUrl = contactUrl;
      }

      var etag = responseHeaders.ETagOrNull;
      string version;
      if (etag != null)
      {
        version = etag;
      }
      else
      {
        version = await GetEtag (effectiveContactUrl);
      }

      return new EntityVersion<WebResourceName, string> (new WebResourceName(effectiveContactUrl), version);
    }

    public async Task<EntityVersion<WebResourceName, string>> TryUpdateEntity (WebResourceName url, string etag, string contents)
    {
      s_logger.DebugFormat ("Updating entity '{0}'", url);

      var absoluteContactUrl = new Uri (_serverUrl, url.OriginalAbsolutePath);

      s_logger.DebugFormat ("Absolute entity location: '{0}'", absoluteContactUrl);

      IHttpHeaders responseHeaders;
      try
      {
        responseHeaders = await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders (
            absoluteContactUrl,
            "PUT",
            null,
            etag,
            null,
            _writeContentType,
            contents);
      }
      catch (WebDavClientException x) when (x.StatusCode == HttpStatusCode.NotFound || x.StatusCode == HttpStatusCode.PreconditionFailed)
      {
        return null;
      }

      if (s_logger.IsDebugEnabled)
        s_logger.DebugFormat ("Updated entity. Server response header: '{0}'", responseHeaders.ToString().Replace ("\r\n", " <CR> "));

      Uri effectiveContactUrl;
      if (responseHeaders.LocationOrNull != null)
      {
        s_logger.DebugFormat ("Server sent new location: '{0}'", responseHeaders.LocationOrNull);
        effectiveContactUrl = responseHeaders.LocationOrNull.IsAbsoluteUri ? responseHeaders.LocationOrNull : new Uri (_serverUrl, responseHeaders.LocationOrNull);
        s_logger.DebugFormat ("New entity location: '{0}'", effectiveContactUrl);
      }
      else
      {
        effectiveContactUrl = absoluteContactUrl;
      }

      var newEtag = responseHeaders.ETagOrNull;
      string version;
      if (newEtag != null)
      {
        version = newEtag;
      }
      else
      {
        version = await GetEtag (effectiveContactUrl);
      }

      return new EntityVersion<WebResourceName, string> (new WebResourceName(effectiveContactUrl), version);
    }

    public async Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetVersions (IEnumerable<WebResourceName> urls)
    {
      WebResourceName firstResourceNameOrNull = null;

      var requestBody = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
                           <A:addressbook-multiget xmlns:D=""DAV:"" xmlns:A=""urn:ietf:params:xml:ns:carddav"">
                             <D:prop>
                               <D:getetag/>
                               <D:getcontenttype/>
                             </D:prop>
                             " +
                        String.Join(
                          "\r\n",
                          urls.Select(u =>
                          {
                            if (firstResourceNameOrNull == null)
                              firstResourceNameOrNull = u;
                            return $"<D:href>{SecurityElement.Escape(u.OriginalAbsolutePath)}</D:href>";
                          })) + @"
                           </A:addressbook-multiget>
                         ";
      try
      {
        var responseXml = await _webDavClient.ExecuteWebDavRequestAndReadResponse (
            UriHelper.AlignServerUrl (_serverUrl, firstResourceNameOrNull),
            "REPORT",
            0,
            null,
            null,
            "application/xml",
            requestBody
            );

        return ExtractVersions (responseXml);
      }
      catch (WebDavClientException x)
      {
        if (x.StatusCode == HttpStatusCode.NotFound && await IsResourceAddressBook ())
          return new EntityVersion<WebResourceName, string>[] { };

        throw;
      }
    }

    public async Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetAllVersions ()
    {
      try
      {
        var responseXml = await _webDavClient.ExecuteWebDavRequestAndReadResponse (
            _serverUrl,
            "PROPFIND",
            1,
            null,
            null,
            "application/xml",
            @"<?xml version='1.0'?>
                        <D:propfind xmlns:D=""DAV:"">
                            <D:prop>
                              <D:getetag/>
                              <D:getcontenttype/>
                            </D:prop>
                        </D:propfind>
                 "
            );

        return ExtractVersions(responseXml);
      }
      catch (WebDavClientException x)
      {
        if (x.StatusCode == HttpStatusCode.NotFound && await IsResourceAddressBook ())
          return new EntityVersion<WebResourceName, string>[] {};

        throw;
      }
    }

    private IReadOnlyList<EntityVersion<WebResourceName, string>> ExtractVersions (XmlDocumentWithNamespaceManager responseXml)
    {
      s_logger.Debug ("Entered ExtractVersions.");

      var entities = new List<EntityVersion<WebResourceName, string>> ();

      XmlNodeList responseNodes = responseXml.XmlDocument.SelectNodes ("/D:multistatus/D:response", responseXml.XmlNamespaceManager);

      // ReSharper disable once LoopCanBeConvertedToQuery
      // ReSharper disable once PossibleNullReferenceException
      foreach (XmlElement responseElement in responseNodes)
      {
        var urlNode = responseElement.SelectSingleNode ("D:href", responseXml.XmlNamespaceManager);
        var etagNode = responseElement.SelectSingleNode ("D:propstat/D:prop/D:getetag", responseXml.XmlNamespaceManager);
        var contentTypeNode = responseElement.SelectSingleNode ("D:propstat/D:prop/D:getcontenttype", responseXml.XmlNamespaceManager);

        if (urlNode != null && etagNode != null)
        {
          string contentType = contentTypeNode?.InnerText ?? string.Empty;
          var eTag = HttpUtility.GetQuotedEtag (etagNode.InnerText);
          // the directory is also included in the list. It has a etag of '"None"' and is skipped
          // in Owncloud eTag is empty for directory
          // Yandex returns some eTag and the urlNode for the directory itself, so we need to filter that out aswell
          // TODO: add vlist support but for now filter out sogo vlists since we can't parse them atm

          if (!string.IsNullOrEmpty (eTag) &&
              String.Compare (eTag, @"""None""", StringComparison.OrdinalIgnoreCase) != 0 &&
              _serverUrl.AbsolutePath != UriHelper.DecodeUrlString (urlNode.InnerText) &&
              _contentTypeReadPredicate(contentType)
              )
          {
            if (s_logger.IsDebugEnabled)
              s_logger.DebugFormat ($"'{urlNode.InnerText}': '{eTag}'");
            entities.Add (EntityVersion.Create (new WebResourceName (urlNode.InnerText), eTag));
          }
        }
      }

      s_logger.Debug ("Exiting ExtractVersions.");
      return entities;
    }

    public async Task<IReadOnlyList<EntityWithId<WebResourceName, string>>> GetEntities (IEnumerable<WebResourceName> urls)
    {
      s_logger.Debug ("Entered GetEntities.");

      WebResourceName firstResourceNameOrNull = null;

      var requestBody = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
   <A:addressbook-multiget xmlns:D=""DAV:"" xmlns:A=""urn:ietf:params:xml:ns:carddav"">
     <D:prop>
       <D:getetag/>
       <D:getcontenttype/>
       <A:address-data/>
     </D:prop>
     " + 
     String.Join ("\r\n", urls.Select (u => 
     {
        if (s_logger.IsDebugEnabled)
          s_logger.Debug ($"Requesting: '{u}'");

       if (firstResourceNameOrNull == null)
         firstResourceNameOrNull = u;

       return $"<D:href>{SecurityElement.Escape (u.OriginalAbsolutePath)}</D:href>";
     })) 
     + @"
   </A:addressbook-multiget>
 ";

      var responseXml = await _webDavClient.ExecuteWebDavRequestAndReadResponse (
          UriHelper.AlignServerUrl (_serverUrl, firstResourceNameOrNull) ,
          "REPORT",
          0,
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
        var dataNode = responseElement.SelectSingleNode ("D:propstat/D:prop/A:address-data", responseXml.XmlNamespaceManager);
        var contentTypeNode = responseElement.SelectSingleNode("D:propstat/D:prop/D:getcontenttype", responseXml.XmlNamespaceManager);
        string contentType = contentTypeNode?.InnerText ?? string.Empty;

        if (urlNode != null && dataNode != null && !string.IsNullOrEmpty (dataNode.InnerText) && _contentTypeReadPredicate(contentType))
        {
          if (s_logger.IsDebugEnabled)
            s_logger.DebugFormat ($"Got: '{urlNode.InnerText}'");
          entities.Add (EntityWithId.Create (new WebResourceName(urlNode.InnerText), dataNode.InnerText));
        }
      }

      s_logger.Debug ("Exiting GetEntities.");
      return entities;
    }
  }
}