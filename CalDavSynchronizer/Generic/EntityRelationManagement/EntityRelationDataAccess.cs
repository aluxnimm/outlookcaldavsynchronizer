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

    public IEnumerable<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> Load ()
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

    public void Save (List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> data)
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