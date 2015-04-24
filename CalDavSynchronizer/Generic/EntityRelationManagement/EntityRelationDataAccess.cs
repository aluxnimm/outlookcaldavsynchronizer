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
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace CalDavSynchronizer.Generic.EntityRelationManagement
{
  public class EntityRelationDataAccess<TAtypeEntityId, TAtypeEntityVersion, TEntityRelationData, TBtypeEntityId, TBtypeEntityVersion> : IEntityRelationDataAccess<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
      where TEntityRelationData : IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
  {
    private const string s_relationStorageName = "relations.xml";

    readonly XmlSerializer _serializer = new XmlSerializer (typeof (List<TEntityRelationData>));
    private readonly string _dataDirectory;

    public EntityRelationDataAccess (string dataDirectory)
    {
      _dataDirectory = dataDirectory;
    }

    public void DeleteCaches ()
    {
      if (!Directory.Exists (_dataDirectory))
        return;

      File.Delete (GetFullEntityPath (s_relationStorageName));

    }

    public IEnumerable<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> LoadEntityRelationData ()
    {
      if (!DoesEntityExist (s_relationStorageName))
        return null;

      using (var stream = CreateInputStream (s_relationStorageName))
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
      if (!Directory.Exists (_dataDirectory))
        Directory.CreateDirectory (_dataDirectory);

      var typedData = data.Cast<TEntityRelationData>().ToList();

      using (var stream = CreateOutputStream (s_relationStorageName))
      {
        _serializer.Serialize (stream, typedData);
      }
    }

    private bool DoesEntityExist (string entityId)
    {
      return File.Exists (GetFullEntityPath (entityId));
    }
    
    private Stream CreateOutputStream (string entityId)
    {
      return new FileStream (GetFullEntityPath (entityId), FileMode.Create, FileAccess.Write);
    }

    private string GetFullEntityPath (string entityId)
    {
      return Path.Combine (_dataDirectory, entityId);
    }

    private Stream CreateInputStream (string entityId)
    {
      return new FileStream (GetFullEntityPath (entityId), FileMode.Open, FileAccess.Read);
    }

 
  }
}