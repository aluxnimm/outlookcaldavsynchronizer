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
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation;
using GenSync.Logging;
using GenSync.Synchronization;

namespace CalDavSynchronizer.Synchronization
{
  public class OutlookSynchronizer : IOutlookSynchronizer
  {
    private readonly IPartialSynchronizer<string, Uri> _synchronizer;
    private readonly IOutlookRepository _outlookRepository;

    public OutlookSynchronizer (IPartialSynchronizer<string, Uri> synchronizer, IOutlookRepository outlookRepository)
    {
      _synchronizer = synchronizer;
      _outlookRepository = outlookRepository;
    }

    public Task SynchronizeNoThrow (ISynchronizationLogger logger)
    {
      return _synchronizer.SynchronizeNoThrow (logger);
    }

    public async Task SnychronizePartialNoThrow (IEnumerable<string> outlookIds, ISynchronizationLogger logger)
    {
      await _synchronizer.SynchronizePartialNoThrow (outlookIds, new Uri[] { }, logger);
    }

    public bool IsResponsible (string folderEntryId, string folderStoreId)
    {
      return _outlookRepository.IsResponsibleForFolder (folderEntryId, folderStoreId);
    }
  }
}