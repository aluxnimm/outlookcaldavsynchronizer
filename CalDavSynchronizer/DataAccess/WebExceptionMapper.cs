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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess.HttpClientBasedClient;
using CalDavSynchronizer.Implementation;

namespace CalDavSynchronizer.DataAccess
{
  static class WebExceptionMapper
  {
    private const HttpStatusCode HttpTooManyRequests = (HttpStatusCode) 429;

    public static Exception Map(WebDavClientException exception)
    {
      if (exception.StatusCode == HttpTooManyRequests)
      {
        DateTime? retryAfter = null;

        IEnumerable<string> values;
        if (exception.Headers.TryGetValues("Retry-After", out values))
        {
          if (values.Any())
          {
            int retryAfterSeconds;
            if (int.TryParse(values.First(), out retryAfterSeconds))
              retryAfter = DateTime.UtcNow.AddSeconds(retryAfterSeconds);
          }
        }

        return new WebRepositoryOverloadException (retryAfter, exception);
      }

      return exception;
    }

    internal static Exception Map(HttpRequestException x)
    {
      var match = Regex.Match (x.Message, @"'(?<code>\d{3})'\s+\('(?<description>.*?)'\)");
      if (match.Success)
      {
        var httpStatusCode = (HttpStatusCode) int.Parse (match.Groups["code"].Value);
        if (httpStatusCode == HttpTooManyRequests)
        {
          return new WebRepositoryOverloadException (null, x);
        }
        else
        {
          return new WebDavClientException(
            x,
            httpStatusCode,
            match.Groups["description"].Value,
            new NullHeaders());
        }
      }
      else
      {
        return new WebDavClientException (x, null, null, new NullHeaders());
      }
    }
  }
}
