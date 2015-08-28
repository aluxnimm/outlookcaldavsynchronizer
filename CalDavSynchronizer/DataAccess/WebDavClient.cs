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
using System.Reflection;
using System.Text;
using System.Xml;
using log4net;

namespace CalDavSynchronizer.DataAccess
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

    public XmlDocumentWithNamespaceManager ExecuteWebDavRequestAndReadResponse (Uri url, Action<HttpWebRequest> modifier, string requestBody)
    {
      using (var response = ExecuteWebDavRequest (url, modifier, requestBody))
      {
        using (var responseStream = response.GetResponseStream())
        {
          return CreateXmlDocument (responseStream);
        }
      }
    }

    public WebHeaderCollection ExecuteWebDavRequestAndReturnResponseHeaders (Uri url, Action<HttpWebRequest> modifier, string requestBody)
    {
      using (var response = ExecuteWebDavRequest (url, modifier, requestBody))
      {
        return response.Headers;
      }
    }

    private WebResponse ExecuteWebDavRequest (Uri url, Action<HttpWebRequest> modifier, string requestBody)
    {
      var request = CreateRequest (url);
      modifier (request);
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
          return ExecuteWebDavRequest (new Uri (response.Headers["Location"]), modifier, requestBody);
        }
        else
        {
          s_logger.Warn ("Ignoring Redirection without Location header.");
        }
      }
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