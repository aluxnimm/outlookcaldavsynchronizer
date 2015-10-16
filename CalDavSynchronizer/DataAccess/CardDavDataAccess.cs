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

    public async Task<IReadOnlyList<Tuple<Uri, string>>> GetUserAddressBooks(bool useWellKnownUrl)
    {
      var autodiscoveryUrl = useWellKnownUrl ? AutoDiscoveryUrl : _serverUrl;

      var properties = await GetCurrentUserPrincipal(autodiscoveryUrl);

      XmlNode principal = properties.XmlDocument.SelectSingleNode("/D:multistatus/D:response/D:propstat/D:prop/D:current-user-principal", properties.XmlNamespaceManager);

      var addressbooks = new List<Tuple<Uri, string>>();

      if (principal != null)
      {
        properties = await GetAddressBookHomeSet(new Uri(autodiscoveryUrl.GetLeftPart(UriPartial.Authority) + principal.InnerText));

        XmlNode homeSet = properties.XmlDocument.SelectSingleNode("/D:multistatus/D:response/D:propstat/D:prop/A:addressbook-home-set", properties.XmlNamespaceManager);

        if (homeSet != null)
        {
          properties = await ListAddressBooks(new Uri(autodiscoveryUrl.GetLeftPart(UriPartial.Authority) + homeSet.InnerText));

          XmlNodeList responseNodes = properties.XmlDocument.SelectNodes("/D:multistatus/D:response", properties.XmlNamespaceManager);

          foreach (XmlElement responseElement in responseNodes)
          {
            var urlNode = responseElement.SelectSingleNode("D:href", properties.XmlNamespaceManager);
            var displayNameNode = responseElement.SelectSingleNode("D:propstat/D:prop/D:displayname", properties.XmlNamespaceManager);
            if (urlNode != null && displayNameNode != null)
            {
              XmlNode isCollection = responseElement.SelectSingleNode("D:propstat/D:prop/D:resourcetype/A:addressbook", properties.XmlNamespaceManager);
              if (isCollection != null)
              {
                var uri = UriHelper.UnescapeRelativeUri(autodiscoveryUrl, urlNode.InnerText);
                addressbooks.Add(Tuple.Create(uri, displayNameNode.InnerText));
              }
            }
          }
        }
      }
      return addressbooks;
    }

    private Uri AutoDiscoveryUrl
    {
      get
      {
        return new Uri(_serverUrl.GetLeftPart(UriPartial.Authority) + "/.well-known/carddav/");
      }
    }

    private Task<XmlDocumentWithNamespaceManager> GetAddressBookHomeSet(Uri url)
    {
      return _webDavClient.ExecuteWebDavRequestAndReadResponse(
          url,
          "PROPFIND",
          0,
          null,
          null,
          "application/xml",
          @"<?xml version='1.0'?>
                        <D:propfind xmlns:D=""DAV:"" xmlns:C=""urn:ietf:params:xml:ns:carddav"">
                          <D:prop>
                            <C:addressbook-home-set/>
                          </D:prop>
                        </D:propfind>
                 "
          );
    }

    private Task<XmlDocumentWithNamespaceManager> ListAddressBooks(Uri url)
    {
      return _webDavClient.ExecuteWebDavRequestAndReadResponse(
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

    public Task<EntityIdWithVersion<Uri, string>> CreateEntity (string iCalData)
    {
      return CreateEntity (string.Format ("{0:D}.vcs", Guid.NewGuid()), iCalData);
    }

    public async Task<IReadOnlyList<EntityIdWithVersion<Uri, string>>> GetContacts ()
    {
      var entities = new List<EntityIdWithVersion<Uri, string>>();

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
          if (urlNode != null && etagNode != null)
          {
            var eTag = etagNode.InnerText;
            // the directory is also included in the list. It has a etag of '"None"' and is skipped
            if (String.Compare (eTag, @"""None""", StringComparison.OrdinalIgnoreCase) != 0)
            {
              var uri = UriHelper.UnescapeRelativeUri (_serverUrl, urlNode.InnerText);
              entities.Add (EntityIdWithVersion.Create (uri, eTag));
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

    public async Task<IReadOnlyList<EntityWithVersion<Uri, string>>> GetEntities (IEnumerable<Uri> urls)
    {
      var requestBody = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
   <C:addressbook-multiget xmlns:D=""DAV:"" xmlns:C=""urn:ietf:params:xml:ns:carddav"">
     <D:prop>
       <D:getetag/>
       <C:address-data/>
     </D:prop>
     " + String.Join (Environment.NewLine, urls.Select (u => string.Format ("<D:href>{0}</D:href>", u))) + @"
   </C:addressbook-multiget>
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

      var entities = new List<EntityWithVersion<Uri, string>>();

      if (responseNodes == null)
        return entities;

      // ReSharper disable once LoopCanBeConvertedToQuery
      // ReSharper disable once PossibleNullReferenceException
      foreach (XmlElement responseElement in responseNodes)
      {
        var urlNode = responseElement.SelectSingleNode ("D:href", responseXml.XmlNamespaceManager);
        var dataNode = responseElement.SelectSingleNode ("D:propstat/D:prop/A:address-data", responseXml.XmlNamespaceManager);
        if (urlNode != null && dataNode != null)
        {
          entities.Add (EntityWithVersion.Create (UriHelper.UnescapeRelativeUri (_serverUrl, urlNode.InnerText), dataNode.InnerText));
        }
      }

      return entities;
    }
  }
}