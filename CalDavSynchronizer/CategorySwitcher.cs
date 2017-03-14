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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Events;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer
{
  public class CategorySwitcher
  {
    private static readonly ILog s_logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly DaslFilterProvider _daslFilterProvider;
    private readonly OutlookFolderStrategyWrapper _queryFolderStrategyWrapper;
    // ReSharper disable once ConvertToConstant.Local


    private readonly IOutlookSession _session;

    public CategorySwitcher(IOutlookSession session, DaslFilterProvider daslFilterProvider, OutlookFolderStrategyWrapper queryFolderStrategyWrapper)
    {
      if (session == null) throw new ArgumentNullException(nameof(session));
      if (daslFilterProvider == null) throw new ArgumentNullException(nameof(daslFilterProvider));
      if (queryFolderStrategyWrapper == null) throw new ArgumentNullException(nameof(queryFolderStrategyWrapper));

      _session = session;
      _daslFilterProvider = daslFilterProvider;
      _queryFolderStrategyWrapper = queryFolderStrategyWrapper;
    }

    public void SwitchCategories(ChangedOptions[] changedOptions)
    {
      foreach (var changedOption in changedOptions)
      {
        var oldEventCategory = GetMappingRefPropertyOrNull<EventMappingConfiguration, string>(changedOption.Old.MappingConfiguration, o => o.EventCategory);
        var newEventCategory = GetMappingRefPropertyOrNull<EventMappingConfiguration, string>(changedOption.New.MappingConfiguration, o => o.EventCategory);
        var negateEventCategoryFilter = GetMappingPropertyOrNull<EventMappingConfiguration, bool>(changedOption.New.MappingConfiguration, o => o.InvertEventCategoryFilter);

        if (oldEventCategory != newEventCategory && !string.IsNullOrEmpty(oldEventCategory) && !negateEventCategoryFilter.Value)
          try
          {
            SwitchEventCategories(changedOption, oldEventCategory, newEventCategory);
          }
          catch (Exception x)
          {
            s_logger.Error(null, x);
          }

        if (!string.IsNullOrEmpty(newEventCategory))
        {
          var mappingConfiguration = (EventMappingConfiguration) changedOption.New.MappingConfiguration;

          if (mappingConfiguration.UseEventCategoryColorAndMapFromCalendarColor || mappingConfiguration.CategoryShortcutKey != OlCategoryShortcutKey.olCategoryShortcutKeyNone)
            try
            {
              using (var categoriesWrapper = GenericComObjectWrapper.Create(_session.Categories))
              {
                foreach (var existingCategory in categoriesWrapper.Inner.ToSafeEnumerable<Category>())
                  if (existingCategory.ShortcutKey == mappingConfiguration.CategoryShortcutKey)
                    existingCategory.ShortcutKey = OlCategoryShortcutKey.olCategoryShortcutKeyNone;

                using (var categoryWrapper = GenericComObjectWrapper.Create(categoriesWrapper.Inner[newEventCategory]))
                {
                  if (categoryWrapper.Inner == null)
                  {
                    categoriesWrapper.Inner.Add(newEventCategory, mappingConfiguration.EventCategoryColor, mappingConfiguration.CategoryShortcutKey);
                  }
                  else
                  {
                    categoryWrapper.Inner.Color = mappingConfiguration.EventCategoryColor;
                    categoryWrapper.Inner.ShortcutKey = mappingConfiguration.CategoryShortcutKey;
                  }
                }
              }
            }
            catch (Exception x)
            {
              s_logger.Error(null, x);
            }
        }

        var oldTaskCategory = GetMappingRefPropertyOrNull<TaskMappingConfiguration, string>(changedOption.Old.MappingConfiguration, o => o.TaskCategory);
        var newTaskCategory = GetMappingRefPropertyOrNull<TaskMappingConfiguration, string>(changedOption.New.MappingConfiguration, o => o.TaskCategory);
        var negateTaskCategoryFilter = GetMappingPropertyOrNull<TaskMappingConfiguration, bool>(changedOption.New.MappingConfiguration, o => o.InvertTaskCategoryFilter);

        if (oldTaskCategory != newTaskCategory && !string.IsNullOrEmpty(oldTaskCategory) && !negateTaskCategoryFilter.Value)
          try
          {
            SwitchTaskCategories(changedOption, oldTaskCategory, newTaskCategory);
          }
          catch (Exception x)
          {
            s_logger.Error(null, x);
          }
      }
    }

    private void SwitchEventCategories(ChangedOptions changedOption, string oldCategory, string newCategory)
    {
      using (var calendarFolderWrapper = GenericComObjectWrapper.Create(
        _session.GetFolderFromId(changedOption.New.OutlookFolderEntryId, changedOption.New.OutlookFolderStoreId)))
      {
        s_logger.Info($"Switching category of items in folder '{calendarFolderWrapper.Inner.Name}' from '{oldCategory}' to '{newCategory}', due to changes in profile '{changedOption.New.Name}' (OptionId:'{changedOption.New.Id}' FolderId:'{changedOption.New.OutlookFolderEntryId}' StoreId:'{changedOption.New.OutlookFolderStoreId}')");

        var isInstantSearchEnabled = false;

        try
        {
          using (var store = GenericComObjectWrapper.Create(calendarFolderWrapper.Inner.Store))
          {
            if (store.Inner != null) isInstantSearchEnabled = store.Inner.IsInstantSearchEnabled;
          }
        }
        catch (COMException)
        {
          s_logger.Info("Can't access IsInstantSearchEnabled property of store, defaulting to false.");
        }
        var filterBuilder = new StringBuilder(_daslFilterProvider.GetAppointmentFilter(isInstantSearchEnabled));
        OutlookEventRepository.AddCategoryFilter(filterBuilder, oldCategory, false, false);
        var eventIds = _queryFolderStrategyWrapper.QueryAppointmentFolder(_session, calendarFolderWrapper.Inner, filterBuilder.ToString()).Select(e => e.Version.Id);
        // todo concat Ids from cache

        foreach (var eventId in eventIds)
          try
          {
            SwitchEventCategories(changedOption, oldCategory, newCategory, eventId);
          }
          catch (Exception x)
          {
            s_logger.Error(null, x);
          }
      }
    }

    private void SwitchTaskCategories(ChangedOptions changedOption, string oldCategory, string newCategory)
    {
      using (var taskFolderWrapper = GenericComObjectWrapper.Create(
        _session.GetFolderFromId(changedOption.New.OutlookFolderEntryId, changedOption.New.OutlookFolderStoreId)))
      {
        s_logger.Info($"Switching category of items in folder '{taskFolderWrapper.Inner.Name}' from '{oldCategory}' to '{newCategory}', due to changes in profile '{changedOption.New.Name}' (OptionId:'{changedOption.New.Id}' FolderId:'{changedOption.New.OutlookFolderEntryId}' StoreId:'{changedOption.New.OutlookFolderStoreId}')");

        var isInstantSearchEnabled = false;

        try
        {
          using (var store = GenericComObjectWrapper.Create(taskFolderWrapper.Inner.Store))
          {
            if (store.Inner != null) isInstantSearchEnabled = store.Inner.IsInstantSearchEnabled;
          }
        }
        catch (COMException)
        {
          s_logger.Info("Can't access IsInstantSearchEnabled property of store, defaulting to false.");
        }
        var filterBuilder = new StringBuilder(_daslFilterProvider.GetTaskFilter(isInstantSearchEnabled));
        OutlookEventRepository.AddCategoryFilter(filterBuilder, oldCategory, false, false);
        var taskIds = _queryFolderStrategyWrapper.QueryTaskFolder(_session, taskFolderWrapper.Inner, filterBuilder.ToString()).Select(e => e.Id);
        // todo concat Ids from cache

        foreach (var taskId in taskIds)
          try
          {
            SwitchTaskCategories(changedOption, oldCategory, newCategory, taskId);
          }
          catch (Exception x)
          {
            s_logger.Error(null, x);
          }
      }
    }

    private void SwitchEventCategories(ChangedOptions changedOption, string oldCategory, string newCategory, AppointmentId eventId)
    {
      using (var eventWrapper = new AppointmentItemWrapper(
        _session.GetAppointmentItem(eventId.EntryId, changedOption.New.OutlookFolderStoreId),
        entryId => _session.GetAppointmentItem(entryId, changedOption.New.OutlookFolderStoreId)))
      {
        var categories = eventWrapper.Inner.Categories
          .Split(new[] {CultureInfo.CurrentCulture.TextInfo.ListSeparator}, StringSplitOptions.RemoveEmptyEntries)
          .Select(c => c.Trim());

        eventWrapper.Inner.Categories = string.Join(
          CultureInfo.CurrentCulture.TextInfo.ListSeparator,
          categories
            .Except(new[] {oldCategory})
            .Concat(new[] {newCategory})
            .Distinct());

        eventWrapper.Inner.Save();
      }
    }

    private void SwitchTaskCategories(ChangedOptions changedOption, string oldCategory, string newCategory, string eventId)
    {
      using (var taskWrapper = new TaskItemWrapper(
        _session.GetTaskItem(eventId, changedOption.New.OutlookFolderStoreId),
        entryId => _session.GetTaskItem(entryId, changedOption.New.OutlookFolderStoreId)))
      {
        var categories = taskWrapper.Inner.Categories
          .Split(new[] {CultureInfo.CurrentCulture.TextInfo.ListSeparator}, StringSplitOptions.RemoveEmptyEntries)
          .Select(c => c.Trim());

        taskWrapper.Inner.Categories = string.Join(
          CultureInfo.CurrentCulture.TextInfo.ListSeparator,
          categories
            .Except(new[] {oldCategory})
            .Concat(new[] {newCategory})
            .Distinct());

        taskWrapper.Inner.Save();
      }
    }


    private TProperty? GetMappingPropertyOrNull<TMappingConfiguration, TProperty>(MappingConfigurationBase mappingConfiguration, Func<TMappingConfiguration, TProperty> selector)
      where TMappingConfiguration : MappingConfigurationBase
      where TProperty : struct
    {
      var typedMappingConfiguration = mappingConfiguration as TMappingConfiguration;

      if (typedMappingConfiguration != null)
        return selector(typedMappingConfiguration);
      return null;
    }

    private TProperty GetMappingRefPropertyOrNull<TMappingConfiguration, TProperty>(MappingConfigurationBase mappingConfiguration, Func<TMappingConfiguration, TProperty> selector)
      where TMappingConfiguration : MappingConfigurationBase
      where TProperty : class
    {
      var typedMappingConfiguration = mappingConfiguration as TMappingConfiguration;

      if (typedMappingConfiguration != null)
        return selector(typedMappingConfiguration);
      return null;
    }
  }
}