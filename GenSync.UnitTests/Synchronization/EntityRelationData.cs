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
using GenSync.EntityRelationManagement;

namespace GenSync.UnitTests.Synchronization
{
  internal class EntityRelationData : IEntityRelationData<string, int, string, int>
  {
    public EntityRelationData (string atypeId, int atypeVersion, string btypeId, int btypeVersion)
    {
      AtypeId = atypeId;
      AtypeVersion = atypeVersion;
      BtypeId = btypeId;
      BtypeVersion = btypeVersion;
    }

    public string AtypeId { get; set; }
    public int AtypeVersion { get; set; }
    public string BtypeId { get; set; }
    public int BtypeVersion { get; set; }
  }

  internal class EntityRelationDataString : IEntityRelationData<string, string, string, string>
  {
    public EntityRelationDataString (string atypeId, string atypeVersion, string btypeId, string btypeVersion)
    {
      AtypeId = atypeId;
      AtypeVersion = atypeVersion;
      BtypeId = btypeId;
      BtypeVersion = btypeVersion;
    }

    public string AtypeId { get; set; }
    public string AtypeVersion { get; set; }
    public string BtypeId { get; set; }
    public string BtypeVersion { get; set; }
  }
}