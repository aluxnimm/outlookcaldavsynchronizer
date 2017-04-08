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
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizer.DataAccess.HttpClientBasedClient
{
  internal class HttpResponseHeadersAdapter : IHttpHeaders
  {
    private readonly HttpResponseHeaders _headersFromFirstCall;
    private readonly HttpResponseHeaders _headersFromLastCall;

    public HttpResponseHeadersAdapter (HttpResponseHeaders headersFromFirstCall, HttpResponseHeaders headersFromLastCall)
    {
      if (headersFromFirstCall == null)
        throw new ArgumentNullException ("headersFromFirstCall");
      if (headersFromLastCall == null)
        throw new ArgumentNullException ("headersFromLastCall");

      _headersFromFirstCall = headersFromFirstCall;
      _headersFromLastCall = headersFromLastCall;
    }

    public bool TryGetValues (string name, out IEnumerable<string> values)
    {
      return _headersFromFirstCall.TryGetValues (name, out values);
    }

    public Uri LocationOrNull
    {
      get { return _headersFromFirstCall.Location; }
    }

    public string ETagOrNull
    {
      get
      {
        var etagValue = _headersFromLastCall.ETag;
        if (etagValue != null)
          return etagValue.Tag;

        IEnumerable<string> etags;
        if (_headersFromLastCall.TryGetValues ("etag", out etags))
          return HttpUtility.GetQuotedEtag (etags.FirstOrDefault());
        else
          return null;
      }
    }
  }
}