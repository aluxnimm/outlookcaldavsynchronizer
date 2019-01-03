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
using System.Xml.Serialization;

namespace GenSync.Logging
{
  public class EntitySynchronizationReport
  {
    private string[] _errors;
    private string[] _warnings;

    [XmlArray("MappingErrors")]
    public string[] Errors
    {
      get => _errors ?? new string[] {};
      set => _errors = value;
    }

    [XmlArray("MappingWarnings")]
    public string[] Warnings
    {
      get => _warnings ?? new string[] { };
      set => _warnings = value;
    }

    public string AId { get; set; }
    public string BId { get; set; }

    public string ADisplayName { get; set; }
    public string BDisplayName { get; set; }

    public string ExceptionThatLeadToAbortion { get; set; }
    public SynchronizationOperation Operation { get; set; }

    [XmlIgnore]
    public bool HasErrors => ExceptionThatLeadToAbortion != null || _errors?.Length > 0;

    [XmlIgnore]
    public bool HasWarnings => _warnings?.Length > 0;
  }
}