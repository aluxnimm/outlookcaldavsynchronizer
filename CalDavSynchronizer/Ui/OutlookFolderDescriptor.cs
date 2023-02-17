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

using CalDavSynchronizer.Implementation.ComWrappers;
using log4net;
using Microsoft.Office.Interop.Outlook;
using System.Runtime.InteropServices;

namespace CalDavSynchronizer.Ui
{
    public class OutlookFolderDescriptor
    {
        private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

        public string EntryId { get; }
        public string StoreId { get; }
        public OlItemType DefaultItemType { get; }
        public string Name { get; }
        public int ItemCount { get; }

        public OutlookFolderDescriptor(string entryId, string storeId, OlItemType defaultItemType, string name, int itemCount)
        {
            EntryId = entryId;
            StoreId = storeId;
            DefaultItemType = defaultItemType;
            Name = name;
            ItemCount = itemCount;
        }

        public OutlookFolderDescriptor(MAPIFolder folder)
            : this(folder.EntryID, folder.StoreID, folder.DefaultItemType, folder.Name, 0)
        {
            using (var itemsWrapper = GenericComObjectWrapper.Create(folder.Items))
            {
                try
                {
                    ItemCount = itemsWrapper.Inner.Count;
                }
                catch (COMException ex)
                {
                    s_logger.Warn("Could not get ItemCount of folder: " + folder.Name, ex);          
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    s_logger.Warn("Could not get ItemCount of folder: " + folder.Name, ex);
                }
            }
        }
    }
}