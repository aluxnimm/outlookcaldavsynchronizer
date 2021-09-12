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
using System.Xml;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Tasks;
using DDay.iCal;
using GenSync.InitialEntityMatching;
using Google.Apis.Tasks.v1.Data;

namespace CalDavSynchronizer.Implementation.GoogleTasks
{
    internal class InitialGoogleTastEntityMatcher : InitialEntityMatcherByPropertyGrouping<string, DateTime, TaskEntityMatchData, string, string, string, Task, string>
    {
        public InitialGoogleTastEntityMatcher(IEqualityComparer<string> btypeIdEqualityComparer)
            : base(btypeIdEqualityComparer)
        {
        }

        protected override bool AreEqual(TaskEntityMatchData atypeEntity, Task btypeEntity)
        {
            if (atypeEntity.Subject == btypeEntity.Title)
            {
                if (string.IsNullOrEmpty(btypeEntity.Due))
                {
                    return atypeEntity.DueDate == OutlookUtility.OUTLOOK_DATE_NONE;
                }
                else
                {
                    return atypeEntity.DueDate == XmlConvert.ToDateTime(btypeEntity.Due, XmlDateTimeSerializationMode.Utc).Date;
                }
            }

            return false;
        }

        protected override string GetAtypePropertyValue(TaskEntityMatchData atypeEntity)
        {
            return (atypeEntity.Subject ?? string.Empty).ToLower();
        }

        protected override string GetBtypePropertyValue(Task btypeEntity)
        {
            return (btypeEntity.Title ?? string.Empty).ToLower();
        }

        protected override string MapAtypePropertyValue(string value)
        {
            return value;
        }
    }
}