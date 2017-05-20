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
using System.IO;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.Scheduling.ComponentCollectors;
using CalDavSynchronizer.Synchronization;
using DDay.iCal;
using GenSync.Synchronization;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests.TestBase
{
  public class TaskTestSynchronizer : TestSynchronizerBase
  {
    public AvailableTaskSynchronizerComponents Components { get; private set; }
    public EasyAccessRepositoryAdapter<string, DateTime, ITaskItemWrapper, int> Outlook { get; private set; }
    public EasyAccessRepositoryAdapter<WebResourceName, string, IICalendar, int> Server { get; private set; }

    public override async Task DeleteAllEntites ()
    {
      await Outlook.DeleteAllEntities ();
      await Server.DeleteAllEntities ();
    }

    public TaskTestSynchronizer(Options options, TestComponentContainer testComponentContainer) : base(options, testComponentContainer)
    {
    }

    protected override async Task<IOutlookSynchronizer> InitializeOverride()
    {
      var synchronizerWithComponents = await TestComponentContainer.SynchronizerFactory.CreateSynchronizerWithComponents(Options, TestComponentContainer.GeneralOptions);

      var components = (AvailableTaskSynchronizerComponents) synchronizerWithComponents.Item2;

      var synchronizer = synchronizerWithComponents.Item1;
      Components = components;
      Outlook = EasyAccessRepositoryAdapter.Create(components.OutlookRepository, NullSynchronizationContextFactory.Instance);
      Server = EasyAccessRepositoryAdapter.Create(components.CalDavRepository, NullSynchronizationContextFactory.Instance);
      return synchronizer;
    }

    public async Task<string> CreateInOutlook (string subject)
    {
      return await Outlook.CreateEntity (
        e =>
        {
          e.Inner.Subject = subject;
          e.Inner.ReminderSet = false;
        });
    }

    public async Task<WebResourceName> CreateOnServer(string subject, Action<Todo> initializer = null)
    {
      return await Server.CreateEntity(
        c =>
        {
          var todo = new Todo();
          todo.Summary = subject;

          initializer?.Invoke(todo);

          c.Todos.Add(todo);
        });
    }

  }
}