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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using log4net;

namespace CalDavSynchronizer.DataAccess
{
  public class WebDavClient : IWebDavClient
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly ProductInfoHeaderValue _productInfo;
    private readonly Lazy<HttpClient> _httpClient;

    protected WebDavClient (Lazy<HttpClient> httpClient, string productName, string productVersion)
    {
      if (httpClient == null)
        throw new ArgumentNullException ("httpClient");

      _productInfo = new ProductInfoHeaderValue (productName, productVersion);
      _httpClient = httpClient;
    }

    private HttpRequestMessage CreateRequestMessage (Uri url)
    {
      var request = new HttpRequestMessage();

      request.RequestUri = url;
      request.Headers.UserAgent.Add (_productInfo);

      return request;
    }

    public async Task<XmlDocumentWithNamespaceManager> ExecuteWebDavRequestAndReadResponse (
        Uri url,
        Action<HttpRequestMessage> modifier,
        string mediaType,
        string requestBody)
    {
      using (var response = ExecuteWebDavRequest (url, modifier, mediaType, requestBody))
      {
        using (var responseStream = await (await response).Content.ReadAsStreamAsync())
        {
          return CreateXmlDocument (responseStream);
        }
      }
    }

    public async Task<HttpResponseHeaders> ExecuteWebDavRequestAndReturnResponseHeaders (
        Uri url,
        Action<HttpRequestMessage> modifier,
        string mediaType,
        string requestBody)
    {
      using (var response = await ExecuteWebDavRequest (url, modifier, mediaType, requestBody))
      {
        return response.Headers;
      }
    }

    private async Task<HttpResponseMessage> ExecuteWebDavRequest (Uri url, Action<HttpRequestMessage> modifier, string mediaType, string requestBody)
    {
      var requestMessage = CreateRequestMessage (url);
      modifier (requestMessage);
      if (!string.IsNullOrEmpty (requestBody))
      {
        requestMessage.Content = new StringContent (requestBody, Encoding.UTF8, mediaType);
      }

      var response = await _httpClient.Value.SendAsync (requestMessage);
      if (response.StatusCode == HttpStatusCode.Moved || response.StatusCode == HttpStatusCode.Redirect)
      {
        if (response.Headers.Location != null)
        {
          return await ExecuteWebDavRequest (response.Headers.Location, modifier, mediaType, requestBody);
        }
        else
        {
          s_logger.Warn ("Ignoring Redirection without Location header.");
        }
      }

      response.EnsureSuccessStatusCode();

      return response;
    }

    private XmlDocumentWithNamespaceManager CreateXmlDocument (Stream webDavXmlStream)
    {
      using (var reader = new StreamReader (webDavXmlStream, Encoding.UTF8))
      {
        XmlDocument responseBody = new XmlDocument();
        responseBody.Load (reader);

        XmlNamespaceManager namespaceManager = new XmlNamespaceManager (responseBody.NameTable);
        namespaceManager.AddNamespace ("D", "DAV:");
        RegisterNameSpaces (namespaceManager);

        return new XmlDocumentWithNamespaceManager (responseBody, namespaceManager);
      }
    }

    protected virtual void RegisterNameSpaces (XmlNamespaceManager namespaceManager)
    {
    }
  }
}