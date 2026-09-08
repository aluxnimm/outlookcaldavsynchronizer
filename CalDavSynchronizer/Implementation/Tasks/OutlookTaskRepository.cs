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
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using Microsoft.Office.Interop.Outlook;
using System.Runtime.InteropServices;
using System.Text;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.Events;
using log4net;

namespace CalDavSynchronizer.Implementation.Tasks
{
    public class OutlookTaskRepository : IEntityRepository<string, DateTime, ITaskItemWrapper, int>
    {
        private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
        private readonly IOutlookSession _session;
        private readonly string _folderId;
        private readonly string _folderStoreId;
        private readonly IDaslFilterProvider _daslFilterProvider;
        private readonly TaskMappingConfiguration _configuration;
        private readonly IQueryOutlookTaskItemFolderStrategy _queryFolderStrategy;
        private readonly IComWrapperFactory _comWrapperFactory;
        private readonly bool _useDefaultFolderItemType;
        private List<TaskSlim> _taskSnapshot = new List<TaskSlim>();
        public OutlookTaskRepository(IOutlookSession session, string folderId, string folderStoreId, IDaslFilterProvider daslFilterProvider, TaskMappingConfiguration configuration, IQueryOutlookTaskItemFolderStrategy queryFolderStrategy, IComWrapperFactory comWrapperFactory, bool useDefaultFolderItemType)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (daslFilterProvider == null)
                throw new ArgumentNullException(nameof(daslFilterProvider));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (queryFolderStrategy == null) throw new ArgumentNullException(nameof(queryFolderStrategy));
            if (comWrapperFactory == null) throw new ArgumentNullException(nameof(comWrapperFactory));
            if (String.IsNullOrEmpty(folderId))
                throw new ArgumentException("Argument is null or empty", nameof(folderId));
            if (String.IsNullOrEmpty(folderStoreId))
                throw new ArgumentException("Argument is null or empty", nameof(folderStoreId));

            _session = session;
            _folderId = folderId;
            _folderStoreId = folderStoreId;
            _daslFilterProvider = daslFilterProvider;
            _configuration = configuration;
            _queryFolderStrategy = queryFolderStrategy;
            _comWrapperFactory = comWrapperFactory;
            _useDefaultFolderItemType = useDefaultFolderItemType;
        }

        public Task<IEnumerable<EntityVersion<string, DateTime>>> GetVersions(IEnumerable<IdWithAwarenessLevel<string>> idsOfEntitiesToQuery, int context, IGetVersionsLogger logger)
        {
            var result = new List<EntityVersion<string, DateTime>>();

            foreach (var id in idsOfEntitiesToQuery)
            {
                var task = _session.GetTaskItemOrNull(id.Id, _folderId, _folderStoreId);
                if (task != null)
                {
                    try
                    {
                        if (_configuration.IsCategoryFilterSticky && id.IsKnown || DoesMatchCategoryCriterion(task))
                        {
                            result.Add(EntityVersion.Create(id.Id, task.LastModificationTime.ToUniversalTime()));
                        }
                    }
                    finally
                    {
                        Marshal.FinalReleaseComObject(task);
                    }
                }
            }

            return Task.FromResult<IEnumerable<EntityVersion<string, DateTime>>>(result);
        }

        private GenericComObjectWrapper<Folder> CreateFolderWrapper()
        {
            return GenericComObjectWrapper.Create((Folder) _session.GetFolderFromId(_folderId, _folderStoreId));
        }

