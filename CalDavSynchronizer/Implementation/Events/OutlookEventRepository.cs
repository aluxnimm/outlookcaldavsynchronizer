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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer.Implementation.Events
{
  public class OutlookEventRepository : IEntityRepository<AppointmentId, DateTime, IAppointmentItemWrapper, IEventSynchronizationContext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly IOutlookSession _session;
    private readonly string _folderId;
    private readonly string _folderStoreId;
    private readonly IDateTimeRangeProvider _dateTimeRangeProvider;
    private readonly EventMappingConfiguration _configuration;
    private readonly IDaslFilterProvider _daslFilterProvider;
    private readonly IQueryOutlookAppointmentItemFolderStrategy _queryFolderStrategy;
    private readonly IComWrapperFactory _comWrapperFactory;
    private readonly bool _useDefaultFolderItemType;

    public OutlookEventRepository (
      IOutlookSession session, 
      string folderId, 
      string folderStoreId, 
      IDateTimeRangeProvider dateTimeRangeProvider,
      EventMappingConfiguration configuration,
      IDaslFilterProvider daslFilterProvider,
      IQueryOutlookAppointmentItemFolderStrategy queryFolderStrategy, 
      IComWrapperFactory comWrapperFactory,
      bool useDefaultFolderItemType)
    {
      if (session == null)
        throw new ArgumentNullException (nameof (session));
      if (dateTimeRangeProvider == null)
        throw new ArgumentNullException (nameof (dateTimeRangeProvider));
      if (configuration == null)
        throw new ArgumentNullException (nameof (configuration));
      if (daslFilterProvider == null)
        throw new ArgumentNullException (nameof (daslFilterProvider));
      if (queryFolderStrategy == null) throw new ArgumentNullException(nameof(queryFolderStrategy));
      if (comWrapperFactory == null) throw new ArgumentNullException(nameof(comWrapperFactory));

      _session = session;
      _folderId = folderId;
      _folderStoreId = folderStoreId;
      _dateTimeRangeProvider = dateTimeRangeProvider;
      _configuration = configuration;
      _daslFilterProvider = daslFilterProvider;
      _queryFolderStrategy = queryFolderStrategy;
      _comWrapperFactory = comWrapperFactory;
      _useDefaultFolderItemType = useDefaultFolderItemType;
    }

    private GenericComObjectWrapper<Folder> CreateFolderWrapper ()
    {
      return GenericComObjectWrapper.Create ((Folder) _session.GetFolderFromId (_folderId, _folderStoreId));
    }

    public Task<IEnumerable<EntityVersion<AppointmentId, DateTime>>> GetVersions (IEnumerable<IdWithAwarenessLevel<AppointmentId>> idsOfEntitiesToQuery, IEventSynchronizationContext context, IGetVersionsLogger logger)
    {
      var result = new List<EntityVersion<AppointmentId, DateTime>>();

      foreach (var id in idsOfEntitiesToQuery)
      {
        var appointment = _session.GetAppointmentItemOrNull (id.Id.EntryId, _folderId, _folderStoreId);
        if (appointment != null)
        {
          try
          {
            if (_configuration.IsCategoryFilterSticky && id.IsKnown || DoesMatchCategoryCriterion (appointment))
            {
              result.Add (EntityVersion.Create (id.Id, appointment.LastModificationTime));
              context.DuplicateEventCleaner.AnnounceAppointment (AppointmentSlim.FromAppointmentItem(appointment));
            }
          }
          finally
          {
              Marshal.FinalReleaseComObject (appointment);
          }
        }
      }

      return Task.FromResult<IEnumerable<EntityVersion<AppointmentId, DateTime>>> ( result);
    }
    
    private bool DoesMatchCategoryCriterion (AppointmentItem item)
    {
      if (!_configuration.UseEventCategoryAsFilter)
        return true;

      var categoryCsv = item.Categories;

      if (string.IsNullOrEmpty (categoryCsv))
        return _configuration.InvertEventCategoryFilter || _configuration.IncludeEmptyEventCategoryFilter;

      var found = item.Categories
          .Split (new[] { CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries)
          .Select (c => c.Trim())
          .Any (c => c == _configuration.EventCategory);
      return _configuration.InvertEventCategoryFilter ? !found : found;
    }

    public async Task<IEnumerable<EntityVersion<AppointmentId, DateTime>>> GetAllVersions (IEnumerable<AppointmentId> idsOfknownEntities, IEventSynchronizationContext context, IGetVersionsLogger logger)
    {
      var all =  await GetAll (idsOfknownEntities,context, logger);
      
      foreach (var appointment in all)
        context.DuplicateEventCleaner.AnnounceAppointment(appointment);

      return all.Select(a => a.Version).ToList();
    }

    private Task<IReadOnlyList<AppointmentSlim>> GetAll (IEnumerable<AppointmentId> idsOfknownEntities, IEventSynchronizationContext context, IGetVersionsLogger logger)
    {
      var range = _dateTimeRangeProvider.GetRange ();

      List<AppointmentSlim> events;
      using (var calendarFolderWrapper = CreateFolderWrapper ())
      {
        bool isInstantSearchEnabled = false;

        try
        {
          using (var store = GenericComObjectWrapper.Create (calendarFolderWrapper.Inner.Store))
          {
            if (store.Inner != null)
              isInstantSearchEnabled = store.Inner.IsInstantSearchEnabled;
          }
        }
        catch (COMException)
        {
          s_logger.Info ("Can't access IsInstantSearchEnabled property of store, defaulting to false.");
        }

        // Table Filtering in the MSDN: https://msdn.microsoft.com/EN-US/library/office/ff867581.aspx
        var filterBuilder = new StringBuilder (_daslFilterProvider.GetAppointmentFilter (isInstantSearchEnabled));

        if (range.HasValue)
          filterBuilder.AppendFormat (" And \"urn:schemas:calendar:dtstart\" < '{0}' And \"urn:schemas:calendar:dtend\" > '{1}'", ToOutlookDateString (range.Value.To), ToOutlookDateString (range.Value.From));
        if (_configuration.UseEventCategoryAsFilter)
        {
          AddCategoryFilter (filterBuilder, _configuration.EventCategory, _configuration.InvertEventCategoryFilter, _configuration.IncludeEmptyEventCategoryFilter);
        }

        s_logger.DebugFormat ("Using Outlook DASL filter: {0}", filterBuilder.ToString ());

        events = _queryFolderStrategy.QueryAppointmentFolder (_session, calendarFolderWrapper.Inner, filterBuilder.ToString(), logger);
      }

      if (_configuration.IsCategoryFilterSticky && _configuration.UseEventCategoryAsFilter)
      {
        var knownEntitesThatWereFilteredOut = idsOfknownEntities.Except (events.Select (e => e.Version.Id));
        events.AddRange (
            knownEntitesThatWereFilteredOut
                .Select (id => _session.GetAppointmentItemOrNull(id.EntryId, _folderId, _folderStoreId))
                .Where (i => i != null)
                .ToSafeEnumerable ()
                .Select (AppointmentSlim.FromAppointmentItem));
      }

      return Task.FromResult<IReadOnlyList<AppointmentSlim>> (events);
    }


    public static void AddCategoryFilter (StringBuilder filterBuilder, string category, bool negate, bool includeEmpty)
    {
      var negateFilter = negate ? "Not" : "";
      var emptyFilter = includeEmpty ? " Or \"urn:schemas-microsoft-com:office:office#Keywords\" is null" : "";
      filterBuilder.AppendFormat (" And "+ negateFilter + "(\"urn:schemas-microsoft-com:office:office#Keywords\" = '{0}'" + emptyFilter + ")", category.Replace ("'","''"));
    }

   


    private static readonly CultureInfo _currentCultureInfo = CultureInfo.CurrentCulture;

    private string ToOutlookDateString (DateTime value)
    {
      return value.ToString ("g", _currentCultureInfo);
    }

#pragma warning disable 1998
    public async Task<IEnumerable<EntityWithId<AppointmentId, IAppointmentItemWrapper>>> Get (ICollection<AppointmentId> ids, ILoadEntityLogger logger, IEventSynchronizationContext context)
#pragma warning restore 1998
    {
      return ids
        .Select(id => EntityWithId.Create(
          id,
          _comWrapperFactory.Create(
            _session.GetAppointmentItem(id.EntryId, _folderStoreId),
            entryId => _session.GetAppointmentItem(entryId, _folderStoreId))));
    }

    public async Task VerifyUnknownEntities (Dictionary<AppointmentId, DateTime> unknownEntites, IEventSynchronizationContext context)
    {
      foreach (var deletedId in await context.DuplicateEventCleaner.DeleteAnnouncedEventsIfDuplicates(unknownEntites.ContainsKey))
        unknownEntites.Remove(deletedId);
    }

    public void Cleanup(IAppointmentItemWrapper entity)
    {
      entity.Dispose();
    }
    
    public void Cleanup (IEnumerable<IAppointmentItemWrapper> entities)
    {
      foreach (var appointmentItemWrapper in entities)
        appointmentItemWrapper.Dispose ();
    }

    public async Task<EntityVersion<AppointmentId, DateTime>> TryUpdate (
        AppointmentId entityId,
        DateTime entityVersion,
        IAppointmentItemWrapper entityToUpdate,
        Func<IAppointmentItemWrapper, Task<IAppointmentItemWrapper>> entityModifier,
        IEventSynchronizationContext context, 
        IEntitySynchronizationLogger logger)
    {
      entityToUpdate = await entityModifier (entityToUpdate);
      entityToUpdate.Inner.Save();
      context.DuplicateEventCleaner.AnnounceAppointment (AppointmentSlim.FromAppointmentItem(entityToUpdate.Inner));

      var newAppointmentId = new AppointmentId(entityToUpdate.Inner.EntryID, entityToUpdate.Inner.GlobalAppointmentID);

      if (!entityId.Equals(newAppointmentId))
        context.DuplicateEventCleaner.AnnounceAppointmentDeleted(entityId);

      return new EntityVersion<AppointmentId, DateTime> (
        newAppointmentId, 
        entityToUpdate.Inner.LastModificationTime);
    }

    public Task<bool> TryDelete (AppointmentId entityId, DateTime version, IEventSynchronizationContext context, IEntitySynchronizationLogger logger)
    {
      var entityWithId = Get (new[] { entityId }, NullLoadEntityLogger.Instance, context).Result.SingleOrDefault();
      if (entityWithId == null)
        return Task.FromResult (true);

      using (var appointment = entityWithId.Entity)
      {
        context.DuplicateEventCleaner.AnnounceAppointmentDeleted (new AppointmentId (appointment.Inner.EntryID, appointment.Inner.GlobalAppointmentID));
        appointment.Inner.Delete();
      }
      return Task.FromResult (true);
    }

    public async Task<EntityVersion<AppointmentId, DateTime>> Create (Func<IAppointmentItemWrapper, Task<IAppointmentItemWrapper>> entityInitializer, IEventSynchronizationContext context)
    {
      IAppointmentItemWrapper newAppointmentItemWrapper;

      using (var folderWrapper = CreateFolderWrapper())
      {
        newAppointmentItemWrapper = _comWrapperFactory.Create (
            _useDefaultFolderItemType ? (AppointmentItem) folderWrapper.Inner.Items.Add()
                                      : (AppointmentItem) folderWrapper.Inner.Items.Add (OlItemType.olAppointmentItem),
            entryId => _session.GetAppointmentItem (entryId, _folderStoreId));
      }

      using (newAppointmentItemWrapper)
      {
        try
        {
          var initializedWrapper = await entityInitializer(newAppointmentItemWrapper);

          using (initializedWrapper)
          {
            initializedWrapper.SaveAndReload();
            context.DuplicateEventCleaner.AnnounceAppointment(AppointmentSlim.FromAppointmentItem(initializedWrapper.Inner));
            var result = new EntityVersion<AppointmentId, DateTime>(
              new AppointmentId(initializedWrapper.Inner.EntryID, initializedWrapper.Inner.GlobalAppointmentID),
              initializedWrapper.Inner.LastModificationTime);
            return result;
          }
        }
        catch
        {
          try
          {
            newAppointmentItemWrapper.Inner.Delete();
          }
          catch (System.Exception x)
          {
            s_logger.Error("Error while deleting leftover entity", x);
          }
          throw;
        }

 
      }
    }

    public static IAppointmentItemWrapper CreateNewAppointmentForTesting (MAPIFolder calendarFolder, NameSpace mapiNamespace, string folderStoreId)
    {
      return new AppointmentItemWrapper ((AppointmentItem) calendarFolder.Items.Add (OlItemType.olAppointmentItem), entryId => (AppointmentItem) mapiNamespace.GetItemFromID (entryId, folderStoreId));
    }

    public static IAppointmentItemWrapper GetOutlookEventForTesting (string id, NameSpace mapiNamespace, string folderStoreId)
    {
      return new AppointmentItemWrapper (
          (AppointmentItem) mapiNamespace.GetItemFromID (id, folderStoreId),
          entryId => (AppointmentItem) mapiNamespace.GetItemFromID (id, folderStoreId));
    }
  }
}