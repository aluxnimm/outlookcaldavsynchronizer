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
using System.Threading.Tasks;
using System.Xml;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using GenSync;
using log4net;

namespace CalDavSynchronizer.DataAccess
{
  public class CardDavDataAccess : WebDavDataAccess, ICardDavDataAccess
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    public CardDavDataAccess (Uri serverUrl, IWebDavClient webDavClient)
        : base (serverUrl, webDavClient)
    {
    }

    public Task<bool> IsAddressBookAccessSupported ()
    {
      return HasOption ("addressbook");
    }

    public Task<bool> IsResourceAddressBook ()
    {
      return IsResourceType ("A", "addressbook");
    }

    public async Task<IReadOnlyList<Tuple<Uri, string>>> GetUserAddressBooksNoThrow (bool useWellKnownUrl)
    {
      try
      {
        var autodiscoveryUrl = useWellKnownUrl ? AutoDiscoveryUrl : _serverUrl;

        var properties = await GetCurrentUserPrincipal (autodiscoveryUrl);

        XmlNode principal = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-principal", properties.XmlNamespaceManager);

        var addressbooks = new List<Tuple<Uri, string>>();

        if (principal != null)
        {
          properties = await GetAddressBookHomeSet (new Uri (autodiscoveryUrl.GetLeftPart (UriPartial.Authority) + principal.InnerText));

          XmlNode homeSet = properties.XmlDocument.SelectSingleNode ("/D:multistatus/D:response/D:propstat/D:prop/A:addressbook-home-set", properties.XmlNamespaceManager);

          if (homeSet != null && !string.IsNullOrEmpty (homeSet.InnerText))
          {
            properties = await ListAddressBooks (new Uri (autodiscoveryUrl.GetLeftPart (UriPartial.Authority) + homeSet.InnerText));

            XmlNodeList responseNodes = properties.XmlDocument.SelectNodes ("/D:multistatus/D:response", properties.XmlNamespaceManager);

            foreach (XmlElement responseElement in responseNodes)
            {
              var urlNode = responseElement.SelectSingleNode ("D:href", properties.XmlNamespaceManager);
              var displayNameNode = responseElement.SelectSingleNode ("D:propstat/D:prop/D:displayname", properties.XmlNamespaceManager);
              if (urlNode != null && displayNameNode != null)
              {
                XmlNode isCollection = responseElement.SelectSingleNode ("D:propstat/D:prop/D:resourcetype/A:addressbook", properties.XmlNamespaceManager);
                if (isCollection != null)
                {
                  var uri = UriHelper.UnescapeRelativeUri (autodiscoveryUrl, urlNode.InnerText);
                  addressbooks.Add (Tuple.Create (uri, displayNameNode.InnerText));
                }
              }
            }
          }
        }
        return addressbooks;
      }
      catch (Exception x)
      {
        if (x.Message.Contains ("404") || x.Message.Contains ("405"))
          return new List<Tuple<Uri, string>>();
        else
          throw;
      }
    }

