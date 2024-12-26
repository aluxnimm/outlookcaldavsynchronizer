﻿// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
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
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using DDay.iCal;
using GenSync.InitialEntityMatching;

namespace CalDavSynchronizer.Implementation.Events
{
    internal class InitialEventEntityMatcher : InitialEntityMatcherByPropertyGrouping<AppointmentId, DateTime, EventEntityMatchData, string, WebResourceName, string, EventServerEntityMatchData, string>
    {
        public InitialEventEntityMatcher(IEqualityComparer<WebResourceName> btypeIdEqualityComparer)
            : base(btypeIdEqualityComparer)
        {
        }

        protected override bool AreEqual(EventEntityMatchData atypeEntity, EventServerEntityMatchData evt)
        {
            if (evt.Summary == atypeEntity.Subject)
            {
                if (evt.IsAllDay && atypeEntity.AllDayEvent)
                {
                    if (evt.StartUtc == atypeEntity.StartUtc)
                    {
                        if (evt.EndUtc == null)
                            return evt.StartUtc.AddDays(1) == atypeEntity.EndUtc;
                        else
                            return evt.EndUtc == atypeEntity.EndUtc;
                    }
                    else return false;
                }
                else if (!evt.IsAllDay)
                {
                    if (evt.StartUtc == atypeEntity.StartUtc)
                    {
                        if (evt.DTEndUtc == null)
                            return evt.StartUtc == atypeEntity.EndUtc;
                        else
                            return evt.DTEndUtc == atypeEntity.EndUtc;
                    }
                    else return false;
                }
            }

            return false;
        }

        protected override string GetAtypePropertyValue(EventEntityMatchData atypeEntity)
        {
            return atypeEntity.Subject?.ToLower() ?? string.Empty;
        }

        protected override string GetBtypePropertyValue(EventServerEntityMatchData btypeEntity)
        {
            return btypeEntity.Summary?.ToLower() ?? string.Empty;
        }

        protected override string MapAtypePropertyValue(string value)
        {
            return value;
        }
    }
}