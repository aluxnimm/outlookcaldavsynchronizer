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
using System.Linq;
using System.Xml.Serialization;

namespace GenSync.EntityRelationManagement
{
  public static class EntityRelationDataAccess
  {
    private const string s_relationStorageName = "relations.xml";

    public static string GetRelationStoragePath (string profileDataDirectory)
    {
      return Path.Combine (profileDataDirectory, s_relationStorageName);
    }
  }

  /// <summary>
  /// Defaultimplementation for IEntityRelationDataAccess, which uses  an XML-file as underlying storage
  /// </summary>
  public class EntityRelationDataAccess<TAtypeEntityId, TAtypeEntityVersion, TEntityRelationData, TBtypeEntityId, TBtypeEntityVersion> : IEntityRelationDataAccess<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
      where TEntityRelationData : IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
  {
    private readonly XmlSerializer _serializer = new XmlSerializer (typeof (List<TEntityRelationData>));
    private readonly string _relationStorageFile;

    public EntityRelationDataAccess (string dataDirectory)
    {
      _relationStorageFile = EntityRelationDataAccess.GetRelationStoragePath (dataDirectory);
    }

    public void DeleteCaches ()
    {
      if (!File.Exists (_relationStorageFile))
        return;

      File.Delete (_relationStorageFile);
    }

    public IReadOnlyCollection<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> LoadEntityRelationData ()
    {
      if (!File.Exists (_relationStorageFile))
        return null;

      using (var stream = CreateInputStream())
      {
        var result = new List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>();
        foreach (var d in (List<TEntityRelationData>) _serializer.Deserialize (stream))
        {
          result.Add (d);
        }

        return result;
      }
    }

    public void SaveEntityRelationData (List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> data)
    {
      if (!Directory.Exists (Path.GetDirectoryName (_relationStorageFile)))
        Directory.CreateDirectory (Path.GetDirectoryName (_relationStorageFile));

      var typedData = data.Cast<TEntityRelationData>().ToList();

      using (var stream = CreateOutputStream())
      {
        _serializer.Serialize (stream, typedData);
      }
    }

    private Stream CreateOutputStream ()
    {
      return new FileStream (_relationStorageFile, FileMode.Create, FileAccess.Write);
    }

    private Stream CreateInputStream ()
    {
      return new FileStream (_relationStorageFile, FileMode.Open, FileAccess.Read);
    }
  }
}