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
using System.Reflection;
using System.Security;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Ui.Options.Models;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  internal class GoogleServerSettingsViewModel : ModelBase, IOptionsSection
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);
    private readonly DelegateCommandWithoutCanExecuteDelegation _doAutoDiscoveryCommand;
    private readonly DelegateCommandWithoutCanExecuteDelegation _testConnectionCommand;

    private readonly OptionsModel _model;
    private readonly IOptionTasks _optionTasks;

    public GoogleServerSettingsViewModel (OptionsModel model, IOptionTasks optionTasks, IViewOptions viewOptions)
    {
      if (model == null) throw new ArgumentNullException(nameof(model));
      if (optionTasks == null) throw new ArgumentNullException(nameof(optionTasks));
      if (viewOptions == null) throw new ArgumentNullException(nameof(viewOptions));

      _model = model;
      _optionTasks = optionTasks;
      ViewOptions = viewOptions;
      _doAutoDiscoveryCommand = new DelegateCommandWithoutCanExecuteDelegation (_ => DoAutoDiscovery());
      _testConnectionCommand = new DelegateCommandWithoutCanExecuteDelegation (_ =>
      {
        ComponentContainer.EnsureSynchronizationContext();
        TestConnectionAsync (CalenderUrl);
      });


      RegisterPropertyChangePropagation(_model, nameof(_model.CalenderUrl), nameof(CalenderUrl));
      RegisterPropertyChangePropagation(_model, nameof(_model.EmailAddress), nameof(EmailAddress));
      RegisterPropertyChangePropagation(_model, nameof(_model.UseGoogleNativeApi), nameof(UseGoogleNativeApi));
      RegisterPropertyChangePropagation(_model, nameof(_model.UseGoogleNativeApiAvailable), nameof(UseGoogleNativeApiAvailable));

    }

    public ICommand DoAutoDiscoveryCommand => _doAutoDiscoveryCommand;
    public ICommand TestConnectionCommand => _testConnectionCommand;

    public string CalenderUrl
    {
      get { return _model.CalenderUrl; }
      set { _model.CalenderUrl = value; }
    }

    public string EmailAddress
    {
      get { return _model.EmailAddress; }
      set
      {
        _model.EmailAddress = value;
        _model.UserName = value;
      }
    }

    public bool UseGoogleNativeApi
    {
      get { return _model.UseGoogleNativeApi; }
      set { _model.UseGoogleNativeApi = value; }
    }

    public bool UseGoogleNativeApiAvailable => _model.UseGoogleNativeApiAvailable;
    
    private async void TestConnectionAsync (string testUrl)
    {
      _testConnectionCommand.SetCanExecute (false);
      _doAutoDiscoveryCommand.SetCanExecute (false);
      try
      {
        var newUrl = await _optionTasks.TestGoogleConnection (_model, testUrl);
        if (newUrl != testUrl)
          CalenderUrl = newUrl;
      }
      catch (Exception x)
      {
        s_logger.Error ("Exception while testing the connection.", x);
        string message = null;
        for (var ex = x; ex != null; ex = ex.InnerException)
          message += ex.Message + Environment.NewLine;
        MessageBox.Show (message, OptionTasks.ConnectionTestCaption);
      }
      finally
      {
        _testConnectionCommand.SetCanExecute (true);
        _doAutoDiscoveryCommand.SetCanExecute (true);
      }
    }

    private void DoAutoDiscovery ()
    {
      string testUrl;
      if (_model.SelectedFolderOrNull?.DefaultItemType == OlItemType.olTaskItem)
        testUrl = string.Empty;
      else
        testUrl = OptionTasks.GoogleDavBaseUrl;

      ComponentContainer.EnsureSynchronizationContext();
      TestConnectionAsync (testUrl);
    }

    public static GoogleServerSettingsViewModel DesignInstance => new GoogleServerSettingsViewModel(OptionsModel.DesignInstance, NullOptionTasks.Instance, OptionsCollectionViewModel.DesignViewOptions)
    {
      CalenderUrl = "http://calendar.url",
      EmailAddress = "bla@dot.com",
      UseGoogleNativeApi = true
    };

    public IViewOptions ViewOptions { get; }
  }
}