using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using GenSync.EntityRelationManagement;

namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  public class EntityRelationStorage<TAtypeEntityId, TAtypeEntityVersion, TEntityRelationData, TBtypeEntityId, TBtypeEntityVersion> : IEntityRelationDataAccess<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
      where TEntityRelationData : IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
  {
    private List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> _data;

    public EntityRelationStorage ()
    {
    }

    public IEnumerable<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> LoadEntityRelationData ()
    {
      return _data;
    }

    public void SaveEntityRelationData (List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> data)
    {
      _data = data;
    }
  }
}