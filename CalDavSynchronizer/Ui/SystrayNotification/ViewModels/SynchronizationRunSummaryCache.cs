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
using GenSync.Logging;

namespace CalDavSynchronizer.Ui.SystrayNotification.ViewModels
{
    public class SynchronizationRunSummaryCache
    {
        private readonly Dictionary<Guid, SynchronizationRunSummary?> _summaryByProfileId = new Dictionary<Guid, SynchronizationRunSummary?>();

        public IReadOnlyDictionary<Guid, SynchronizationRunSummary?> SummaryByProfileId => _summaryByProfileId;

        public void NotifyProfilesChanged(Contracts.Options[] profiles)
        {
            HashSet<Guid> existingProfiles = new HashSet<Guid>();

            foreach (var profile in profiles)
            {
                existingProfiles.Add(profile.Id);
                if (!_summaryByProfileId.ContainsKey(profile.Id))
                    _summaryByProfileId[profile.Id] = null;
            }

            foreach (var profileId in _summaryByProfileId.Keys.ToArray())
            {
                if (!existingProfiles.Contains(profileId))
                    _summaryByProfileId.Remove(profileId);
            }
        }

        public void Update(Guid profileId, SynchronizationRunSummary summary)
        {
            if (_summaryByProfileId.ContainsKey(profileId))
                _summaryByProfileId[profileId] = summary;
        }
    }
}