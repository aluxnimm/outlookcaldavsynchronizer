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
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using log4net;

namespace CalDavSynchronizer.DataAccess.WebRequestBasedClient
{
  public class WebDavClient : WebDavClientBase, IWebDavClient
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly string _username;
    private readonly SecureString _password;
    private readonly string _serverUrl;
    private readonly TimeSpan _connectTimeout;
    private readonly TimeSpan _readWriteTimeout;
    private readonly string _userAgent;
    private readonly bool _closeConnectionAfterEachRequest;
    private readonly bool _preemptiveAuthentication;
    private readonly bool _forceBasicAuthentication;


    public WebDavClient (
      string username, 
      SecureString password,
      string serverUrl, 
      TimeSpan connectTimeout, 
      TimeSpan readWriteTimeout, 
      bool closeConnectionAfterEachRequest,
      bool preemptiveAuthentication,
      bool forceBasicAuthentication,
      bool acceptInvalidChars) 
      : base (acceptInvalidChars)
    {
      _username = username;
      _password = password;
      _serverUrl = serverUrl;
      _connectTimeout = connectTimeout;
      _readWriteTimeout = readWriteTimeout;
      _closeConnectionAfterEachRequest = closeConnectionAfterEachRequest;
      _preemptiveAuthentication = preemptiveAuthentication;
      _forceBasicAuthentication = forceBasicAuthentication;
      var version = Assembly.GetExecutingAssembly().GetName().Version;
      _userAgent = string.Format ("CalDavSynchronizer/{0}.{1}", version.Major, version.Minor);
    }

    private HttpWebRequest CreateRequest (Uri url)
    {
      var request = (HttpWebRequest) HttpWebRequest.Create (url);
      request.Timeout = (int) _connectTimeout.TotalMilliseconds;
      request.ReadWriteTimeout = (int) _readWriteTimeout.TotalMilliseconds;
      request.UserAgent = _userAgent;
      request.KeepAlive = !_closeConnectionAfterEachRequest;

      if (!string.IsNullOrEmpty (_username))
      {
        request.PreAuthenticate = _preemptiveAuthentication;
        var credentials = new NetworkCredential (_username, _password);
        if (_forceBasicAuthentication)
        {
          var cache = new CredentialCache();
          cache.Add (new Uri (new Uri (_serverUrl).GetLeftPart (UriPartial.Authority)), "Basic", credentials);
          request.Credentials = cache;
        }
        else
        {
          request.Credentials = credentials;
        }
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
      try
      {
        using (var response = ExecuteWebDavRequest (url, httpMethod, depth, ifMatch, ifNoneMatch, mediaType, requestBody).Item2)
        {
          using (var responseStream = response.GetResponseStream())
          {
            return Task.FromResult (CreateXmlDocument (responseStream));
          }
        }
      }
      catch (WebException x)
      {
        throw WebDavClientException.Create (x);
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
      try
      {
        var result = ExecuteWebDavRequest (url, httpMethod, depth, ifMatch, ifNoneMatch, mediaType, requestBody);

        using (var response = result.Item2)
        {
          return Task.FromResult<IHttpHeaders> (new WebHeaderCollectionAdapter (result.Item1, response.Headers));
        }
      }
      catch (WebException x)
      {
        throw WebDavClientException.Create (x);
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
          var location = new Uri (response.Headers["Location"], UriKind.RelativeOrAbsolute);
          var effectiveLocation = location.IsAbsoluteUri ? location : new Uri (url, location);
          using (response)
          {
            return ExecuteWebDavRequest (effectiveLocation, httpMethod, depth, ifMatch, ifNoneMatch, mediaType, requestBody, headersFromFirstCall ?? response.Headers);
          }
        }
        else
        {
          s_logger.Warn ("Ignoring Redirection without Location header.");
        }
      }

      return Tuple.Create (headersFromFirstCall ?? response.Headers, response);
    }
  }
}