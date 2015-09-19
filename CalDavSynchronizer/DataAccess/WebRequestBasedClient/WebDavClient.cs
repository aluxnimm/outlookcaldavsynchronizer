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
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using log4net;

namespace CalDavSynchronizer.DataAccess.WebRequestBasedClient
{
  public class WebDavClient : IWebDavClient
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly string _username;
    // TODO: consider to use SecureString
    private readonly string _password;
    private readonly TimeSpan _connectTimeout;
    private readonly TimeSpan _readWriteTimeout;
    private readonly string _userAgent;

    public WebDavClient (string username, string password, TimeSpan connectTimeout, TimeSpan readWriteTimeout)
    {
      _username = username;
      _password = password;
      _connectTimeout = connectTimeout;
      _readWriteTimeout = readWriteTimeout;
      var version = Assembly.GetExecutingAssembly().GetName().Version;
      _userAgent = string.Format ("CalDavSynchronizer/{0}.{1}", version.Major, version.Minor);
    }

    private HttpWebRequest CreateRequest (Uri url)
    {
      var request = (HttpWebRequest) HttpWebRequest.Create (url);
      request.Timeout = (int) _connectTimeout.TotalMilliseconds;
      request.ReadWriteTimeout = (int) _readWriteTimeout.TotalMilliseconds;
      request.UserAgent = _userAgent;

      if (!string.IsNullOrEmpty (_username))
      {
        request.PreAuthenticate = true;
        request.Credentials = new NetworkCredential (_username, _password);
      }
      request.AllowAutoRedirect = false;
      return request;
    }

    public Task<XmlDocumentWithNamespaceManager> ExecuteWebDavRequestAndReadResponse (
        Uri url,
        string httpMethod,
        int? depth,
        string ifMatch,
        string ifNoneMatch,
        string mediaType,
        string requestBody)
    {
      using (var response = ExecuteWebDavRequest (url, httpMethod, depth, ifMatch, ifNoneMatch, mediaType, requestBody).Item2)
      {
        using (var responseStream = response.GetResponseStream())
        {
          return Task.FromResult (CreateXmlDocument (responseStream));
        }
      }
    }

    public Task<IHttpHeaders> ExecuteWebDavRequestAndReturnResponseHeaders (
        Uri url,
        string httpMethod,
        int? depth,
        string ifMatch,
        string ifNoneMatch,
        string mediaType,
        string requestBody)
    {
      var result = ExecuteWebDavRequest (url, httpMethod, depth, ifMatch, ifNoneMatch, mediaType, requestBody);

      using (var response = result.Item2)
      {
        return Task.FromResult<IHttpHeaders> (new WebHeaderCollectionAdapter (result.Item1, response.Headers));
      }
    }

    /// <returns>
    /// Tuple with the sucsessful response and teh headerCollection from the FIRST call
    /// </returns>
    private Tuple<WebHeaderCollection, WebResponse> ExecuteWebDavRequest (
        Uri url,
        string httpMethod,
        int? depth,
        string ifMatch,
        string ifNoneMatch,
        string mediaType,
        string requestBody,
        WebHeaderCollection headersFromFirstCall = null)
    {
      var request = CreateRequest (url);

      request.Method = httpMethod;

      if (depth.HasValue)
        request.Headers.Add ("Depth", depth.Value.ToString());

      if (!string.IsNullOrEmpty (ifMatch))
        request.Headers.Add ("If-Match", ifMatch);

      if (!string.IsNullOrEmpty (ifNoneMatch))
        request.Headers.Add ("If-None-Match", ifNoneMatch);

      if (!string.IsNullOrEmpty (mediaType))
        request.ContentType = mediaType;

      if (!string.IsNullOrEmpty (requestBody))
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
          var location = response.Headers["Location"];
          using (response)
          {
            return Tuple.Create (
                headersFromFirstCall ?? response.Headers,
                ExecuteWebDavRequest (new Uri (location), httpMethod, depth, ifMatch, ifNoneMatch, mediaType, requestBody).Item2);
          }
        }
        else
        {
          s_logger.Warn ("Ignoring Redirection without Location header.");
        }
      }

      return Tuple.Create (headersFromFirstCall ?? response.Headers, response);
    }

    private XmlDocumentWithNamespaceManager CreateXmlDocument (Stream webDavXmlStream)
    {
      using (var reader = new StreamReader (webDavXmlStream, Encoding.UTF8))
      {
        XmlDocument responseBody = new XmlDocument();
        responseBody.Load (reader);

        XmlNamespaceManager namespaceManager = new XmlNamespaceManager (responseBody.NameTable);
        namespaceManager.AddNamespace ("D", "DAV:");
        namespaceManager.AddNamespace ("C", "urn:ietf:params:xml:ns:caldav");
        namespaceManager.AddNamespace ("A", "urn:ietf:params:xml:ns:carddav");

        return new XmlDocumentWithNamespaceManager (responseBody, namespaceManager);
      }
    }
  }
}