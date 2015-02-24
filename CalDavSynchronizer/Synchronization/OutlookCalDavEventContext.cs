// This file is Part of CalDavSynchronizer (https://sourceforge.net/projects/outlookcaldavsynchronizer/)
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
using System.Reflection;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.EntityMapping;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.InitialEntityMatching;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Synchronization
{
  public class OutlookCalDavEventContext : ISynchronizerContext<AppointmentItem, string, DateTime, IEvent, Uri, string>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);


    private readonly IStorageDataAccess<string, DateTime, Uri, string> _storageDataAccess;
    private readonly EntityMapper _entityMapper;
    private readonly OutlookAppoointmentRepository _atypeRepository;
    private readonly CalDavEventRepository _btypeRepository;


    public OutlookCalDavEventContext (NameSpace outlookSession, IStorageDataAccess<string, DateTime, Uri, string> storageDataAccess, Options options, string outlookEmailAddress)
    {
      if (outlookSession == null)
        throw new ArgumentNullException ("outlookSession");

      SynchronizationMode = options.SynchronizationMode;
      From = DateTime.Now.AddDays (-options.DaysToSynchronizeInThePast);
      To = DateTime.Now.AddDays (options.DaysToSynchronizeInTheFuture);

      _entityMapper = new EntityMapper (outlookEmailAddress, new Uri ("mailto:" + options.EmailAddress));

      var calendarFolder = outlookSession.GetFolderFromID (options.OutlookFolderEntryId, options.OutlookFolderStoreId);
      _atypeRepository = new OutlookAppoointmentRepository (new OutlookDataAccess (calendarFolder));

      _btypeRepository = new CalDavEventRepository (new CalDavDataAccess (new Uri (options.CalenderUrl), options.UserName, options.Password), new iCalendarSerializer());

      _storageDataAccess = storageDataAccess;

      InitialEntityMatcher = new OutlookCalDavInitialEntityMatcher();
    }


    public IEntityMapper<AppointmentItem, IEvent> EntityMapper
    {
      get { return _entityMapper; }
    }

    public EntityRepositoryBase<AppointmentItem, string, DateTime> AtypeRepository
    {
      get { return _atypeRepository; }
    }

    public EntityRepositoryBase<IEvent, Uri, string> BtypeRepository
    {
      get { return _btypeRepository; }
    }

    public SynchronizationMode SynchronizationMode { get; private set; }
    public DateTime From { get; private set; }
    public DateTime To { get; private set; }
    public InitialEntityMatcher<AppointmentItem, string, DateTime, IEvent, Uri, string> InitialEntityMatcher { get; private set; }

    public void SaveChaches (EntityCaches<string, DateTime, Uri, string> caches)
    {
      _storageDataAccess.SaveChaches (caches);
    }

    public EntityCaches<string, DateTime, Uri, string> LoadOrCreateCaches (out bool cachesWereCreatedNew)
    {
      return _storageDataAccess.LoadOrCreateCaches (out cachesWereCreatedNew);
    }
  }
}