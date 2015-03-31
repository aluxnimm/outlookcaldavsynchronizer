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
using System.Collections.Generic;
using System.Reflection;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.Generic.EntityMapping;
using CalDavSynchronizer.Generic.EntityRelationManagement;
using CalDavSynchronizer.Generic.InitialEntityMatching;
using CalDavSynchronizer.Generic.Synchronization;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation
{
  public class OutlookCalDavEventContext : ISynchronizerContext<string, DateTime, AppointmentItem, Uri, string, IEvent>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);


    private readonly IEntityRelationDataAccess<string, DateTime, Uri, string> _storageDataAccess;
    private readonly AppointmentEventEntityMapper _entityMapper;
    private readonly OutlookAppoointmentRepository _atypeRepository;
    private readonly CalDavEventRepository _btypeRepository;
    private readonly IEntityRelationDataFactory<string, DateTime, Uri, string> _entityRelationDataFactory;


    public OutlookCalDavEventContext (NameSpace outlookSession, IEntityRelationDataAccess<string, DateTime, Uri, string> storageDataAccess, Options options, string outlookEmailAddress, TimeSpan connectTimeout, TimeSpan readWriteTimeout)
    {
      if (outlookSession == null)
        throw new ArgumentNullException ("outlookSession");

      SynchronizationMode = options.SynchronizationMode;
      From = DateTime.Now.AddDays (-options.DaysToSynchronizeInThePast);
      To = DateTime.Now.AddDays (options.DaysToSynchronizeInTheFuture);

      _entityRelationDataFactory = new OutlookEventRelationDataFactory();

      _entityMapper = new AppointmentEventEntityMapper (outlookEmailAddress, new Uri ("mailto:" + options.EmailAddress));

      var calendarFolder = outlookSession.GetFolderFromID (options.OutlookFolderEntryId, options.OutlookFolderStoreId);
      _atypeRepository = new OutlookAppoointmentRepository (calendarFolder, outlookSession);

      _btypeRepository = new CalDavEventRepository (
          new CalDavDataAccess (
              new Uri (options.CalenderUrl),
              options.UserName,
              options.Password,
              connectTimeout,
              readWriteTimeout
          ),
          new iCalendarSerializer());

      _storageDataAccess = storageDataAccess;

      InitialEntityMatcher = new OutlookCalDavInitialEntityMatcher();
    }


    public IEntityMapper<AppointmentItem, IEvent> EntityMapper
    {
      get { return _entityMapper; }
    }

    public IEntityRepository<AppointmentItem, string, DateTime> AtypeRepository
    {
      get { return _atypeRepository; }
    }

    public IEntityRepository<IEvent, Uri, string> BtypeRepository
    {
      get { return _btypeRepository; }
    }

    public SynchronizationMode SynchronizationMode { get; private set; }
    public DateTime From { get; private set; }
    public DateTime To { get; private set; }
    public IInitialEntityMatcher<AppointmentItem, string, DateTime, IEvent, Uri, string> InitialEntityMatcher { get; private set; }

    public IEntityRelationDataFactory<string, DateTime, Uri, string> EntityRelationDataFactory
    {
      get { return _entityRelationDataFactory; }
    }


    public IEnumerable<IEntityRelationData<string, DateTime, Uri, string>> Load ()
    {
      return _storageDataAccess.Load();
    }

    public void Save (List<IEntityRelationData<string, DateTime, Uri, string>> data)
    {
      _storageDataAccess.Save (data);
    }
  }
}