        private List<TaskSlim> QueryTasks(IGetVersionsLogger logger)
        {
            using (var taskFolderWrapper = CreateFolderWrapper())
            {
                bool isInstantSearchEnabled = false;

                try
                {
                    using (var store = GenericComObjectWrapper.Create(taskFolderWrapper.Inner.Store))
                    {
                        if (store.Inner != null)
                            isInstantSearchEnabled = store.Inner.IsInstantSearchEnabled;
                    }
                }
                catch (COMException)
                {
                    s_logger.Info("Can't access IsInstantSearchEnabled property of store, defaulting to false.");
                }

                var filterBuilder = new StringBuilder(_daslFilterProvider.GetTaskFilter(isInstantSearchEnabled));
                if (_configuration.UseTaskCategoryAsFilter)
                {
                    OutlookEventRepository.AddCategoryFilter(
                        filterBuilder,
                        _configuration.TaskCategory,
                        _configuration.InvertTaskCategoryFilter,
                        _configuration.IncludeEmptyTaskCategoryFilter);
                }

                s_logger.DebugFormat("Using Outlook DASL filter: {0}", filterBuilder);

                return _queryFolderStrategy.QueryTaskFolder(
                    _session,
                    taskFolderWrapper.Inner,
                    filterBuilder.ToString(),
                    logger ?? NullGetVersionsLogger.Instance);
            }
        }

        private Dictionary<string, string> EnsureSeriesIds(
            List<TaskSlim> tasks,
            ISet<string> idsToInitialize = null)
        {
            var initializedIds = new Dictionary<string, string>(StringComparer.Ordinal);

            for (var i = 0; i < tasks.Count; i++)
            {
                var taskInfo = tasks[i];

                // Only recurring masters need their own persistent series id.
                // Outlook copies this user property to completed/dead occurrences.
                if (!taskInfo.IsRecurring ||
                    taskInfo.IsDeadOccurrence ||
                    !string.IsNullOrEmpty(taskInfo.SeriesId) ||
                    (idsToInitialize != null && !idsToInitialize.Contains(taskInfo.EntityVersion.Id)))
                {
                    continue;
                }

                var originalEntryId = taskInfo.EntityVersion.Id;
                var taskItem = _session.GetTaskItemOrNull(
                    originalEntryId,
                    _folderId,
                    _folderStoreId);

                if (taskItem == null)
                    continue;

                try
                {
                    var seriesId = RecurringTaskSeries.EnsureSeriesId(taskItem);
                    var currentEntryId = taskItem.EntryID;

                    tasks[i] = new TaskSlim(
                        new EntityVersion<string, DateTime>(
                            currentEntryId,
                            taskItem.LastModificationTime.ToUniversalTime()),
                        seriesId,
                        true,
                        false);

                    initializedIds[originalEntryId] = currentEntryId;
                }
                finally
                {
                    Marshal.FinalReleaseComObject(taskItem);
                }
            }

            return initializedIds;
        }

        private Dictionary<string, TaskSlim> GetUnambiguousSeriesRepresentatives(
            IEnumerable<TaskSlim> tasks)
        {
            var representatives =
                new Dictionary<string, TaskSlim>(StringComparer.Ordinal);

            var seriesGroups =
                tasks
                    .Where(t => !string.IsNullOrEmpty(t.SeriesId))
                    .GroupBy(t => t.SeriesId, StringComparer.Ordinal);

            foreach (var seriesGroup in seriesGroups)
            {
                var liveMasters =
                    seriesGroup
                        .Where(t => t.IsRecurring && !t.IsDeadOccurrence)
                        .ToList();

                if (liveMasters.Count == 1)
                {
                    representatives[seriesGroup.Key] = liveMasters[0];
                    continue;
                }

                if (liveMasters.Count > 1)
                {
                    s_logger.WarnFormat(
                        "Multiple recurring task masters share CalDavSynchronizerSeriesId '{0}'. " +
                        "Dead occurrences for this series will not be aggregated automatically.",
                        seriesGroup.Key);
                    continue;
                }

                // Outlook keeps completed occurrences of a recurring task as recurring
                // dead items. After the final occurrence there is no live master left,
                // so select the chronologically latest dead occurrence as the terminal
                // series representative. Earlier dead occurrences remain exceptions of
                // that same CalDAV resource.
                var terminalMasters =
                    seriesGroup
                        .Where(t => t.IsRecurring && t.IsDeadOccurrence)
                        .ToList();

                if (terminalMasters.Count == 1)
                {
                    representatives[seriesGroup.Key] = terminalMasters[0];
                    continue;
                }

                if (terminalMasters.Count == 0)
                    continue;

                var terminalMastersWithDate =
                    terminalMasters
                        .Select(t => new
                        {
                            Task = t,
                            Date = GetTaskOccurrenceDate(t)
                        })
                        .ToList();

                if (terminalMastersWithDate.Any(t => !t.Date.HasValue))
                {
                    s_logger.WarnFormat(
                        "Recurring dead occurrences for CalDavSynchronizerSeriesId '{0}' " +
                        "cannot be ordered because at least one has neither StartDate nor DueDate. " +
                        "They will not be aggregated automatically.",
                        seriesGroup.Key);
                    continue;
                }

                var latestDate =
                    terminalMastersWithDate.Max(t => t.Date.Value);

                var latestTerminalMasters =
                    terminalMastersWithDate
                        .Where(t => t.Date.Value == latestDate)
                        .Select(t => t.Task)
                        .ToList();

                if (latestTerminalMasters.Count == 1)
                {
                    representatives[seriesGroup.Key] = latestTerminalMasters[0];
                }
                else
                {
                    s_logger.WarnFormat(
                        "Multiple recurring dead occurrences share the latest occurrence date {0:d} " +
                        "for CalDavSynchronizerSeriesId '{1}'. The terminal recurring task is ambiguous " +
                        "and dead occurrences will not be aggregated automatically.",
                        latestDate,
                        seriesGroup.Key);
                }
            }

            return representatives;
        }

