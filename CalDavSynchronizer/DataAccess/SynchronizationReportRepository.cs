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
using CalDavSynchronizer.Utilities;
using GenSync.Logging;

namespace CalDavSynchronizer.DataAccess
{
  public class SynchronizationReportRepository : ISynchronizationReportRepository
  {
    private readonly string _reportDirectory;

    public SynchronizationReportRepository (string reportDirectory)
    {
      _reportDirectory = reportDirectory;
    }

    public event EventHandler<ReportEventArgs> ReportAdded;

    protected virtual void OnReportAdded (SynchronizationReport report)
    {
      var handler = ReportAdded;
      if (handler != null)
        handler (this, new ReportEventArgs (report));
    }

    public void AddReport (SynchronizationReport report)
    {
      var reportName = GetNextFreeName (_reportDirectory, report);

      using (var fileStream = File.Create (Path.Combine (_reportDirectory, reportName.ToString())))
      {
        Serializer<SynchronizationReport>.SerializeTo (report, fileStream);
      }

      OnReportAdded (report);
    }

    public IReadOnlyList<SynchronizationReportName> GetAvailableReports ()
    {
      var names = new List<SynchronizationReportName>();
      var fileNames = Directory.GetFiles (_reportDirectory);

      foreach (var fileName in fileNames)
      {
        SynchronizationReportName name;
        if (SynchronizationReportName.TryParse (Path.GetFileName (fileName), out name))
          names.Add (name);
      }

      return names;
    }

    public SynchronizationReport GetReport (SynchronizationReportName name)
    {
      using (var fileStream = File.OpenRead (Path.Combine (_reportDirectory, name.ToString())))
      {
        return Serializer<SynchronizationReport>.DeserializeFrom (fileStream);
      }
    }

    public void DeleteReport (SynchronizationReportName name)
    {
      File.Delete (Path.Combine (_reportDirectory, name.ToString()));
    }

    public Stream GetReportStream (SynchronizationReportName name)
    {
      return File.OpenRead (Path.Combine (_reportDirectory, name.ToString()));
    }

    private SynchronizationReportName GetNextFreeName (string directory, SynchronizationReport report)
    {
      var reportName = SynchronizationReportName.Create (report.ProfileId, report.StartTime, report.HasWarnings, report.HasErrors);
      while (File.Exists (Path.Combine (directory, reportName.ToString())))
        reportName = reportName.IncreaseSequence();

      return reportName;
    }
  }
}