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
using CalDavSynchronizer.Implementation.Events;
using GenSync;
using GenSync.Logging;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.Common
{
  public class OutlookFolderStrategyWrapper : IQueryOutlookFolderStrategy
  {
    private IQueryOutlookFolderStrategy _strategy;

    public OutlookFolderStrategyWrapper(IQueryOutlookFolderStrategy strategy)
    {
      if (strategy == null) throw new ArgumentNullException(nameof(strategy));
      _strategy = strategy;
    }

    public void SetStrategy(IQueryOutlookFolderStrategy strategy)
    {
      if (strategy == null) throw new ArgumentNullException(nameof(strategy));
      _strategy = strategy;
    }

    public List<AppointmentSlim> QueryAppointmentFolder(IOutlookSession session, Folder folder, string filter, IGetVersionsLogger logger)
    {
      return _strategy.QueryAppointmentFolder(session, folder, filter, logger);
    }

    public List<EntityVersion<string, DateTime>> QueryContactItemFolder(IOutlookSession session, Folder folder, string expectedFolderId, string filter, IGetVersionsLogger logger)
    {
      return _strategy.QueryContactItemFolder (session, folder, expectedFolderId, filter, logger);
    }

    public List<EntityVersion<string, DateTime>> QueryDistListFolder(IOutlookSession session, Folder folder, string expectedFolderId, string filter, IGetVersionsLogger logger)
    {
      return _strategy.QueryDistListFolder (session, folder, expectedFolderId, filter, logger);
    }

    public List<EntityVersion<string, DateTime>> QueryTaskFolder(IOutlookSession session, Folder folder, string filter, IGetVersionsLogger logger)
    {
      return _strategy.QueryTaskFolder (session, folder, filter, logger);
    }
  }
}