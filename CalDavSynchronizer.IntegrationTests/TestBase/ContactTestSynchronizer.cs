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
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Contacts;
using CalDavSynchronizer.Implementation.DistributionLists;
using CalDavSynchronizer.Implementation.DistributionLists.Sogo;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Scheduling.ComponentCollectors;
using CalDavSynchronizer.Synchronization;
using DDay.iCal;
using GenSync.Synchronization;
using Thought.vCards;

namespace CalDavSynchronizer.IntegrationTests.TestBase
{
  public class ContactTestSynchronizer : TestSynchronizerBase
  {
    public AvailableContactSynchronizerComponents Components { get; private set; }
    public EasyAccessRepositoryAdapter<string, DateTime, IContactItemWrapper, ICardDavRepositoryLogger> Outlook { get; private set; }
    public EasyAccessRepositoryAdapter<WebResourceName, string, vCard, ICardDavRepositoryLogger> Server { get; private set; }
    public EasyAccessRepositoryAdapter<string, DateTime, IDistListItemWrapper, DistributionListSychronizationContext> OutlookDistListsOrNull { get; private set; }
    public EasyAccessRepositoryAdapter<WebResourceName, string, vCard, DistributionListSychronizationContext> ServerVCardGroupsOrNull { get; private set; }
    public EasyAccessRepositoryAdapter<WebResourceName, string, DistributionList, DistributionListSychronizationContext> ServerSogoDistListsOrNull { get; private set; }

    public override async Task DeleteAllEntites ()
    {
      await Outlook.DeleteAllEntities ();
      await Server.DeleteAllEntities ();
      if(OutlookDistListsOrNull != null)
        await OutlookDistListsOrNull.DeleteAllEntities ();
      if (ServerVCardGroupsOrNull != null)
        await ServerVCardGroupsOrNull.DeleteAllEntities ();
      if (ServerSogoDistListsOrNull != null)
        await ServerSogoDistListsOrNull.DeleteAllEntities ();
    }

    public ContactTestSynchronizer(Options options, TestComponentContainer testComponentContainer) : base(options, testComponentContainer)
    {
    }

    protected override async Task<IOutlookSynchronizer> InitializeOverride ()
    {
      var synchronizerWithComponents = await TestComponentContainer.SynchronizerFactory.CreateSynchronizerWithComponents (Options, TestComponentContainer.GeneralOptions);

      var components = (AvailableContactSynchronizerComponents) synchronizerWithComponents.Item2;

      var synchronizer = synchronizerWithComponents.Item1;
      Components = components;
      Outlook = EasyAccessRepositoryAdapter.Create (components.OutlookContactRepository, new SynchronizationContextFactory<ICardDavRepositoryLogger>(() =>  NullCardDavRepositoryLogger.Instance));
      Server = EasyAccessRepositoryAdapter.Create (components.CardDavEntityRepository, new SynchronizationContextFactory<ICardDavRepositoryLogger> (() => NullCardDavRepositoryLogger.Instance));

      var distributionListSychronizationContextDummy = new DistributionListSychronizationContext(new CacheItem[0], new OutlookSession(TestComponentContainer.Application.Session));

      if(components.OutlookDistListRepositoryOrNull != null)
        OutlookDistListsOrNull = EasyAccessRepositoryAdapter.Create (components.OutlookDistListRepositoryOrNull, new SynchronizationContextFactory<DistributionListSychronizationContext> (() => distributionListSychronizationContextDummy));
      if(components.SogoDistListRepositoryOrNull != null)
        ServerSogoDistListsOrNull = EasyAccessRepositoryAdapter.Create (components.SogoDistListRepositoryOrNull, new SynchronizationContextFactory<DistributionListSychronizationContext> (() => distributionListSychronizationContextDummy));
      if(components.VCardGroupRepositoryOrNull != null)
        ServerVCardGroupsOrNull = EasyAccessRepositoryAdapter.Create (components.VCardGroupRepositoryOrNull, new SynchronizationContextFactory<DistributionListSychronizationContext> (() => distributionListSychronizationContextDummy));

      return synchronizer;
    }
  }
}