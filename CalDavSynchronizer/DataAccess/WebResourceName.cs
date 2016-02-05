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
using System.IO;

namespace CalDavSynchronizer.DataAccess
{
  public class WebResourceName
  {
    public string OriginalAbsolutePath {get; set; }
    public string Id {get; set; }

    public WebResourceName (Uri uri)
        : this (uri.AbsolutePath)
    {
      
    }

    public WebResourceName (string absolutePath)
    {
      OriginalAbsolutePath = absolutePath;
      Id = DecodedString (absolutePath);
    }
    
    private static string DecodedString (string value)
    {
      string newValue;
      while ((newValue = Uri.UnescapeDataString (value)) != value)
        value = newValue;

      return newValue;
    }

    public WebResourceName ()
    {
      
    }

    public override string ToString ()
    {
      return OriginalAbsolutePath;
    }

    public override int GetHashCode ()
    {
      return Comparer.GetHashCode (this);
    }

    public override bool Equals (object obj)
    {
      return Comparer.Equals (this, obj as WebResourceName);
    }

    public static readonly IEqualityComparer<WebResourceName> Comparer = new WebResourceNameEqualityComparer();

    private class WebResourceNameEqualityComparer : IEqualityComparer<WebResourceName>
    {
      private static readonly IEqualityComparer<string> s_stringComparer = StringComparer.OrdinalIgnoreCase;

      public bool Equals (WebResourceName x, WebResourceName y)
      {
        if (x == null)
          return y == null;

        if (y == null)
          return false;

        return s_stringComparer.Equals (x.Id, y.Id);
      }

      public int GetHashCode (WebResourceName obj)
      {
        return s_stringComparer.GetHashCode (obj.Id);
      }
    }
  }
}