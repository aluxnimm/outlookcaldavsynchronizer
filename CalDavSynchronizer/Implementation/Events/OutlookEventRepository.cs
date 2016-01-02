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
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.Events
{
  public class OutlookEventRepository : IEntityRepository<AppointmentItemWrapper, string, DateTime>, IOutlookRepository
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly NameSpace _mapiNameSpace;
    private readonly string _folderId;
    private readonly string _folderStoreId;
    private readonly IDateTimeRangeProvider _dateTimeRangeProvider;
    private readonly EventMappingConfiguration _configuration;

    public const string PR_MESSAGE_CLASS_DASLFILTER = "@SQL=\"http://schemas.microsoft.com/mapi/proptag/0x001A001E\" = 'IPM.Appointment'";

    public OutlookEventRepository (
      NameSpace mapiNameSpace, 
      string folderId, 
      string folderStoreId, 
      IDateTimeRangeProvider dateTimeRangeProvider,
      EventMappingConfiguration configuration)
    {
      if (mapiNameSpace == null)
        throw new ArgumentNullException ("mapiNameSpace");

      _mapiNameSpace = mapiNameSpace;
      _folderId = folderId;
      _folderStoreId = folderStoreId;
      _dateTimeRangeProvider = dateTimeRangeProvider;
      _configuration = configuration;
    }

    private const string c_entryIdColumnName = "EntryID";


    private GenericComObjectWrapper<Folder> CreateFolderWrapper ()
    {
      return GenericComObjectWrapper.Create ((Folder) _mapiNameSpace.GetFolderFromID (_folderId, _folderStoreId));
    }

    public Task<IReadOnlyList<EntityVersion<string, DateTime>>> GetVersions (IEnumerable<IdWithAwarenessLevel<string>> idsOfEntitiesToQuery)
    {
      var result = new List<EntityVersion<string, DateTime>>();

      foreach (var id in idsOfEntitiesToQuery)
      {
        var appointment = GetAppointmentItemOrNull (id.Id);
        if (appointment != null)
        {
          try
          {
            if (id.IsKnown || DoesMatchCategoryCriterion (appointment))
              result.Add (EntityVersion.Create (id.Id, appointment.LastModificationTime));
          }
          finally
          {
              Marshal.FinalReleaseComObject (appointment);
          }
        }
      }

      return Task.FromResult<IReadOnlyList<EntityVersion<string, DateTime>>> ( result);
    }

    private AppointmentItem GetAppointmentItemOrNull (string id)
    {
      try
      {
        var item = (AppointmentItem) _mapiNameSpace.GetItemFromID (id, _folderStoreId);
        return item;
      }
      catch (COMException x)
      {
        const int messageNotFoundResult = -2147221233;
        if (x.HResult != messageNotFoundResult)
          s_logger.Error ("Error while fetching entity.", x);
        return null;
      }
    }
    
    private bool DoesMatchCategoryCriterion (AppointmentItem item)
    {
      if (!_configuration.UseEventCategoryAsFilter)
        return true;

      var categoryCsv = item.Categories;

      if (string.IsNullOrEmpty (categoryCsv))
        return false;

     return  item.Categories
          .Split (new[] { CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries)
          .Select (c => c.Trim())
          .Any (c => c == _configuration.EventCategory);
    }

    public Task<IReadOnlyList<EntityVersion<string, DateTime>>> GetAllVersions (IEnumerable<string> idsOfknownEntities)
    {
      var range = _dateTimeRangeProvider.GetRange();

      // Table Filtering in the MSDN: https://msdn.microsoft.com/EN-US/library/office/ff867581.aspx
      var filterBuilder = new StringBuilder (PR_MESSAGE_CLASS_DASLFILTER);

      if (range.HasValue)
        filterBuilder.AppendFormat (" And \"urn:schemas:calendar:dtstart\" < '{0}' And \"urn:schemas:calendar:dtend\" > '{1}'", ToOutlookDateString(range.Value.To), ToOutlookDateString(range.Value.From));
      if (_configuration.UseEventCategoryAsFilter)
      {
        AddCategoryFilter (filterBuilder, _configuration.EventCategory);
      }
      s_logger.InfoFormat("Using Outlook DASL filter: {0}", filterBuilder.ToString());

      List<EntityVersion<string, DateTime>> events;
      using (var calendarFolderWrapper = CreateFolderWrapper())
      {
        events = QueryFolder (_mapiNameSpace, calendarFolderWrapper, filterBuilder);
      }

      if (_configuration.UseEventCategoryAsFilter)
      {
        var knownEntitesThatWereFilteredOut = idsOfknownEntities.Except (events.Select (e => e.Id));
        events.AddRange (
            knownEntitesThatWereFilteredOut
                .Select (GetAppointmentItemOrNull)
                .Where (i => i != null)
                .ToSafeEnumerable()
                .Select (c => EntityVersion.Create (c.EntryID, c.LastModificationTime)));
      }

      return Task.FromResult<IReadOnlyList<EntityVersion<string, DateTime>>> (events);
    }

    public static void AddCategoryFilter (StringBuilder filterBuilder, string category)
    {
      filterBuilder.AppendFormat (" And \"urn:schemas-microsoft-com:office:office#Keywords\" = '{0}'", category.Replace ("'","''"));
    }

    public static List<EntityVersion<string, DateTime>> QueryFolder (NameSpace session, GenericComObjectWrapper<Folder> calendarFolderWrapper, StringBuilder filterBuilder)
    {
      var events = new List<EntityVersion<string, DateTime>>();

      using (var tableWrapper = GenericComObjectWrapper.Create (
          calendarFolderWrapper.Inner.GetTable (filterBuilder.Length > 0 ? filterBuilder.ToString() : Type.Missing)))
      {
        var table = tableWrapper.Inner;
        table.Columns.RemoveAll();
        table.Columns.Add (c_entryIdColumnName);

        var storeId = calendarFolderWrapper.Inner.StoreID;

        while (!table.EndOfTable)
        {
          var row = table.GetNextRow();
          var entryId = (string) row[c_entryIdColumnName];
          try
          {
            using (var appointmentWrapper = GenericComObjectWrapper.Create ((AppointmentItem) session.GetItemFromID (entryId, storeId)))
            {
              events.Add (new EntityVersion<string, DateTime> (appointmentWrapper.Inner.EntryID, appointmentWrapper.Inner.LastModificationTime));
            }
          }
          catch (COMException ex)
          {
            s_logger.Error ("Could not fetch AppointmentItem, skipping.", ex);
          }
        }
      }
      return events;
    }

    private static readonly CultureInfo _currentCultureInfo = CultureInfo.CurrentCulture;

    private string ToOutlookDateString (DateTime value)
    {
      return value.ToString ("g", _currentCultureInfo);
    }

#pragma warning disable 1998
    public async Task<IReadOnlyList<EntityWithId<string, AppointmentItemWrapper>>> Get (ICollection<string> ids, ILoadEntityLogger logger)
#pragma warning restore 1998
    {
      return ids
          .Select (id => EntityWithId.Create (
              id,
              new AppointmentItemWrapper (
                  (AppointmentItem) _mapiNameSpace.GetItemFromID (id, _folderStoreId),
                  entryId => (AppointmentItem) _mapiNameSpace.GetItemFromID (entryId, _folderStoreId))))
          .ToArray();
    }

    public void Cleanup (IReadOnlyDictionary<string, AppointmentItemWrapper> entities)
    {
      foreach (var appointmentItemWrapper in entities.Values)
        appointmentItemWrapper.Dispose();
    }

    public Task<EntityVersion<string, DateTime>> Update (
        string entityId,
        DateTime entityVersion,
        AppointmentItemWrapper entityToUpdate,
        Func<AppointmentItemWrapper, AppointmentItemWrapper> entityModifier)
    {
      entityToUpdate = entityModifier (entityToUpdate);
      entityToUpdate.Inner.Save();
      return Task.FromResult (new EntityVersion<string, DateTime> (entityToUpdate.Inner.EntryID, entityToUpdate.Inner.LastModificationTime));
    }

    public Task Delete (string entityId, DateTime version)
    {
      var entityWithId = Get (new[] { entityId }, NullSynchronizationLogger.Instance).Result.SingleOrDefault();
      if (entityWithId == null)
        return Task.FromResult (0);

      using (var appointment = entityWithId.Entity)
      {
        appointment.Inner.Delete();
      }
      return Task.FromResult (0);
    }

    public Task<EntityVersion<string, DateTime>> Create (Func<AppointmentItemWrapper, AppointmentItemWrapper> entityInitializer)
    {
      AppointmentItemWrapper newAppointmentItemWrapper;

      using (var folderWrapper = CreateFolderWrapper())
      {
        newAppointmentItemWrapper = new AppointmentItemWrapper (
            (AppointmentItem) folderWrapper.Inner.Items.Add (OlItemType.olAppointmentItem),
            entryId => (AppointmentItem) _mapiNameSpace.GetItemFromID (entryId, _folderStoreId));
      }

      using (newAppointmentItemWrapper)
      {
        using (var initializedWrapper = entityInitializer (newAppointmentItemWrapper))
        {
          initializedWrapper.SaveAndReload();
          var result = new EntityVersion<string, DateTime> (initializedWrapper.Inner.EntryID, initializedWrapper.Inner.LastModificationTime);
          return Task.FromResult (result);
        }
      }
    }

    public static AppointmentItemWrapper CreateNewAppointmentForTesting (MAPIFolder calendarFolder, NameSpace mapiNamespace, string folderStoreId)
    {
      return new AppointmentItemWrapper ((AppointmentItem) calendarFolder.Items.Add (OlItemType.olAppointmentItem), entryId => (AppointmentItem) mapiNamespace.GetItemFromID (entryId, folderStoreId));
    }


    public static AppointmentItemWrapper GetOutlookEventForTesting (string id, NameSpace mapiNamespace, string folderStoreId)
    {
      return new AppointmentItemWrapper (
          (AppointmentItem) mapiNamespace.GetItemFromID (id, folderStoreId),
          entryId => (AppointmentItem) mapiNamespace.GetItemFromID (id, folderStoreId));
    }

    public bool IsResponsibleForFolder (string folderEntryId, string folderStoreId)
    {
      return folderEntryId == _folderId && folderStoreId == _folderStoreId;
    }
  }
}