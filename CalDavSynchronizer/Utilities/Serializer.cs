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
using System.Text;
using System.Xml.Serialization;

namespace CalDavSynchronizer.Utilities
{
  public static class Serializer<T>
  {
    private static readonly XmlSerializer _xmlSerializer = new XmlSerializer (typeof (T));

    public static string Serialize (T o)
    {
      var stringBuilder = new StringBuilder();

      using (var writer = new StringWriter (stringBuilder))
      {
        _xmlSerializer.Serialize (writer, o);
      }

      return stringBuilder.ToString();
    }

    public static T Deserialize (string serialized)
    {
      using (var reader = new StringReader (serialized))
      {
        return (T) _xmlSerializer.Deserialize (reader);
      }
    }

    public static T DeserializeFrom (Stream stream)
    {
      return (T) _xmlSerializer.Deserialize (stream);
    }

    public static void SerializeTo (T o, Stream stream)
    {
      _xmlSerializer.Serialize (stream, o);
    }
  }
}