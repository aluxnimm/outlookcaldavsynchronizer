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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation.ComWrappers;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.EntityCacheVersionConversion
{
    public static class Version1To2
    {
        private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        public static void Convert(
            NameSpace outlookSession,
            Options[] options,
            Func<Options, string> cacheFileGetter,
            Action<Options> cacheDeleter)
        {
            foreach (var option in options)
            {
                try
                {
                    Convert(outlookSession, option, cacheFileGetter, cacheDeleter);
                }
                catch (System.Exception x)
                {
                    s_logger.Error($"Error during conversion for profile '{option.Name}'. Deleting caches", x);
                    cacheDeleter(option);
                }
            }
        }

        private static void Convert(NameSpace outlookSession, Options options, Func<Options, string> cacheFileGetter, Action<Options> cacheDeleter)
        {
            OlItemType defaultItemType;

            using (var outlookFolderWrapper = GenericComObjectWrapper.Create((Folder) outlookSession.GetFolderFromID(options.OutlookFolderEntryId, options.OutlookFolderStoreId)))
            {
                defaultItemType = outlookFolderWrapper.Inner.DefaultItemType;
            }

            if (defaultItemType == OlItemType.olAppointmentItem)
            {
                var fileName = cacheFileGetter(options);
                XDocument document = XDocument.Load(fileName);
                var aTypeNodes = document.Descendants().Where(n => n.Name == "AtypeId");

                foreach (var atypeNode in aTypeNodes)
                {
                    var entryId = atypeNode.Value;
                    string globalAppointmentId;
                    using (var appointmentWrapper = GenericComObjectWrapper.Create((AppointmentItem) outlookSession.GetItemFromID(entryId, options.OutlookFolderStoreId)))
                    {
                        globalAppointmentId = appointmentWrapper.Inner.GlobalAppointmentID;
                    }

                    atypeNode.RemoveAll();
                    atypeNode.Add(new XElement("EntryId", entryId));
                    atypeNode.Add(new XElement("GlobalAppointmentId", globalAppointmentId));
                }

                document.Save(fileName);
            }
        }
    }
}