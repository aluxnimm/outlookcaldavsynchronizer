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
using GenSync.EntityRelationManagement;

namespace GenSync.UnitTests.Synchronization.Stubs
{
    internal class EntityRelationData : IEntityRelationData<Identifier, int, Identifier, int>
    {
        public EntityRelationData(Identifier atypeId, int atypeVersion, Identifier btypeId, int btypeVersion)
        {
            AtypeId = atypeId;
            AtypeVersion = atypeVersion;
            BtypeId = btypeId;
            BtypeVersion = btypeVersion;
        }

        public Identifier AtypeId { get; set; }
        public int AtypeVersion { get; set; }
        public Identifier BtypeId { get; set; }
        public int BtypeVersion { get; set; }
    }

    internal class EntityRelationDataString : IEntityRelationData<Identifier, string, Identifier, string>
    {
        public EntityRelationDataString(Identifier atypeId, string atypeVersion, Identifier btypeId, string btypeVersion)
        {
            AtypeId = atypeId;
            AtypeVersion = atypeVersion;
            BtypeId = btypeId;
            BtypeVersion = btypeVersion;
        }

        public Identifier AtypeId { get; set; }
        public string AtypeVersion { get; set; }
        public Identifier BtypeId { get; set; }
        public string BtypeVersion { get; set; }
    }

    internal class EntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
        : IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
    {
        public EntityRelationData(TAtypeEntityId atypeId, TAtypeEntityVersion atypeVersion, TBtypeEntityId btypeId, TBtypeEntityVersion btypeVersion)
        {
            AtypeId = atypeId;
            AtypeVersion = atypeVersion;
            BtypeId = btypeId;
            BtypeVersion = btypeVersion;
        }

        public TAtypeEntityId AtypeId { get; set; }
        public TAtypeEntityVersion AtypeVersion { get; set; }
        public TBtypeEntityId BtypeId { get; set; }
        public TBtypeEntityVersion BtypeVersion { get; set; }
    }
}