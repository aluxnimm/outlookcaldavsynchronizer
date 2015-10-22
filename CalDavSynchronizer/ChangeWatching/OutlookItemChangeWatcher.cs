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
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.ChangeWatching
{
  internal class OutlookItemChangeWatcher
  {
    private readonly Dictionary<IOutlookItem, bool> _isChangedByItem = new Dictionary<IOutlookItem, bool>();

    public event EventHandler<ItemSavedEventArgs> ItemSaved;

    public OutlookItemChangeWatcher (Inspectors inspectors)
    {
      inspectors.NewInspector += Inspectors_NewInspector;
    }

    protected virtual void OnItemSaved (ItemSavedEventArgs e)
    {
      var handler = ItemSaved;
      if (handler != null)
        handler (this, e);
    }

    private void Inspectors_NewInspector (Inspector inspector)
    {
      var item = TryCreateItem (inspector);
      if (item != null)
      {
        _isChangedByItem.Add (item, false);
        item.Closed += Item_Closed;
        item.Saved += Item_Saved;
      }
    }

    private void Item_Saved (object sender, EventArgs e)
    {
      _isChangedByItem[((IOutlookItem) sender)] = true;
    }

    private void Item_Closed (object sender, EventArgs e)
    {
      var outlookItem = ((IOutlookItem) sender);
      var isChanged = _isChangedByItem[outlookItem];
      _isChangedByItem.Remove (outlookItem);
      if (isChanged)
      {
        var folderIds = outlookItem.FolderEntryIdAndStoreIdOrNull;
        if (folderIds != null)
          OnItemSaved (new ItemSavedEventArgs (outlookItem.EntryId, folderIds.Item1, folderIds.Item2));
      }
      outlookItem.Dispose();
    }

    private IOutlookItem TryCreateItem (Inspector inspector)
    {
      var appointment = inspector.CurrentItem as AppointmentItem;
      if (appointment != null)
        return new AppointmentItemChangeWrapper (inspector, appointment);
      else
        return null;
    }
  }
}