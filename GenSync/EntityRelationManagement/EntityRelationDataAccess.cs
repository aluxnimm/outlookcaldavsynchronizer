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
using System.Xml;
using System.Xml.Serialization;
using log4net;

namespace GenSync.EntityRelationManagement
{
  public static class EntityRelationDataAccess
  {
    private const string s_relationStorageName = "relations.xml";

    public static string GetRelationStoragePath (string profileDataDirectory)
    {
      return Path.Combine (profileDataDirectory, s_relationStorageName);
    }

    public static string GetRelationStoragePath(string profileDataDirectory, string customPrefix)
    {
      return Path.Combine(profileDataDirectory, $"{customPrefix}_{s_relationStorageName}");
    }
  }

  /// <summary>
  /// Defaultimplementation for IEntityRelationDataAccess, which uses  an XML-file as underlying storage
  /// </summary>
  public class EntityRelationDataAccess<TAtypeEntityId, TAtypeEntityVersion, TEntityRelationData, TBtypeEntityId, TBtypeEntityVersion> :
    XmlFileDataAccess<List<TEntityRelationData>>,
    IEntityRelationDataAccess<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
      where TEntityRelationData : IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);

    public EntityRelationDataAccess (string dataDirectory) : 
      base(EntityRelationDataAccess.GetRelationStoragePath(dataDirectory))
    {
    }

    public EntityRelationDataAccess(string dataDirectory, string customPrefix) :
      base(EntityRelationDataAccess.GetRelationStoragePath(dataDirectory, customPrefix))

    {
    }

    public IReadOnlyCollection<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> LoadEntityRelationData ()
    {
      return  LoadDataOrNull()
        ?.Select (d => (IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>) d)
        .ToList();
    }

    public void SaveEntityRelationData (List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> data)
    {
      SaveData(data.Cast<TEntityRelationData>().ToList());
    }
  }
}