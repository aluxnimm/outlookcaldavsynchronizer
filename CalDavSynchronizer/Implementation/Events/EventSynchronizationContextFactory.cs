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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.using System;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Utilities;
using DDay.iCal;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.Synchronization;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.Events
{
  public class EventSynchronizationContextFactory : ISynchronizationContextFactory<IEventSynchronizationContext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly OutlookEventRepository _outlookRepository;
    private readonly IEntityRepository<WebResourceName, string, IICalendar, IEventSynchronizationContext> _btypeRepository;
    private readonly IEntityRelationDataAccess<AppointmentId, DateTime, WebResourceName, string> _entityRelationDataAccess;
    private readonly bool _cleanupDuplicateEvents;
    private readonly IEqualityComparer<AppointmentId> _idComparer;
    private readonly IOutlookSession _outlookSession;
    private readonly bool _mapEventColorToCategory;

    public EventSynchronizationContextFactory(OutlookEventRepository outlookRepository, IEntityRepository<WebResourceName, string, IICalendar, IEventSynchronizationContext> btypeRepository, IEntityRelationDataAccess<AppointmentId, DateTime, WebResourceName, string> entityRelationDataAccess, bool cleanupDuplicateEvents, IEqualityComparer<AppointmentId> idComparer, IOutlookSession outlookSession, bool mapEventColorToCategory)
    {
      if (outlookRepository == null)
        throw new ArgumentNullException (nameof (outlookRepository));
      if (btypeRepository == null)
        throw new ArgumentNullException (nameof (btypeRepository));
      if (entityRelationDataAccess == null)
        throw new ArgumentNullException (nameof (entityRelationDataAccess));
      if (idComparer == null) throw new ArgumentNullException(nameof(idComparer));
      if (outlookSession == null) throw new ArgumentNullException(nameof(outlookSession));

      _outlookRepository = outlookRepository;
      _btypeRepository = btypeRepository;
      _entityRelationDataAccess = entityRelationDataAccess;
      _cleanupDuplicateEvents = cleanupDuplicateEvents;
      _idComparer = idComparer;
      _outlookSession = outlookSession;
      _mapEventColorToCategory = mapEventColorToCategory;
    }

    public Task<IEventSynchronizationContext> Create()
    {
      if (_mapEventColorToCategory)
        EnsureColorCategoriesExist();

      return Task.FromResult<IEventSynchronizationContext>(
        new EventSynchronizationContext(
          _cleanupDuplicateEvents
            ? new DuplicateEventCleaner(
              _outlookRepository,
              _btypeRepository,
              _entityRelationDataAccess,
              _idComparer)
            : NullDuplicateEventCleaner.Instance,
          _outlookSession));
    }

    private void EnsureColorCategoriesExist()
    {
      try
      {
        using (var categoriesWrapper = GenericComObjectWrapper.Create(_outlookSession.Categories))
        {
          var categoryNames = new HashSet<string>(
            categoriesWrapper.Inner.ToSafeEnumerable<Category>().Select(c => c.Name),
            StringComparer.InvariantCultureIgnoreCase);

          foreach (var item in ColorHelper.HtmlColorByCategoryColor)
          {
            if (!categoryNames.Contains(item.Value))
            {
              categoriesWrapper.Inner.Add(item.Value, item.Key);
            }
          }
        }
      }
      catch (System.Exception e)
      {
        s_logger.Error("Can't add color categories.", e);
      }
    }


    public async Task SynchronizationFinished (IEventSynchronizationContext context)
    {
      await context.DuplicateEventCleaner.NotifySynchronizationFinished();
    }
  }
}