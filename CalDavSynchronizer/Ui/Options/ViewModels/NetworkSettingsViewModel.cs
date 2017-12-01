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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.ProfileTypes;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Utilities;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public class NetworkSettingsViewModel : ModelBase, ISubOptionsViewModel
  {
    private readonly OptionsModel _model;
 
    private bool _isSelected;
    private bool _isExpanded;

    public NetworkSettingsViewModel(OptionsModel model)
    {
      if (model == null) throw new ArgumentNullException(nameof(model));
      _model = model;
      ModelOptions = model.ModelFactory.ModelOptions;

      RegisterPropertyChangePropagation(_model, nameof(_model.CloseConnectionAfterEachRequest), nameof(CloseConnectionAfterEachRequest));
      RegisterPropertyChangePropagation(_model, nameof(_model.PreemptiveAuthentication), nameof(PreemptiveAuthentication));
      RegisterPropertyChangePropagation(_model, nameof(_model.ProxyUseDefault), nameof(ProxyUseDefault));
      RegisterPropertyChangePropagation(_model, nameof(_model.ProxyUseManual), nameof(ProxyUseManual));
      RegisterPropertyChangePropagation(_model, nameof(_model.ProxyUrl), nameof(ProxyUrl));
      RegisterPropertyChangePropagation(_model, nameof(_model.ProxyUserName), nameof(ProxyUserName));
      RegisterPropertyChangePropagation(_model, nameof(_model.ProxyPassword), nameof(ProxyPassword));
      RegisterPropertyChangePropagation(_model, nameof(_model.ForceBasicAuthentication), nameof(ForceBasicAuthentication));

    }

    public ProfileModelOptions ModelOptions { get; }

    public bool CloseConnectionAfterEachRequest
    {
      get { return _model.CloseConnectionAfterEachRequest; }
      set { _model.CloseConnectionAfterEachRequest = value; }
    }

    public bool PreemptiveAuthentication
    {
      get { return _model.PreemptiveAuthentication; }
      set { _model.PreemptiveAuthentication = value; }
    }

    public bool ProxyUseDefault
    {
      get { return _model.ProxyUseDefault; }
      set { _model.ProxyUseDefault = value; }
    }

    public bool ProxyUseManual
    {
      get { return _model.ProxyUseManual; }
      set { _model.ProxyUseManual = value; }
    }

    public string ProxyUrl
    {
      get { return _model.ProxyUrl; }
      set { _model.ProxyUrl = value; }
    }

    public string ProxyUserName
    {
      get { return _model.ProxyUserName; }
      set { _model.ProxyUserName = value; }
    }

    public SecureString ProxyPassword
    {
      get { return _model.ProxyPassword; }
      set { _model.ProxyPassword = value; }
    }

    public bool ForceBasicAuthentication
    {
      get { return _model.ForceBasicAuthentication; }
      set { _model.ForceBasicAuthentication = value; }
    }

    public static NetworkSettingsViewModel DesignInstance => new NetworkSettingsViewModel(OptionsModel.DesignInstance)
    {
      CloseConnectionAfterEachRequest = true,
      PreemptiveAuthentication = true,
      ForceBasicAuthentication = true,
      ProxyPassword = SecureStringUtility.ToSecureString("proxypassword"),
      ProxyUrl = "proxyurl",
      ProxyUseDefault = true,
      ProxyUseManual = true,
      ProxyUserName = "proxyusername"
    };




    public string Name => "Network settings";


    public IEnumerable<ITreeNodeViewModel> Items { get; } = new ITreeNodeViewModel[0];

  

    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        CheckedPropertyChange (ref _isSelected, value);
      }
    }

    public bool IsExpanded
    {
      get { return _isExpanded; }
      set
      {
        CheckedPropertyChange (ref _isExpanded, value);
      }
    }
  }
}