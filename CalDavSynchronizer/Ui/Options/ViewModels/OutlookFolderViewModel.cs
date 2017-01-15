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
using System.Windows;
using System.Windows.Input;
using CalDavSynchronizer.Ui.Options.Models;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public class OutlookFolderViewModel : ModelBase, IOptionsSection
  {
    private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly OptionsModel _model;
    private readonly IOptionTasks _optionTasks;

    public OutlookFolderViewModel(OptionsModel model, IOptionTasks optionTasks)
    {
      if (model == null) throw new ArgumentNullException(nameof(model));
      if (optionTasks == null) throw new ArgumentNullException(nameof(optionTasks));

      _model = model;
      _optionTasks = optionTasks;


      RegisterPropertyChangePropagation(_model, nameof(_model.EnableChangeTriggeredSynchronization), nameof(EnableChangeTriggeredSynchronization));
      RegisterPropertyChangePropagation(_model, nameof(_model.SelectedFolderOrNull), nameof(SelectedFolderName));
      SelectFolderCommand = new DelegateCommand(_ => SelectFolder());
    }

    public bool EnableChangeTriggeredSynchronization
    {
      get { return _model.EnableChangeTriggeredSynchronization; }
      set { _model.EnableChangeTriggeredSynchronization = value; }
    }

    public string SelectedFolderName => _model.SelectedFolderOrNull?.Name ?? "<MISSING>";


    public ICommand SelectFolderCommand { get; }

    public static OutlookFolderViewModel DesignInstance => new OutlookFolderViewModel(OptionsModel.DesignInstance, NullOptionTasks.Instance)
    {
      EnableChangeTriggeredSynchronization = true,
    };


    private void SelectFolder()
    {
      var folder = _optionTasks.PickFolderOrNull();
      if (folder != null)
      {
        if (!_model.SetFolder(folder))
        {
          string wrongFolderMessage = string.Format("Wrong ItemType in folder '{0}'. It should be a calendar, task or contact folder.", folder.Name);
          MessageBox.Show(wrongFolderMessage, "Configuration Error");
          return;
        }

        if (folder.DefaultItemType == OlItemType.olContactItem)
        {
          MessageBox.Show(
              "Synchronization of Outlook distribution lists is only supported for SOGo at the moment!",
              ComponentContainer.MessageBoxTitle,
              MessageBoxButton.OK,
              MessageBoxImage.Warning);
        }
      }
    }
  }
}