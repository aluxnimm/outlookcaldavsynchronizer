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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.GoogleContacts;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.Scheduling.ComponentCollectors;
using CalDavSynchronizer.Synchronization;
using GenSync.Synchronization;
using Google.GData.Extensions;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests.TestBase
{
 
  public class GoogleTaskTestSynchronizer : TestSynchronizerBase
  {
    public AvailableGoogleTaskApiSynchronizerComponents Components { get; private set; }
    public EasyAccessRepositoryAdapter<string, DateTime, ITaskItemWrapper, int> Outlook { get; private set; }
    public EasyAccessRepositoryAdapter<string, string, Google.Apis.Tasks.v1.Data.Task, int> Server { get; private set; }

    public override async Task DeleteAllEntites ()
    {
      await Outlook.DeleteAllEntities ();
      await Server.DeleteAllEntities ();
    }

    public GoogleTaskTestSynchronizer(Options options, TestComponentContainer testComponentContainer) : base(options, testComponentContainer)
    {
    }

    protected override async Task<IOutlookSynchronizer> InitializeOverride()
    {
      var synchronizerWithComponents = await TestComponentContainer.SynchronizerFactory.CreateSynchronizerWithComponents(Options, TestComponentContainer.GeneralOptions);

      var components = (AvailableGoogleTaskApiSynchronizerComponents) synchronizerWithComponents.Item2;

      var synchronizer = synchronizerWithComponents.Item1;
      Components = components;
      Outlook = EasyAccessRepositoryAdapter.Create(components.OutlookRepository, NullSynchronizationContextFactory.Instance);
      Server = EasyAccessRepositoryAdapter.Create(components.ServerRepository, NullSynchronizationContextFactory.Instance);
      return synchronizer;
    }
    
  }
}