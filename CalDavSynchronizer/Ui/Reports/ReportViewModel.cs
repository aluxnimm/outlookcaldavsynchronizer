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
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Utilities;
using GenSync.Logging;

namespace CalDavSynchronizer.Ui.Reports
{
  public class ReportViewModel : ViewModelBase
  {
    private ISynchronizationReportRepository _reportRepository;
    private readonly ReportProxy _reportProxy;
    private string _asString;
    private bool _isSelected;

    public ReportViewModel (ReportProxy reportProxy, ISynchronizationReportRepository reportRepository)
    {
      _reportRepository = reportRepository;
      _reportProxy = reportProxy;
    }

    public SynchronizationReportName ReportName
    {
      get { return _reportProxy.Name; }
    }

    public bool HasErrors
    {
      get { return _reportProxy.Name.HasErrors; }
    }

    public bool HasWarnings
    {
      get { return _reportProxy.Name.HasWarnings; }
    }

    public string ProfileName
    {
      get { return _reportProxy.ProfileName; }
    }

    public DateTime StartTime
    {
      get { return _reportProxy.Name.StartTime.ToLocalTime(); }
    }

    public string AsString
    {
      get { return _asString ?? (_asString = Serializer<SynchronizationReport>.Serialize (_reportProxy.Value)); }
    }

    public bool IsSelected
    {
      get { return _isSelected; }
      set { CheckedPropertyChange (ref _isSelected, value, () => IsSelected); }
    }

    public void Delete ()
    {
      _reportRepository.DeleteReport (_reportProxy.Name);
    }

    public static readonly ReportViewModel DesignInstance;

    static ReportViewModel ()
    {
      DesignInstance = CreateDesignInstance (true, true);
    }


    public static ReportViewModel CreateDesignInstance (bool hasWarnings = false, bool hasErrors = false)
    {
      var report = new SynchronizationReport();
      report.ADelta = "This is the ADelta";
      report.BDelta = "This is the BDelta";

      var reportName = SynchronizationReportName.Create (Guid.NewGuid(), new DateTime (2000, 10, 10), hasWarnings, hasErrors);

      var proxy = new ReportProxy (reportName, () => report, "The profile name");

      return new ReportViewModel (proxy, NullSynchronizationReportRepository.Instance);
    }
  }
}