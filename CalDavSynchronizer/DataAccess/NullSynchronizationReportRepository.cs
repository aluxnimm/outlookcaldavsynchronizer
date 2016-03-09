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
using GenSync.Logging;

namespace CalDavSynchronizer.DataAccess
{
  internal class NullSynchronizationReportRepository : ISynchronizationReportRepository
  {
    public static readonly ISynchronizationReportRepository Instance = new NullSynchronizationReportRepository();

    private NullSynchronizationReportRepository ()
    {
    }

    public event EventHandler<ReportAddedEventArgs> ReportAdded;

    protected virtual void OnReportAdded (ReportAddedEventArgs e)
    {
      var handler = ReportAdded;
      if (handler != null)
        handler (this, e);
    }

    public SynchronizationReportName AddReport (SynchronizationReport report)
    {
      throw new NotSupportedException();
    }

    public IReadOnlyList<SynchronizationReportName> GetAvailableReports ()
    {
      return new SynchronizationReportName[] { };
    }

    public SynchronizationReport GetReport (SynchronizationReportName name)
    {
      return null;
    }

    public void DeleteReport (SynchronizationReportName name)
    {
    }

    public Stream GetReportStream (SynchronizationReportName name)
    {
      return new MemoryStream();
    }
  }
}