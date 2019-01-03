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
using System.Linq;
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using Microsoft.Office.Interop.Outlook;
using System.Runtime.InteropServices;
using CalDavSynchronizer.Implementation.Common;
using log4net;

namespace CalDavSynchronizer.Implementation.Contacts
{
  public class OutlookContactRepository<Tcontext> : IEntityRepository<string, DateTime, IContactItemWrapper, Tcontext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodInfo.GetCurrentMethod ().DeclaringType);

    private readonly IOutlookSession _session;
    private readonly string _folderId;
    private readonly string _folderStoreId;
    private readonly IDaslFilterProvider _daslFilterProvider;
    private readonly IQueryOutlookContactItemFolderStrategy _queryFolderStrategy;
    private readonly IComWrapperFactory _comWrapperFactory;
    private readonly bool _useDefaultFolderItemType;
    
    private const string PR_ASSOCIATED_BIRTHDAY_APPOINTMENT_ID = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/804D0102";
    private const string PR_ASSOCIATED_ANNIVERSARY_APPOINTMENT_ID = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/804E0102";

    public OutlookContactRepository (IOutlookSession session, string folderId, string folderStoreId, IDaslFilterProvider daslFilterProvider, IQueryOutlookContactItemFolderStrategy queryFolderStrategy, IComWrapperFactory comWrapperFactory, bool useDefaultFolderItemType)
    {
      if (session == null)
        throw new ArgumentNullException (nameof (session));
      if (daslFilterProvider == null)
        throw new ArgumentNullException (nameof (daslFilterProvider));
      if (queryFolderStrategy == null) throw new ArgumentNullException(nameof(queryFolderStrategy));
      if (comWrapperFactory == null) throw new ArgumentNullException(nameof(comWrapperFactory));

      _session = session;
      _folderId = folderId;
      _folderStoreId = folderStoreId;
      _daslFilterProvider = daslFilterProvider;
      _queryFolderStrategy = queryFolderStrategy;
      _comWrapperFactory = comWrapperFactory;
      _useDefaultFolderItemType = useDefaultFolderItemType;
    }

    private GenericComObjectWrapper<Folder> CreateFolderWrapper ()
    {
      return GenericComObjectWrapper.Create (_session.GetFolderFromId (_folderId, _folderStoreId));
    }

    public Task<IEnumerable<EntityVersion<string, DateTime>>> GetVersions (IEnumerable<IdWithAwarenessLevel<string>> idsOfEntitiesToQuery, Tcontext context, IGetVersionsLogger logger)
    {
      return Task.FromResult<IEnumerable<EntityVersion<string, DateTime>>> (
          idsOfEntitiesToQuery
              .Select (id => _session.GetContactItemOrNull (id.Id, _folderId, _folderStoreId))
              .Where (e => e != null)
              .ToSafeEnumerable ()
              .Select (c => EntityVersion.Create (c.EntryID, c.LastModificationTime))
              .ToList ());
    }

    public Task<IEnumerable<EntityVersion<string, DateTime>>> GetAllVersions (IEnumerable<string> idsOfknownEntities, Tcontext context, IGetVersionsLogger logger)
    {
      using (var addressbookFolderWrapper = CreateFolderWrapper())
      {
        bool isInstantSearchEnabled = false;

        try
        {
          using (var store = GenericComObjectWrapper.Create(addressbookFolderWrapper.Inner.Store))
          {
            if (store.Inner != null)
              isInstantSearchEnabled = store.Inner.IsInstantSearchEnabled;
          }
        }
        catch (COMException)
        {
          s_logger.Info("Can't access IsInstantSearchEnabled property of store, defaulting to false.");
        }

        var filter = _daslFilterProvider.GetContactFilter(isInstantSearchEnabled);

        return Task.FromResult<IEnumerable<EntityVersion<string, DateTime>>>(_queryFolderStrategy.QueryContactItemFolder(_session, addressbookFolderWrapper.Inner, _folderId, filter, logger));
      }
    }

    private static readonly CultureInfo _currentCultureInfo = CultureInfo.CurrentCulture;

    private string ToOutlookDateString (DateTime value)
    {
      return value.ToString ("g", _currentCultureInfo);
    }

#pragma warning disable 1998
    public async Task<IEnumerable<EntityWithId<string, IContactItemWrapper>>> Get (ICollection<string> ids, ILoadEntityLogger logger, Tcontext context)