        private DateTime? GetTaskOccurrenceDate(TaskSlim taskInfo)
        {
            var task =
                _session.GetTaskItemOrNull(
                    taskInfo.EntityVersion.Id,
                    _folderId,
                    _folderStoreId);

            if (task == null)
                return null;

            try
            {
                if (task.StartDate != OutlookUtility.OUTLOOK_DATE_NONE)
                    return task.StartDate.Date;

                if (task.DueDate != OutlookUtility.OUTLOOK_DATE_NONE)
                    return task.DueDate.Date;

                return null;
            }
            finally
            {
                Marshal.FinalReleaseComObject(task);
            }
        }

        public Task<IEnumerable<EntityVersion<string, DateTime>>> GetAllVersions(
            IEnumerable<string> idsOfknownEntities,
            int context,
            IGetVersionsLogger logger)
        {
            var tasks = QueryTasks(logger);

            if (_configuration.IsCategoryFilterSticky && _configuration.UseTaskCategoryAsFilter)
            {
                var knownEntitiesThatWereFilteredOut =
                    idsOfknownEntities.Except(
                        tasks.Select(e => e.EntityVersion.Id));

                foreach (var id in knownEntitiesThatWereFilteredOut)
                {
                    var task = _session.GetTaskItemOrNull(
                        id,
                        _folderId,
                        _folderStoreId);

                    if (task == null)
                        continue;

                    try
                    {
                        tasks.Add(
                            new TaskSlim(
                                new EntityVersion<string, DateTime>(
                                    task.EntryID,
                                    task.LastModificationTime.ToUniversalTime()),
                                RecurringTaskSeries.GetSeriesId(task),
                                task.IsRecurring,
                                RecurringTaskSeries.IsDeadOccurrence(task)));
                    }
                    finally
                    {
                        Marshal.FinalReleaseComObject(task);
                    }
                }
            }

            // Existing profiles are migrated once here. New recurring tasks are also
            // initialized lazily by NormalizeIdsForPartialSync for immediate sync.
            EnsureSeriesIds(tasks);

            _taskSnapshot = tasks.ToList();

            var seriesRepresentatives =
                GetUnambiguousSeriesRepresentatives(tasks);

            var topLevelTasks =
                tasks.Where(t =>
                {
                    if (!t.IsDeadOccurrence || string.IsNullOrEmpty(t.SeriesId))
                        return true;

                    TaskSlim representative;
                    return !seriesRepresentatives.TryGetValue(t.SeriesId, out representative) ||
                           string.Equals(
                               representative.EntityVersion.Id,
                               t.EntityVersion.Id,
                               StringComparison.Ordinal);
                });

            return Task.FromResult<IEnumerable<EntityVersion<string, DateTime>>>(
                topLevelTasks.Select(t => t.EntityVersion));
        }

