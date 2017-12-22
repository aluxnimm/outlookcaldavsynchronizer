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
using Microsoft.Office.Interop.Outlook;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests.TestBase
{
  public class EventTestSynchronizer : TestSynchronizerBase
  {
    public AvailableEventSynchronizerComponents Components { get; private set; }
    public EasyAccessRepositoryAdapter<AppointmentId, DateTime, IAppointmentItemWrapper, IEventSynchronizationContext> Outlook { get; private set; }
    public EasyAccessRepositoryAdapter<WebResourceName, string, IICalendar, IEventSynchronizationContext> Server { get; private set; }

    public override async Task DeleteAllEntites ()
    {
      await Outlook.DeleteAllEntities ();
      await Server.DeleteAllEntities ();
    }

    public EventTestSynchronizer(Options options, TestComponentContainer testComponentContainer) : base(options, testComponentContainer)
    {
    }

    protected override async Task<IOutlookSynchronizer> InitializeOverride()
    {
      var synchronizerWithComponents = await TestComponentContainer.SynchronizerFactory.CreateSynchronizerWithComponents(Options, TestComponentContainer.GeneralOptions);

      var components = (AvailableEventSynchronizerComponents) synchronizerWithComponents.Item2;

      var synchronizer = synchronizerWithComponents.Item1;
      Components = components;
      Outlook = EasyAccessRepositoryAdapter.Create(components.OutlookEventRepository, new SynchronizationContextFactory<IEventSynchronizationContext>(() => NullEventSynchronizationContext.Instance));
      Server = EasyAccessRepositoryAdapter.Create(components.CalDavRepository, new SynchronizationContextFactory<IEventSynchronizationContext>(() => NullEventSynchronizationContext.Instance));
      return synchronizer;
    }

    public async Task<AppointmentId> CreateEventInOutlook (
    string subject,
    DateTime start,
    DateTime end,
    bool isAllDayEvent = false,
    Action<IAppointmentItemWrapper> initializer = null)
    {
      return await Outlook.CreateEntity (
        e =>
        {
          e.Inner.Start = start;
          e.Inner.End = end;
          e.Inner.Subject = subject;
          e.Inner.ReminderSet = false;
          e.Inner.AllDayEvent = isAllDayEvent;
          initializer?.Invoke(e);
        });
    }

    public async Task<WebResourceName> CreateEventOnServer(
      string subject,
      DateTime start,
      DateTime end,
      Action<Event> initializer = null)
    {
      return await Server.CreateEntity(
        c =>
        {
          var evt = new Event();
          evt.Start = new iCalDateTime(start);
          evt.End = new iCalDateTime(end);
          evt.Summary = subject;

          initializer?.Invoke(evt);

          c.Events.Add(evt);
        });
    }

  }
}