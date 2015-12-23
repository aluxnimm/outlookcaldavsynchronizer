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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.using System;

using System;
using System.Collections.Generic;
using System.IO;
using GenSync.Logging;

namespace CalDavSynchronizer.DataAccess
{
  internal class FilteringSynchronizationReportRepositoryWrapper : ISynchronizationReportRepository
  {
    private readonly ISynchronizationReportRepository _inner;

    public FilteringSynchronizationReportRepositoryWrapper (ISynchronizationReportRepository inner)
    {
      _inner = inner;
    }

    public bool AcceptAddingReportsWithJustWarnings { get; set; }
    public bool AcceptAddingReportsWithoutWarningsOrErrors { get; set; }

    public event EventHandler<ReportAddedEventArgs> ReportAdded {
      add { _inner.ReportAdded += value; }
      remove { _inner.ReportAdded -= value; }
    }

    public void AddReport (SynchronizationReport report)
    {
      if (report.HasErrors
          || AcceptAddingReportsWithJustWarnings && report.HasWarnings
          || AcceptAddingReportsWithoutWarningsOrErrors)
      {
        _inner.AddReport (report);
      }
    }

    public IReadOnlyList<SynchronizationReportName> GetAvailableReports ()
    {
      return _inner.GetAvailableReports();
    }

    public SynchronizationReport GetReport (SynchronizationReportName name)
    {
      return _inner.GetReport(name);
    }

    public void DeleteReport (SynchronizationReportName name)
    {
      _inner.DeleteReport (name);
    }

    public Stream GetReportStream (SynchronizationReportName name)
    {
      return _inner.GetReportStream (name);
    }
  }
}