        public IEnumerable<IIdWithHints<string, DateTime>> NormalizeIdsForPartialSync(
            IEnumerable<IIdWithHints<string, DateTime>> ids,
            IGetVersionsLogger logger)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));

            var inputIds = ids.ToList();
            if (inputIds.Count == 0)
                return inputIds;

            // Keep the previous snapshot so a deletion notification for a dead occurrence
            // can still be associated with its recurring master after the item disappeared.
            var previousSnapshot = _taskSnapshot.ToList();
            var currentTasks = QueryTasks(logger);

            // A newly created recurring master can reach immediate sync before the next
            // full synchronization. Initialize only masters that are part of this partial
            // change set, avoiding a mass Save() operation during every partial sync.
            var inputIdSet = new HashSet<string>(
                inputIds.Select(i => i.Id),
                StringComparer.Ordinal);

            var initializedMasterIds = EnsureSeriesIds(currentTasks, inputIdSet);

            // Get(master) uses this snapshot to attach all current dead occurrences as
            // RECURRENCE-ID components to the master VTODO.
            _taskSnapshot = currentTasks.ToList();

            var currentById =
                currentTasks
                    .GroupBy(t => t.EntityVersion.Id, StringComparer.Ordinal)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

            var previousById =
                previousSnapshot
                    .GroupBy(t => t.EntityVersion.Id, StringComparer.Ordinal)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

            var representativeBySeriesId =
                GetUnambiguousSeriesRepresentatives(currentTasks);

            var resultById =
                new Dictionary<string, IIdWithHints<string, DateTime>>(StringComparer.Ordinal);
            var resultOrder = new List<string>();
            var forceReloadIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var inputId in inputIds)
            {
                var outputId = inputId.Id;
                var forceReload = false;

                string initializedEntryId;
                if (initializedMasterIds.TryGetValue(inputId.Id, out initializedEntryId))
                {
                    // EnsureSeriesId saved the Outlook item, so the incoming version hint
                    // is stale. Force GenSync to query the master's current version.
                    outputId = initializedEntryId;
                    forceReload = true;
                }
                else
                {
                    TaskSlim taskInfo;
                    if (!currentById.TryGetValue(inputId.Id, out taskInfo))
                        previousById.TryGetValue(inputId.Id, out taskInfo);

                    TaskSlim representative;
                    if (taskInfo != null &&
                        taskInfo.IsDeadOccurrence &&
                        !string.IsNullOrEmpty(taskInfo.SeriesId) &&
                        representativeBySeriesId.TryGetValue(
                            taskInfo.SeriesId,
                            out representative))
                    {
                        // The version/deletion hints belong to a completed occurrence, not
                        // necessarily to the item which currently represents the series.
                        // During the final occurrence there is no next live master, so the
                        // recurring dead occurrence itself becomes the series representative.
                        outputId = representative.EntityVersion.Id;
                        forceReload = true;
                    }
                }

                if (!resultById.ContainsKey(outputId))
                {
                    resultOrder.Add(outputId);
                    resultById[outputId] = inputId;
                }

                if (forceReload && forceReloadIds.Add(outputId))
                {
                    resultById[outputId] =
                        IdWithHints.Create<string, DateTime>(
                            outputId,
                            (DateTime?) null,
                            null);
                }
            }

            return resultOrder.Select(id => resultById[id]).ToList();
        }


        private static readonly CultureInfo _currentCultureInfo = CultureInfo.CurrentCulture;

        private string ToOutlookDateString(DateTime value)
        {
            return value.ToString("g", _currentCultureInfo);
        }

