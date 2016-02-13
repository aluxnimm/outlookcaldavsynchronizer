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
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Utilities;
using GenSync.Logging;

namespace CalDavSynchronizer.Ui.Reports.ViewModels
{
  public partial class ReportViewModel : ViewModelBase
  {
    private readonly ISynchronizationReportRepository _reportRepository;
    private readonly ReportProxy _reportProxy;
    private string _asString;
    private bool _isSelected;
    private readonly IReportViewModelParent _parent;

    public ReportViewModel (ReportProxy reportProxy, ISynchronizationReportRepository reportRepository, IReportViewModelParent parent)
    {
      _reportRepository = reportRepository;
      _parent = parent;
      _reportProxy = reportProxy;

      OpenAEntityCommand = new DelegateCommand (parameter =>
      {
        OpenAEntity ((EntitySynchronizationReport) parameter);
      });

      OpenBEntityCommand = new DelegateCommand (parameter =>
      {
        OpenBEntity ((EntitySynchronizationReport) parameter);
      });

    }

    public SynchronizationReportName ReportName => _reportProxy.Name;
    public bool HasErrors => _reportProxy.Name.HasErrors;
    public bool HasWarnings => _reportProxy.Name.HasWarnings;
    public string ProfileName => _reportProxy.ProfileName;
    public DateTime StartTime => _reportProxy.Name.StartTime.ToLocalTime();
    public SynchronizationReport Report => _reportProxy.Value;

    public ICommand OpenAEntityCommand { get; }
    public ICommand OpenBEntityCommand { get; }

    private void OpenAEntity (EntitySynchronizationReport entitySynchronizationReport)
    {
      _parent.DiplayAEntity (_reportProxy.Value.ProfileId, entitySynchronizationReport.AId);
    }

    private void OpenBEntity (EntitySynchronizationReport entitySynchronizationReport)
    {
      _parent.DiplayBEntity (_reportProxy.Value.ProfileId, entitySynchronizationReport.BId);
    }


    public string AsString => _asString ?? (_asString = Serializer<SynchronizationReport>.Serialize (_reportProxy.Value));

    public bool IsSelected
    {
      get { return _isSelected; }
      set { CheckedPropertyChange (ref _isSelected, value, () => IsSelected); }
    }

    public void Delete ()
    {
      _reportRepository.DeleteReport (_reportProxy.Name);
    }
  }
}