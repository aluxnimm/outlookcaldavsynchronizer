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
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Implementation;

namespace CalDavSynchronizer.Ui.Options
{
    public class EnumDisplayNameProvider : IEnumDisplayNameProvider
    {
        private EnumDisplayNameProvider()
        {
        }

        public static readonly IEnumDisplayNameProvider Instance = new EnumDisplayNameProvider();

        public string Get(ConflictResolution value)
        {
            switch (value)
            {
                case ConflictResolution.Automatic:
                    return Strings.Get($"Automatic");
                case ConflictResolution.Manual:
                    return Strings.Get($"Manual");
                case ConflictResolution.ServerWins:
                    return Strings.Get($"Server wins");
                case ConflictResolution.OutlookWins:
                    return Strings.Get($"Outlook wins");
                default:
                    throw new NotImplementedException($"Value '{value}' not implemented.");
            }
        }

        public string Get(SynchronizationMode value)
        {
            switch (value)
            {
                case SynchronizationMode.ReplicateOutlookIntoServer:
                    return Strings.Get($"Outlook \u2192 Server (Replicate)");
                case SynchronizationMode.ReplicateServerIntoOutlook:
                    return Strings.Get($"Outlook \u2190 Server (Replicate)");
                case SynchronizationMode.MergeOutlookIntoServer:
                    return Strings.Get($"Outlook \u2192 Server (Merge)");
                case SynchronizationMode.MergeServerIntoOutlook:
                    return Strings.Get($"Outlook \u2190 Server (Merge)");
                case SynchronizationMode.MergeInBothDirections:
                    return Strings.Get($"Outlook \u2190\u2192 Server (Two-Way)");
                default:
                    throw new NotImplementedException($"Value '{value}' not implemented.");
            }
        }
    }
}