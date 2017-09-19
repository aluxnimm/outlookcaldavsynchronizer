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
using System.Xml;
using System.Xml.Serialization;
using log4net;

namespace GenSync.EntityRelationManagement
{
  public class XmlFileDataAccess<TData> 
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);

    private readonly XmlSerializer _serializer = new XmlSerializer (typeof (TData));
    private readonly string _storageFile;
    private bool _ignoreInvalidXml = true;

    public XmlFileDataAccess(string storageFile)
    {
      _storageFile = storageFile ?? throw new ArgumentNullException(nameof(storageFile));
    }

    public TData LoadDataOrNull ()
    {
      if (!File.Exists (_storageFile))
        return default(TData);

      using (var stream = CreateInputStream())
      {
        try
        {
          return (TData) _serializer.Deserialize (stream);
        }
        catch (Exception x) when (_ignoreInvalidXml && IsXmlException(x))
        {
          s_logger.Warn ("Error when deserializing EntityRelationData. Ignoring error.", x);
          return default(TData);
        }
      }
    }

    private static bool IsXmlException (Exception x)
    {
      for (var ex = x; ex != null; ex = ex.InnerException)
      {
        if (ex is XmlException)
        {
          return true;
        }
      }

      return false;
    }

    public void SaveData (TData data)
    {
      if (!Directory.Exists (Path.GetDirectoryName (_storageFile)))
        Directory.CreateDirectory (Path.GetDirectoryName (_storageFile));

      using (var stream = CreateOutputStream())
      {
        _serializer.Serialize (stream, data);
      }

      _ignoreInvalidXml = false;
    }

    private Stream CreateOutputStream ()
    {
      return new FileStream (_storageFile, FileMode.Create, FileAccess.Write);
    }

    private Stream CreateInputStream ()
    {
      return new FileStream (_storageFile, FileMode.Open, FileAccess.Read);
    }
  }
}