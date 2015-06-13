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
using System.Configuration;
using System.IO;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Generic.EntityRelationManagement;
using CalDavSynchronizer.Generic.ProgressReport;
using CalDavSynchronizer.Generic.Synchronization;
using CalDavSynchronizer.Generic.Synchronization.StateFactories;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.Implementation.Tasks;
using DDay.iCal;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Scheduling
{
  internal class SynchronizerFactory : ISynchronizerFactory
  {
    private readonly string _outlookEmailAddress;
    private readonly string _applicationDataDirectory;
    private readonly ITotalProgressFactory _totalProgressFactory;
    private readonly NameSpace _outlookSession;

    public SynchronizerFactory (string applicationDataDirectory, ITotalProgressFactory totalProgressFactory, NameSpace outlookSession)
    {
      _outlookEmailAddress = outlookSession.CurrentUser.Address;
      _applicationDataDirectory = applicationDataDirectory;
      _totalProgressFactory = totalProgressFactory;
      _outlookSession = outlookSession;
    }

    public ISynchronizer CreateSynchronizer (Options options)
    {
      var outlookFolder = (Folder) _outlookSession.GetFolderFromID (options.OutlookFolderEntryId, options.OutlookFolderStoreId);

      switch (outlookFolder.DefaultItemType)
      {
        case OlItemType.olAppointmentItem:
          return CreateEventSynchronizer (options);
        case OlItemType.olTaskItem:
          return CreateTaskSynchronizer (options);
        default:
          throw new NotSupportedException (
              string.Format (
                  "The folder '{0}' contains an item type ('{1}'), whis is not supported for synchronization",
                  outlookFolder.Name,
                  outlookFolder.DefaultItemType));
      }
    }

    private ISynchronizer CreateEventSynchronizer (Options options)
    {
      var storageDataDirectory = Path.Combine (
          _applicationDataDirectory,
          options.Id.ToString()
          );

      var storageDataAccess = new EntityRelationDataAccess<string, DateTime, OutlookEventRelationData, Uri, string> (storageDataDirectory);

      var synchronizationContext = new EventSynchronizationContext (
          _outlookSession,
          storageDataAccess,
          options,
          _outlookEmailAddress,
          TimeSpan.Parse (ConfigurationManager.AppSettings["calDavConnectTimeout"]),
          TimeSpan.Parse (ConfigurationManager.AppSettings["calDavReadWriteTimeout"])
          );

      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> (
          synchronizationContext.EntityMapper,
          synchronizationContext.AtypeRepository,
          synchronizationContext.BtypeRepository,
          synchronizationContext.EntityRelationDataFactory
          );


      return new Synchronizer<string, DateTime, AppointmentItemWrapper, Uri, string, IICalendar> (
          synchronizationContext,
          InitialEventSyncStateCreationStrategyFactory.Create (
              syncStateFactory,
              syncStateFactory.Environment,
              options.SynchronizationMode,
              options.ConflictResolution),
          _totalProgressFactory
          );
    }

    private ISynchronizer CreateTaskSynchronizer (Options options)
    {
      var storageDataDirectory = Path.Combine (
          _applicationDataDirectory,
          options.Id.ToString()
          );

      var storageDataAccess = new EntityRelationDataAccess<string, DateTime, OutlookEventRelationData, Uri, string> (storageDataDirectory);

      var synchronizationContext = new TaskSynchronizationContext (
          _outlookSession,
          storageDataAccess,
          options,
          TimeSpan.Parse (ConfigurationManager.AppSettings["calDavConnectTimeout"]),
          TimeSpan.Parse (ConfigurationManager.AppSettings["calDavReadWriteTimeout"]));

      var syncStateFactory = new EntitySyncStateFactory<string, DateTime, TaskItemWrapper, Uri, string, IICalendar> (
          synchronizationContext.EntityMapper,
          synchronizationContext.AtypeRepository,
          synchronizationContext.BtypeRepository,
          synchronizationContext.EntityRelationDataFactory);

      return new Synchronizer<string, DateTime, TaskItemWrapper, Uri, string, IICalendar> (
          synchronizationContext,
          InitialTaskSyncStateCreationStrategyFactory.Create (
              syncStateFactory,
              syncStateFactory.Environment,
              options.SynchronizationMode,
              options.ConflictResolution),
          _totalProgressFactory);
    }
  }
}