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
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CalDavSynchronizer.DataAccess;
using GenSync.Logging;
using Microsoft.Win32;

namespace CalDavSynchronizer.Ui.Reports.ViewModels
{
  public class ReportsViewModel : ViewModelBase, IReportViewModelParent
  {
    private readonly ObservableCollection<ReportViewModel> _reports = new ObservableCollection<ReportViewModel>();
    private readonly DelegateCommand _deleteSelectedCommand;
    private readonly DelegateCommand _saveSelectedCommand;
    private readonly ISynchronizationReportRepository _reportRepository;
    private readonly Dictionary<Guid, string> _currentProfileNamesById;
    private readonly IReportsViewModelParent _parent;

    public event EventHandler RequiresBringToFront;

    public virtual void RequireBringToFront ()
    {
      var handler = RequiresBringToFront;
      if (handler != null)
        handler (this, EventArgs.Empty);
    }

    public event EventHandler ReportsClosed;

    public virtual void NotifyReportsClosed ()
    {
      var handler = ReportsClosed;
      if (handler != null)
        handler (this, EventArgs.Empty);
    }

    public ReportsViewModel (
        ISynchronizationReportRepository reportRepository,
        Dictionary<Guid, string> currentProfileNamesById, IReportsViewModelParent parent)
    {
      _reportRepository = reportRepository;
      _currentProfileNamesById = currentProfileNamesById;
      _parent = parent;
      _deleteSelectedCommand = new DelegateCommand (DeleteSelected, _ => Reports.Any (r => r.IsSelected));
      _saveSelectedCommand = new DelegateCommand (SaveSelected, _ => Reports.Any (r => r.IsSelected));

      foreach (var reportName in reportRepository.GetAvailableReports())
        AddReportViewModel (reportName);

      // Regarding to race conditions it doesn't matter when the handler is added
      // since everything happens in the ui thread
      _reportRepository.ReportAdded += ReportRepository_ReportAdded;

      if (Reports.Count > 0)
        Reports[0].IsSelected = true;
    }

    private void ReportRepository_ReportAdded (object sender, ReportAddedEventArgs e)
    {
      AddReportViewModel (e.ReportName, e.Report);
    }

    private void AddReportViewModel (SynchronizationReportName reportName)
    {
      string profileName;
      if (!_currentProfileNamesById.TryGetValue (reportName.SyncronizationProfileId, out profileName))
        profileName = "<Not existing anymore>";

      var reportProxy = new ReportProxy (reportName, () => _reportRepository.GetReport (reportName), profileName);
      var reportViewModel = new ReportViewModel (reportProxy, _reportRepository, this);
      _reports.Add (reportViewModel);
    }


    private void AddReportViewModel (SynchronizationReportName reportName, SynchronizationReport report)
    {
      string profileName;
      if (!_currentProfileNamesById.TryGetValue (reportName.SyncronizationProfileId, out profileName))
        profileName = "<Not existing anymore>";

      var reportProxy = new ReportProxy (reportName, () => report, profileName);
      var reportViewModel = new ReportViewModel (reportProxy, _reportRepository, this);
      _reports.Add (reportViewModel);
    }

    private void SaveSelected (object parameter)
    {
      SaveFileDialog dialog = new SaveFileDialog();
      dialog.Filter = "Zip archives|*.zip";
      dialog.FileName = "SynchronizationReports.zip";
      dialog.Title = "Save selected reports";
      if (dialog.ShowDialog() ?? false)
      {
        using (var fileStream = new FileStream (dialog.FileName, FileMode.Create))
        {
          using (var archive = new ZipArchive (fileStream, ZipArchiveMode.Create))
          {
            foreach (var report in Reports.Where (r => r.IsSelected))
            {
              var entry = archive.CreateEntry (report.ReportName.ToString(), CompressionLevel.Optimal);
              using (var entryStream = entry.Open())
              {
                using (var reportStream = _reportRepository.GetReportStream (report.ReportName))
                {
                  reportStream.CopyTo (entryStream);
                }
              }
            }
          }
        }
      }
    }

    private void DeleteSelected (object parameter)
    {
      for (int i = Reports.Count - 1; i >= 0; i--)
      {
        var report = Reports[i];
        if (report.IsSelected)
        {
          report.Delete();
          Reports.RemoveAt (i);
        }
      }
    }

    public ObservableCollection<ReportViewModel> Reports => _reports;
    public DelegateCommand DeleteSelectedCommand => _deleteSelectedCommand;
    public DelegateCommand SaveSelectedCommand => _saveSelectedCommand;

    public static ReportsViewModel DesignInstance
    {
      get
      {
        var designInstance = new ReportsViewModel (
            NullSynchronizationReportRepository.Instance,
            new Dictionary<Guid, string>(),
            NullReportsViewModelParent.Instance);

        designInstance._reports.Add (ReportViewModel.CreateDesignInstance());
        designInstance._reports.Add (ReportViewModel.CreateDesignInstance (true));
        designInstance._reports.Add (ReportViewModel.CreateDesignInstance (false, true));
        designInstance._reports.Add (ReportViewModel.CreateDesignInstance (true, true));

        return designInstance;
      }
    }

    public void DiplayAEntity (Guid synchronizationProfileId, string entityId)
    {
      _parent.DiplayAEntity (synchronizationProfileId, entityId);
    }

    public void DiplayBEntity (Guid synchronizationProfileId, string entityId)
    {
      _parent.DiplayBEntityAsync (synchronizationProfileId, entityId);
    }

    public void ShowLatestSynchronizationReportCommand (Guid profileId)
    {
      foreach (var report in _reports)
        report.IsSelected = false;

      ReportViewModel latestReport = null;

      foreach (var report in _reports.Where (r => r.ProfileId == profileId))
        if (latestReport == null || report.StartTime > latestReport.StartTime)
          latestReport = report;
  
      if (latestReport != null)
        latestReport.IsSelected = true;
    }
  }
}