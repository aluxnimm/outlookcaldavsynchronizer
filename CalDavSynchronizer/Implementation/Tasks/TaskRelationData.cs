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
using System.Xml.Serialization;
using CalDavSynchronizer.Generic.EntityRelationManagement;

namespace CalDavSynchronizer.Implementation.Tasks
{
  public class TaskRelationData : IEntityRelationData<string, DateTime, Uri, string>
  {
    public string AtypeId { get; set; }
    public DateTime AtypeVersion { get; set; }

    [XmlIgnore]
    public Uri BtypeId { get; set; }

    [XmlElement ("BtypeId")]
    public string SerializableBtypeId
    {
      get { return BtypeId.ToString(); }
      set { BtypeId = new Uri (value, UriKind.Relative); }
    }

    public string BtypeVersion { get; set; }
  }
}