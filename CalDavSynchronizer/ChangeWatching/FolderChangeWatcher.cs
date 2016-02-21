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

namespace CalDavSynchronizer.ChangeWatching
{
  class FolderChangeWatcher : IItemCollectionChangeWatcher
  {
    private readonly IItemCollectionChangeWatcher _applicationScopeChangeWatcher;
    private readonly string _folderEntryId;
    private readonly string _folderStoreId;

    public event EventHandler<ItemSavedEventArgs> ItemSavedOrDeleted;

    public FolderChangeWatcher (string folderEntryId, string folderStoreId, IItemCollectionChangeWatcher applicationScopeChangeWatcher)
    {
      if (applicationScopeChangeWatcher == null)
        throw new ArgumentNullException (nameof (applicationScopeChangeWatcher));

      _folderEntryId = folderEntryId;
      _folderStoreId = folderStoreId;
      _applicationScopeChangeWatcher = applicationScopeChangeWatcher;
      _applicationScopeChangeWatcher.ItemSavedOrDeleted += _applicationScopeChangeWatcher_ItemSavedOrDeleted;
    }

    private void _applicationScopeChangeWatcher_ItemSavedOrDeleted (object sender, ItemSavedEventArgs e)
    {
      if (e.FolderEntryId == _folderEntryId && e.FolderStoreId == _folderStoreId)
        ItemSavedOrDeleted?.Invoke (this, e);
    }

    public void Dispose ()
    {
      _applicationScopeChangeWatcher.ItemSavedOrDeleted -= _applicationScopeChangeWatcher_ItemSavedOrDeleted;
    }
  }
}