#pragma warning restore 1998
    {
      return ids
        .Select(id => EntityWithId.Create(
          id,
          _comWrapperFactory.Create(
            _session.GetContactItem(id, _folderStoreId),
            entryId => _session.GetContactItem(entryId, _folderStoreId))));
    }

    public Task VerifyUnknownEntities (Dictionary<string, DateTime> unknownEntites, Tcontext context)
    {
      return Task.FromResult (0);
    }

    public void Cleanup(IContactItemWrapper entity)
    {
      entity.Dispose();
    }

    public void Cleanup(IEnumerable<IContactItemWrapper> entities)
    {
      foreach (var contactItemWrapper in entities)
        contactItemWrapper.Dispose();
    }

    public async Task<EntityVersion<string, DateTime>> TryUpdate (
      string entityId,
      DateTime entityVersion,
      IContactItemWrapper entityToUpdate,
      Func<IContactItemWrapper, Task<IContactItemWrapper>> entityModifier,
      Tcontext context, 
      IEntitySynchronizationLogger logger)
    {
      entityToUpdate = await entityModifier (entityToUpdate);
      entityToUpdate.Inner.Save ();
      return new EntityVersion<string, DateTime> (entityToUpdate.Inner.EntryID, entityToUpdate.Inner.LastModificationTime);
    }

    public Task<bool> TryDelete (
      string entityId,
      DateTime version,
      Tcontext context, 
      IEntitySynchronizationLogger logger)
    {
      var entityWithId = Get (new[] { entityId }, NullLoadEntityLogger.Instance, default (Tcontext)).Result.SingleOrDefault ();
      if (entityWithId == null)
        return Task.FromResult (true);

      using (var contact = entityWithId.Entity)
      {
        if (!contact.Inner.Anniversary.Equals (OutlookUtility.OUTLOOK_DATE_NONE))
        {
          try
          {
            Byte[] ba = contact.Inner.GetPropertySafe (PR_ASSOCIATED_ANNIVERSARY_APPOINTMENT_ID);
            string anniversaryAppointmentItemID = BitConverter.ToString (ba).Replace ("-", string.Empty);
            IAppointmentItemWrapper anniveraryWrapper = _comWrapperFactory.Create ( _session.GetAppointmentItem (anniversaryAppointmentItemID),
                                                                                    entryId => _session.GetAppointmentItem (anniversaryAppointmentItemID));
            anniveraryWrapper.Inner.Delete();
          }
          catch (COMException ex)
          {
            s_logger.Error ("Could not delete associated Anniversary Appointment.", ex);
            logger.LogError ("Could not delete associated Anniversary Appointment.", ex);
          }
        }
        if (!contact.Inner.Birthday.Equals (OutlookUtility.OUTLOOK_DATE_NONE))
        {
          try
          {
            Byte[] ba = contact.Inner.GetPropertySafe (PR_ASSOCIATED_BIRTHDAY_APPOINTMENT_ID);
            string birthdayAppointmentItemID = BitConverter.ToString (ba).Replace ("-", string.Empty);
            IAppointmentItemWrapper birthdayWrapper = _comWrapperFactory.Create ( _session.GetAppointmentItem (birthdayAppointmentItemID),
                                                                                  entryId =>  _session.GetAppointmentItem (birthdayAppointmentItemID));
            birthdayWrapper.Inner.Delete ();
          }
          catch (COMException ex)
          {
            s_logger.Error ("Could not delete associated Birthday Appointment.", ex);
            logger.LogError ("Could not delete associated Birthday Appointment.", ex);
          }
        }
        contact.Inner.Delete ();
      }

      return Task.FromResult (true);
    }

    public async Task<EntityVersion<string, DateTime>> Create (Func<IContactItemWrapper, Task<IContactItemWrapper>> entityInitializer, Tcontext context)
    {
      IContactItemWrapper newWrapper;

      using (var folderWrapper = CreateFolderWrapper ())
      {
        newWrapper = _comWrapperFactory.Create (
          _useDefaultFolderItemType ? (ContactItem) folderWrapper.Inner.Items.Add()
                                    : (ContactItem) folderWrapper.Inner.Items.Add (OlItemType.olContactItem),
          entryId => (ContactItem) _session.GetContactItem (entryId, _folderStoreId));
      }

      using (newWrapper)
      {
        IContactItemWrapper initializedWrapper;

        try
        {
          initializedWrapper = await entityInitializer(newWrapper);
        }
        catch
        {
          try
          {
            newWrapper.Inner.Delete();
          }
          catch (System.Exception x)
          {
            s_logger.Error("Error while deleting leftover entity", x);
          }
          throw;
        }

        using (initializedWrapper)
        {
          initializedWrapper.SaveAndReload ();
          var result = new EntityVersion<string, DateTime> (initializedWrapper.Inner.EntryID, initializedWrapper.Inner.LastModificationTime);
          return result;
        }
      }
    }
  }
}