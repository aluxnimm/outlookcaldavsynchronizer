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
using System.Linq;
using System.Xml.Serialization;

namespace GenSync.Logging
{
  public class SynchronizationReport
  {
    private LoadError[] _loadErrors;
    private EntitySynchronizationReport[] _entitySynchronizationReports;
    public string ProfileName { get; set; }
    public Guid ProfileId { get; set; }
    public DateTime StartTime { get; set; }

    [XmlIgnore]
    public TimeSpan Duration { get; set; }

    public string ADelta { get; set; }
    public string BDelta { get; set; }

    public LoadError[] LoadErrors
    {
      get { return _loadErrors ?? new LoadError[] { }; }
      set { _loadErrors = value; }
    }

    public EntitySynchronizationReport[] EntitySynchronizationReports
    {
      get { return _entitySynchronizationReports ?? new EntitySynchronizationReport[] { }; }
      set { _entitySynchronizationReports = value; }
    }

    public string ExceptionThatLeadToAbortion { get; set; }

    [XmlElement (ElementName = "Duration")]
    public string Duration_ForSerializationOnly
    {
      get { return Duration.ToString(); }
      set { Duration = TimeSpan.Parse (value); }
    }

    public bool HasErrors
    {
      get
      {
        return !string.IsNullOrEmpty (ExceptionThatLeadToAbortion)
               || EntitySynchronizationReports.Any (r => !string.IsNullOrEmpty (r.ExceptionThatLeadToAbortion) || r.MappingErrors.Length > 0)
               || LoadErrors.Length > 0;
      }
    }

    public bool HasWarnings => EntitySynchronizationReports.Any (r => r.MappingWarnings.Length > 0);
  }
}