#pragma warning disable 1998
        public async Task<IEnumerable<EntityWithId<string, ITaskItemWrapper>>> Get(
            ICollection<string> ids,
            ILoadEntityLogger logger,
            int context)
#pragma warning restore 1998
        {
            var result =
                new List<EntityWithId<string, ITaskItemWrapper>>();

            var seriesRepresentatives =
                GetUnambiguousSeriesRepresentatives(_taskSnapshot);

            foreach (var id in ids)
            {
                var task =
                    _session.GetTaskItem(id, _folderStoreId);

                var wrapper =
                    _comWrapperFactory.Create(
                        task,
                        entryId =>
                            _session.GetTaskItem(
                                entryId,
                                _folderStoreId));

                try
                {
                    var seriesId =
                        RecurringTaskSeries.GetSeriesId(task);

                    if (!string.IsNullOrEmpty(seriesId))
                    {
                        TaskSlim representative;
                        if (seriesRepresentatives.TryGetValue(seriesId, out representative) &&
                            string.Equals(
                                representative.EntityVersion.Id,
                                task.EntryID,
                                StringComparison.Ordinal))
                        {
                            var recurrenceExceptions =
                                _taskSnapshot.Where(
                                    t =>
                                        t.IsDeadOccurrence &&
                                        !string.Equals(
                                            t.EntityVersion.Id,
                                            representative.EntityVersion.Id,
                                            StringComparison.Ordinal) &&
                                        string.Equals(
                                            t.SeriesId,
                                            seriesId,
                                            StringComparison.Ordinal));

                            foreach (var recurrenceExceptionInfo in recurrenceExceptions)
                            {
                                var recurrenceException =
                                    _session.GetTaskItemOrNull(
                                        recurrenceExceptionInfo.EntityVersion.Id,
                                        _folderId,
                                        _folderStoreId);

                                if (recurrenceException != null)
                                {
                                    wrapper.AddRecurrenceException(recurrenceException);
                                }
                            }
                        }
                    }

                    result.Add(
                        EntityWithId.Create(
                            id,
                            wrapper));
                }
                catch
                {
                    wrapper.Dispose();
                    throw;
                }
            }

            return result;
        }

        public Task VerifyUnknownEntities(Dictionary<string, DateTime> unknownEntites, int context)
        {
            return Task.FromResult(0);
        }

        private bool DoesMatchCategoryCriterion(TaskItem item)
        {
            if (!_configuration.UseTaskCategoryAsFilter)
                return true;

            var categoryCsv = item.Categories;

            if (string.IsNullOrEmpty(categoryCsv))
                return _configuration.InvertTaskCategoryFilter || _configuration.IncludeEmptyTaskCategoryFilter;

            var found = item.Categories
                .Split(new[] {CultureInfo.CurrentCulture.TextInfo.ListSeparator}, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Any(c => c == _configuration.TaskCategory);
            return _configuration.InvertTaskCategoryFilter ? !found : found;
        }

        public void Cleanup(ITaskItemWrapper entity)
        {
            entity.Dispose();
        }

        public void Cleanup(IEnumerable<ITaskItemWrapper> entities)
        {
            foreach (var contactItemWrapper in entities)
                contactItemWrapper.Dispose();
        }

        public async Task<EntityVersion<string, DateTime>> TryUpdate(
            string entityId,
            DateTime entityVersion,
            ITaskItemWrapper entityToUpdate,
            Func<ITaskItemWrapper, Task<ITaskItemWrapper>> entityModifier,
            int context,
            IEntitySynchronizationLogger logger)
        {
            entityToUpdate = await entityModifier(entityToUpdate);
            entityToUpdate.Inner.Save();
            return new EntityVersion<string, DateTime>(entityToUpdate.Inner.EntryID, entityToUpdate.Inner.LastModificationTime.ToUniversalTime());
        }

        public Task<bool> TryDelete(
            string entityId,
            DateTime version,
            int context,
            IEntitySynchronizationLogger logger)
        {
            var entityWithId = Get(new[] {entityId}, NullLoadEntityLogger.Instance, 0).Result.SingleOrDefault();
            if (entityWithId == null)
                return Task.FromResult(true);

            using (var entity = entityWithId.Entity)
            {
                entity.Inner.Delete();
            }

            return Task.FromResult(true);
        }

        public async Task<EntityVersion<string, DateTime>> Create(Func<ITaskItemWrapper, Task<ITaskItemWrapper>> entityInitializer, int context)
        {
            using (var taskFolderWrapper = CreateFolderWrapper())
            using (var wrapper = _comWrapperFactory.Create(
                _useDefaultFolderItemType
                    ? (TaskItem) taskFolderWrapper.Inner.Items.Add()
                    : (TaskItem) taskFolderWrapper.Inner.Items.Add(OlItemType.olTaskItem),
                entryId => _session.GetTaskItem(entryId, _folderStoreId)))
            {
                ITaskItemWrapper initializedWrapper;

                try
                {
                    initializedWrapper = await entityInitializer(wrapper);
                }
                catch
                {
                    try
                    {
                        wrapper.Inner.Delete();
                    }
                    catch (System.Exception x)
                    {
                        s_logger.Error("Error while deleting leftover entity", x);
                    }

                    throw;
                }

                using (initializedWrapper)
                {
                    initializedWrapper.SaveAndReload();
                    var result = new EntityVersion<string, DateTime>(initializedWrapper.Inner.EntryID, initializedWrapper.Inner.LastModificationTime.ToUniversalTime());
                    return result;
                }
            }
        }
    }
    internal static class RecurringTaskSeries
    {
        public const string SeriesIdPropertyName =
            "CalDavSynchronizerSeriesId";

        public const string SeriesIdColumn =
            "http://schemas.microsoft.com/mapi/string/{00020329-0000-0000-C000-000000000046}/CalDavSynchronizerSeriesId/0x0000001F";

        public const string DeadOccurrenceColumn =
            "http://schemas.microsoft.com/mapi/id/{00062003-0000-0000-C000-000000000046}/8109000B";

        public const string RecurringColumn =
            "http://schemas.microsoft.com/mapi/id/{00062003-0000-0000-C000-000000000046}/8126000B";

        public static string GetSeriesId(TaskItem task)
        {
            using (var userPropertiesWrapper =
                   GenericComObjectWrapper.Create(task.UserProperties))
            {
                using (var propertyWrapper =
                       GenericComObjectWrapper.Create(
                           userPropertiesWrapper.Inner.Find(SeriesIdPropertyName)))
                {
                    return propertyWrapper.Inner?.Value?.ToString();
                }
            }
        }

        public static string EnsureSeriesId(TaskItem task)
        {
            var seriesId = GetSeriesId(task);

            if (!string.IsNullOrEmpty(seriesId))
                return seriesId;

            seriesId = Guid.NewGuid().ToString("D");
            SetSeriesId(task, seriesId);
            task.Save();

            return seriesId;
        }

        public static void SetSeriesId(TaskItem task, string seriesId)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            if (string.IsNullOrEmpty(seriesId))
                throw new ArgumentException("Argument is null or empty", nameof(seriesId));

            using (var userPropertiesWrapper =
                   GenericComObjectWrapper.Create(task.UserProperties))
            {
                var property = userPropertiesWrapper.Inner.Find(SeriesIdPropertyName);
                if (property == null)
                {
                    property = userPropertiesWrapper.Inner.Add(
                        SeriesIdPropertyName,
                        OlUserPropertyType.olText,
                        false);
                }

                using (var propertyWrapper = GenericComObjectWrapper.Create(property))
                {
                    propertyWrapper.Inner.Value = seriesId;
                }
            }
        }

        public static void SetDeadOccurrence(TaskItem task, bool value)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            task.PropertyAccessor.SetProperty(DeadOccurrenceColumn, value);
        }

        public static bool IsDeadOccurrence(TaskItem task)
        {
            try
            {
                return (bool)task.PropertyAccessor.GetProperty(
                    DeadOccurrenceColumn);
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}