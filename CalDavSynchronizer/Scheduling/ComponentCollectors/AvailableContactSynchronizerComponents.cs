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
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.Implementation.Contacts.VCardTypeSwitch;
using CalDavSynchronizer.Implementation.DistributionLists;
using GenSync.EntityRepositories;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Scheduling.ComponentCollectors
{
  public class AvailableContactSynchronizerComponents : AvailableSynchronizerComponents
  {
    public ICardDavDataAccess CardDavDataAccess { get; set; }
    public ICardDavDataAccess SogoDistListDataAccessOrNull { get; set; }

    public IEntityRepository<WebResourceName, string, vCard, ICardDavRepositoryLogger> CardDavEntityRepository { get; set; }
    public IEntityRepository<string, DateTime, ContactItemWrapper, ICardDavRepositoryLogger> OutlookContactRepository { get; set; }

    public IEntityRepository<WebResourceName, string, DistributionList, DistributionListSychronizationContext>  SogoDistListRepositoryOrNull { get; set; }
    public IEntityRepository<WebResourceName, string, vCard, DistributionListSychronizationContext> VCardGroupRepositoryOrNull { get; set; }
    public IEntityRepository<string, DateTime, GenericComObjectWrapper<DistListItem>, DistributionListSychronizationContext> OutlookDistListRepositoryOrNull { get; set; }


    public override DataAccessComponents GetDataAccessComponents ()
    {
      return new DataAccessComponents
      {
        CardDavDataAccess = CardDavDataAccess,
        DistListDataAccess = SogoDistListDataAccessOrNull
      };
    }
  }
}
