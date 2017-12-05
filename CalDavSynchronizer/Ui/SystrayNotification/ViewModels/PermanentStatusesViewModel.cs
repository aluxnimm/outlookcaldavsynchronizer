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
using System.Drawing;
using System.Windows.Forms;
using CalDavSynchronizer.Properties;
using CalDavSynchronizer.Ui.SystrayNotification.Views;
using GenSync.Logging;

namespace CalDavSynchronizer.Ui.SystrayNotification.ViewModels
{
  public class PermanentStatusesViewModel : IPermanentStatusesViewModel
  {
    private readonly IUiService _uiService;
    private readonly ICalDavSynchronizerCommands _commands;
    private readonly SynchronizationRunSummaryCache _summaryChache = new SynchronizationRunSummaryCache();
    private ITransientProfileStatusesViewModel _viewModelOrNull;

    public PermanentStatusesViewModel(IUiService uiService, ICalDavSynchronizerCommands commands, Contracts.Options[] options)
    {
      _commands = commands ?? throw new ArgumentNullException(nameof(commands));
      _uiService = uiService ?? throw new ArgumentNullException(nameof(uiService));
      _summaryChache.NotifyProfilesChanged(options);
    }

    public event EventHandler<OptionsEventArgs> OptionsRequesting;

    Contracts.Options[] OnOptionsRequesting()
    {
      var args = new OptionsEventArgs();
      OptionsRequesting?.Invoke(this, args);
      return args.Options ?? new Contracts.Options[0];
    }

    public void SetVisible()
    {
      if (_viewModelOrNull == null)
      {

        var viewModel = new TransientProfileStatusesViewModel(_commands, OnOptionsRequesting());
        foreach (var kv in _summaryChache.SummaryByProfileId)
        {
          if(kv.Value.HasValue)
            viewModel.Update(kv.Key, kv.Value.Value);
        }
        _viewModelOrNull = viewModel;
        _viewModelOrNull.Closing += _viewModelOrNull_Closing;
        _uiService.Show(viewModel);
      }
      else
      {
        _viewModelOrNull.BringToFront();
      }
    }

    private void _viewModelOrNull_Closing(object sender, EventArgs e)
    {
      _viewModelOrNull.Closing -= _viewModelOrNull_Closing;
      _viewModelOrNull.Dispose();
      _viewModelOrNull = null;
    }
    
    public void Update(Guid profileId, SynchronizationRunSummary summary)
    {
      _summaryChache.Update(profileId, summary);
      _viewModelOrNull?.Update(profileId, summary);
    }

    public void NotifyProfilesChanged(Contracts.Options[] profiles)
    {
      _summaryChache.NotifyProfilesChanged(profiles);
      _viewModelOrNull?.NotifyProfilesChanged(profiles);
    }
  }
}