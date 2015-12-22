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
using CalDavSynchronizer.DataAccess;

namespace CalDavSynchronizer.Ui.Reports
{
  public class ReportsViewModel : ViewModelBase
  {
    private readonly List<ReportViewModel> _reports = new List<ReportViewModel>();

    public static ReportsViewModel Create (Dictionary<Guid, string> currentProfileNamesById, ISynchronizationReportRepository reportRepository)
    {
      var reportNames = reportRepository.GetAvailableReports();

      var reports = new List<ReportViewModel>();
      foreach (var reportName in reportNames)
      {
        string profileName;
        if (!currentProfileNamesById.TryGetValue (reportName.SyncronizationProfileId, out profileName))
          profileName = "<Not existing anymore>";

        var reportNameClosureLocal = reportName;
        var reportProxy = new ReportProxy (reportName, () => reportRepository.GetReport (reportNameClosureLocal), profileName);
        var reportViewModel = new ReportViewModel (reportProxy, reportRepository);
        reports.Add (reportViewModel);
      }

      return new ReportsViewModel (reports);
    }

    private ReportsViewModel (List<ReportViewModel> reports)
    {
      _reports = reports;
    }

    public List<ReportViewModel> Reports
    {
      get { return _reports; }
    }


    public static readonly ReportsViewModel DesignInstance;

    static ReportsViewModel ()
    {
      DesignInstance = new ReportsViewModel (
          new List<ReportViewModel>
          {
              ReportViewModel.CreateDesignInstance(),
              ReportViewModel.CreateDesignInstance(true),
              ReportViewModel.CreateDesignInstance(false, true),
              ReportViewModel.CreateDesignInstance(true, true),
          });
    }
  }
}