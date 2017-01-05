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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.Utilities;
using GenSync.Logging;

namespace CalDavSynchronizer.Implementation.DistributionLists
{
  class OwnCloudDistributionListRepository : CardDavEntityRepository<DistributionList, int, DistributionListSychronizationContext>
  {
    public OwnCloudDistributionListRepository (ICardDavDataAccess cardDavDataAccess, IChunkedExecutor chunkedExecutor) : base(cardDavDataAccess, chunkedExecutor)
    {
    }

    protected override void SetUid(DistributionList entity, string uid)
    {
      entity.Uid = uid;
    }

    protected override string GetUid(DistributionList entity)
    {
      return entity.Uid;
    }

    protected override string Serialize(DistributionList vcard)
    {
      throw new NotImplementedException();
    }

    protected override bool TryDeserialize(string vcardData, out DistributionList vcard, WebResourceName uriOfAddressbookForLogging, int deserializationThreadLocal, ILoadEntityLogger logger)
    {
      throw new NotImplementedException();
    }
  }
}
