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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.using System;

using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace CalDavSynchronizer.DataAccess
{
  class WebDavClientException : Exception
  {
    public HttpStatusCode? StatusCode { get; }
    public string StatusDescription { get; }

    private WebDavClientException (Exception innerException, HttpStatusCode? statusCode, string statusDescription)
        : base (innerException.Message, innerException)
    {
      StatusCode = statusCode;
      StatusDescription = statusDescription;
    }

    public WebDavClientException (HttpStatusCode statusCode, string statusDescription,string responseMessage)
      :base ($"Response status code does not indicate success: '{(int)statusCode}' ('{statusDescription}'). Message:\r\n{responseMessage}")
    {
      StatusCode = statusCode;
      StatusDescription = statusDescription;
    }

    public static WebDavClientException Create (WebException x)
    {
      var httpWebResponse = x.Response as HttpWebResponse;
      return new WebDavClientException (x, httpWebResponse?.StatusCode, httpWebResponse?.StatusDescription);
    }

    public static WebDavClientException Create (HttpRequestException x)
    {
      var match = Regex.Match (x.Message, @"'(?<code>\d{3})'\s+\('(?<description>.*?)'\)");
      if (match.Success)
      {
        return new WebDavClientException (
            x,
            (HttpStatusCode) int.Parse (match.Groups["code"].Value),
            match.Groups["description"].Value);
      }
      else
      {
        return new WebDavClientException (x, null, null);
      }
    }
  }
}