    private Uri AutoDiscoveryUrl
    {
      get { return new Uri (_serverUrl.GetLeftPart (UriPartial.Authority) + "/.well-known/carddav/"); }
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

    public Task<EntityVersion<Uri, string>> CreateEntity (string vCardData, string uid)
    {
      return CreateNewEntity (string.Format ("{0:D}.vcs", uid), vCardData);
    }

    protected async Task<EntityVersion<Uri, string>> CreateNewEntity (string name, string content)
    {
      var contactUrl = new Uri (_serverUrl, name);

      s_logger.DebugFormat ("Creating entity '{0}'", contactUrl);

      IHttpHeaders responseHeaders;

      try
      {
        responseHeaders = await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders(
            contactUrl,
            "PUT",
            null,
            null,
            "*",
            "text/vcard",
            content);
      }
      catch (WebException x)
      {
        if (x.Response != null && ((HttpWebResponse)x.Response).StatusCode == HttpStatusCode.Forbidden)
          throw new Exception (string.Format ("Error creating contact with url '{0}' (Access denied)", contactUrl));
        else
          throw;
      }

      Uri effectiveContactUrl;
      if (responseHeaders.Location != null)
      {
        s_logger.DebugFormat ("Server sent new location: '{0}'", responseHeaders.Location);
        effectiveContactUrl = responseHeaders.Location.IsAbsoluteUri ? responseHeaders.Location : new Uri (_serverUrl, responseHeaders.Location);
        s_logger.DebugFormat ("New entity location: '{0}'", effectiveContactUrl);
      }
      else
      {
        effectiveContactUrl = contactUrl;
      }

      var etag = responseHeaders.ETag;
      string version;
      if (etag != null)
      {
        version = etag;
      }
      else
      {
        version = await GetEtag (effectiveContactUrl);
      }

      return new EntityVersion<Uri, string> (UriHelper.GetUnescapedPath (effectiveContactUrl), version);
    }

    public async Task<EntityVersion<Uri, string>> UpdateEntity (Uri url, string etag, string contents)
    {
      s_logger.DebugFormat ("Updating entity '{0}'", url);

      var absoluteContactUrl = new Uri (_serverUrl, url);

      s_logger.DebugFormat ("Absolute entity location: '{0}'", absoluteContactUrl);

      IHttpHeaders responseHeaders;

      try
      {
        responseHeaders = await _webDavClient.ExecuteWebDavRequestAndReturnResponseHeaders(
            absoluteContactUrl,
            "PUT",
            null,
            etag,
            null,
            "text/vcard",
            contents);
      }
      catch (WebException x)
      {
        if (x.Response != null && ((HttpWebResponse)x.Response).StatusCode == HttpStatusCode.Forbidden)
          throw new Exception (string.Format ("Error updating contact with url '{0}' and etag '{1}' (Access denied)", absoluteContactUrl, etag));
        else
          throw;
      }

      if (s_logger.IsDebugEnabled)
        s_logger.DebugFormat ("Updated entity. Server response header: '{0}'", responseHeaders.ToString().Replace ("\r\n", " <CR> "));

      Uri effectiveContactUrl;
      if (responseHeaders.Location != null)
      {
        s_logger.DebugFormat ("Server sent new location: '{0}'", responseHeaders.Location);
        effectiveContactUrl = responseHeaders.Location.IsAbsoluteUri ? responseHeaders.Location : new Uri (_serverUrl, responseHeaders.Location);
        s_logger.DebugFormat ("New entity location: '{0}'", effectiveContactUrl);
      }
      else
      {
        effectiveContactUrl = absoluteContactUrl;
      }

      var newEtag = responseHeaders.ETag;
      string version;
      if (newEtag != null)
      {
        version = newEtag;
      }
      else
      {
        version = await GetEtag (effectiveContactUrl);
      }

      return new EntityVersion<Uri, string> (UriHelper.GetUnescapedPath (effectiveContactUrl), version);
    }

    public async Task<IReadOnlyList<EntityVersion<Uri, string>>> GetContacts ()
    {
      var entities = new List<EntityVersion<Uri, string>>();

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

        XmlNodeList responseNodes = responseXml.XmlDocument.SelectNodes ("/D:multistatus/D:response", responseXml.XmlNamespaceManager);

        // ReSharper disable once LoopCanBeConvertedToQuery
        // ReSharper disable once PossibleNullReferenceException
        foreach (XmlElement responseElement in responseNodes)
        {
          var urlNode = responseElement.SelectSingleNode ("D:href", responseXml.XmlNamespaceManager);
          var etagNode = responseElement.SelectSingleNode ("D:propstat/D:prop/D:getetag", responseXml.XmlNamespaceManager);
          var contentTypeNode = responseElement.SelectSingleNode("D:propstat/D:prop/D:getcontenttype", responseXml.XmlNamespaceManager);

          if (urlNode != null && etagNode != null)
          {
            string contentType = contentTypeNode.InnerText ?? string.Empty;
            var eTag = HttpUtility.GetQuotedEtag(etagNode.InnerText);
            // the directory is also included in the list. It has a etag of '"None"' and is skipped
            // in Owncloud eTag is empty for directory
            // Yandex returns some eTag and the urlNode for the directory itself, so we need to filter that out aswell
            // TODO: add vlist support but for now filter out sogo vlists since we can't parse them atm

            if (  !string.IsNullOrEmpty (eTag) && 
                  String.Compare (eTag, @"""None""", StringComparison.OrdinalIgnoreCase) != 0 && 
                  _serverUrl.AbsolutePath != UriHelper.DecodeUrlString (urlNode.InnerText) &&
                  contentType != "text/x-vlist"
               )
            {
              var uri = UriHelper.UnescapeRelativeUri (_serverUrl, urlNode.InnerText);
              entities.Add (EntityVersion.Create (uri, eTag));
            }
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

    public async Task<IReadOnlyList<EntityWithId<Uri, string>>> GetEntities (IEnumerable<Uri> urls)
    {
      var requestBody = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
   <A:addressbook-multiget xmlns:D=""DAV:"" xmlns:A=""urn:ietf:params:xml:ns:carddav"">
     <D:prop>
       <D:getetag/>
       <D:getcontenttype/>
       <A:address-data/>
     </D:prop>
     " + String.Join (Environment.NewLine, urls.Select (u => string.Format ("<D:href>{0}</D:href>", u))) + @"
   </A:addressbook-multiget>
 ";


      var responseXml = await _webDavClient.ExecuteWebDavRequestAndReadResponse (
          _serverUrl,
          "REPORT",
          0,
          null,
          null,
          "application/xml",
          requestBody
          );

      XmlNodeList responseNodes = responseXml.XmlDocument.SelectNodes ("/D:multistatus/D:response", responseXml.XmlNamespaceManager);

      var entities = new List<EntityWithId<Uri, string>>();

      if (responseNodes == null)
        return entities;

      // ReSharper disable once LoopCanBeConvertedToQuery
      // ReSharper disable once PossibleNullReferenceException
      foreach (XmlElement responseElement in responseNodes)
      {
        var urlNode = responseElement.SelectSingleNode ("D:href", responseXml.XmlNamespaceManager);
        var dataNode = responseElement.SelectSingleNode ("D:propstat/D:prop/A:address-data", responseXml.XmlNamespaceManager);
        var contentTypeNode = responseElement.SelectSingleNode("D:propstat/D:prop/D:getcontenttype", responseXml.XmlNamespaceManager);
        string contentType = contentTypeNode.InnerText ?? string.Empty;

        // TODO: add vlist support but for now filter out sogo vlists since we can't parse them atm
        if (urlNode != null && dataNode != null && !string.IsNullOrEmpty (dataNode.InnerText) && contentType != "text/x-vlist")
        {
          entities.Add (EntityWithId.Create (UriHelper.UnescapeRelativeUri (_serverUrl, urlNode.InnerText), dataNode.InnerText));
        }
      }

      return entities;
    }
  }
}