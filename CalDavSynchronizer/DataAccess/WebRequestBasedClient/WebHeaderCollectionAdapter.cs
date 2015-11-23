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
using System.Net;
using System.Net.Http.Headers;

namespace CalDavSynchronizer.DataAccess.WebRequestBasedClient
{
  internal class WebHeaderCollectionAdapter : IHttpHeaders
  {
    private readonly WebHeaderCollection _headerFromLastCall;
    private readonly WebHeaderCollection _headerFromFirstCall;

    public WebHeaderCollectionAdapter (WebHeaderCollection headerFromFirstCall, WebHeaderCollection headerFromLastCall)
    {
      if (headerFromLastCall == null)
        throw new ArgumentNullException ("headerFromLastCall");
      if (headerFromFirstCall == null)
        throw new ArgumentNullException ("headerFromFirstCall");

      _headerFromLastCall = headerFromLastCall;
      _headerFromFirstCall = headerFromFirstCall;
    }

    public bool TryGetValues (string name, out IEnumerable<string> values)
    {
      var value = _headerFromLastCall[name];
      if (value != null)
      {
        values = new[] { value };
        return true;
      }
      else
      {
        values = null;
        return false;
      }
    }

    public Uri Location
    {
      get
      {
        var location = _headerFromFirstCall["location"];
        return !string.IsNullOrEmpty (location) ? new Uri (location) : null;
      }
    }

    public string ETag
    {
      get
      {
        return HttpUtility.GetQuotedEtag (_headerFromLastCall["ETag"]);
      }
    }
  }
}