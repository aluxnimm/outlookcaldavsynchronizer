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
using System.Reflection;
using CalDavSynchronizer.Utilities;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer.ChangeWatching
{
  internal class OutlookItemChangeWatcher : IItemCollectionChangeWatcher
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly Dictionary<IOutlookItem, bool> _isChangedByItem = new Dictionary<IOutlookItem, bool>();

    public event EventHandler<ItemSavedEventArgs> ItemSavedOrDeleted;

    public OutlookItemChangeWatcher (Inspectors inspectors)
    {
      inspectors.NewInspector += Inspectors_NewInspector;
    }

    protected virtual void OnItemSavedOrDeleted (ItemSavedEventArgs e)
    {
      var handler = ItemSavedOrDeleted;
      if (handler != null)
        handler (this, e);
    }

    private void Inspectors_NewInspector (Inspector inspector)
    {
      try
      {
        var item = TryCreateItem (inspector);
        if (item != null)
        {
          _isChangedByItem.Add (item, false);
          item.Closed += Item_Closed;
          item.SavedOrDeleted += Item_SavedOrDeleted;
        }
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.LogException (x, s_logger);
      }
    }

    private void Item_SavedOrDeleted (object sender, EventArgs e)
    {
      try
      {
        _isChangedByItem[((IOutlookItem) sender)] = true;
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.LogException (x, s_logger);
      }
    }

    private void Item_Closed (object sender, EventArgs e)
    {
      try
      {
        var outlookItem = ((IOutlookItem) sender);
        var isChanged = _isChangedByItem[outlookItem];
        _isChangedByItem.Remove (outlookItem);
        if (isChanged)
        {
          var folderIds = outlookItem.FolderEntryIdAndStoreIdOrNull;
          if (folderIds != null)
            OnItemSavedOrDeleted (new ItemSavedEventArgs (outlookItem.EntryId, folderIds.Item1, folderIds.Item2));
        }
        outlookItem.Dispose();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.LogException (x, s_logger);
      }
    }

    private IOutlookItem TryCreateItem (Inspector inspector)
    {
      var appointment = inspector.CurrentItem as AppointmentItem;
      if (appointment != null)
        return new AppointmentItemAdapter (inspector, appointment);
      else
        return null;
    }

    public void Dispose ()
    {
      
    }
  }
}