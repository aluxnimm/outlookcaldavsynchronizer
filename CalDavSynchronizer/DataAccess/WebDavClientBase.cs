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
using System.Xml;

namespace CalDavSynchronizer.DataAccess
{
  public class WebDavClientBase
  {
    private readonly bool _acceptInvalidChars;

    protected WebDavClientBase (bool acceptInvalidChars)
    {
      _acceptInvalidChars = acceptInvalidChars;
    }

    protected XmlDocumentWithNamespaceManager CreateXmlDocument (Stream webDavXmlStream, Uri uri)
    {
      var responseBody = DeserializeXmlStream (webDavXmlStream);

      return CreateXmlDocumentWithNamespaceManager(uri, responseBody);
    }

    public static XmlDocumentWithNamespaceManager CreateXmlDocumentWithNamespaceManager(Uri uri, XmlDocument responseBody)
    {
      XmlNamespaceManager namespaceManager = new XmlNamespaceManager(responseBody.NameTable);

      namespaceManager.AddNamespace("D", "DAV:");
      namespaceManager.AddNamespace("C", "urn:ietf:params:xml:ns:caldav");
      namespaceManager.AddNamespace("A", "urn:ietf:params:xml:ns:carddav");
      namespaceManager.AddNamespace("E", "http://apple.com/ns/ical/");
      namespaceManager.AddNamespace("CS", "http://calendarserver.org/ns/");

      return new XmlDocumentWithNamespaceManager(responseBody, namespaceManager, uri);
    }

    private XmlDocument DeserializeXmlStream (Stream webDavXmlStream)
    {
      using (var reader = new StreamReader (webDavXmlStream, Encoding.UTF8))
      {
        if (_acceptInvalidChars)
        {
          var settings = new XmlReaderSettings();
          settings.CheckCharacters = false;
          using (var xmlReader = XmlReader.Create (reader, settings))
          {
            XmlDocument responseBody = new XmlDocument();
            responseBody.Load (xmlReader);
            return responseBody;
          }
        }
        else
        {
          XmlDocument responseBody = new XmlDocument();
          responseBody.Load (reader);
          return responseBody;
        }
      }
    }
  }
}