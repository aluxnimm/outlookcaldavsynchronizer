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
using System.Linq;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Utilities;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.Events
{
  public class EventSynchronizationContext : IEventSynchronizationContext
  {
    private readonly Lazy<Dictionary<string, OlCategoryColor>> _colorByCategoryName;

    public EventSynchronizationContext(IDuplicateEventCleaner duplicateEventCleaner, IOutlookSession session)
    {
      var outlookSession = session ?? throw new ArgumentNullException(nameof(session));
      DuplicateEventCleaner = duplicateEventCleaner ?? throw new ArgumentNullException(nameof(duplicateEventCleaner));

      _colorByCategoryName = new Lazy<Dictionary<string, OlCategoryColor>>(
        () =>
        {
          using (var categoriesWrapper = GenericComObjectWrapper.Create(outlookSession.Categories))
          {
            return categoriesWrapper.Inner.ToSafeEnumerable<Category>().ToDictionary(c => c.Name, c => c.Color);
          }
        },
        false);
    }

    public IDuplicateEventCleaner DuplicateEventCleaner { get; }
    public OlCategoryColor GetCategoryColor(string categoryName)
    {
      if (_colorByCategoryName.Value.TryGetValue(categoryName, out var color))
        return color;
      else
        return OlCategoryColor.olCategoryColorNone;
    }
  }
}