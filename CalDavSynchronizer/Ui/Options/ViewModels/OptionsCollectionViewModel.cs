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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Ui.Options.ViewModels.Mapping;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public class OptionsCollectionViewModel : IOptionsViewModelParent, ISynchronizationProfilesViewModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly ObservableCollection<OptionsViewModelBase> _options = new ObservableCollection<OptionsViewModelBase>();
    private readonly IOptionsViewModelFactory _optionsViewModelFactory;
    private readonly bool _fixInvalidSettings;
    public event EventHandler<CloseEventArgs> CloseRequested;
    private readonly Func<Guid, string> _profileDataDirectoryFactory;
    public event EventHandler RequestBringIntoView;

    public OptionsCollectionViewModel (
      NameSpace session,
      bool fixInvalidSettings,
      IOutlookAccountPasswordProvider outlookAccountPasswordProvider,
      IReadOnlyList<string> availableEventCategories, 
      Func<Guid, string> profileDataDirectoryFactory)
    {
      _fixInvalidSettings = fixInvalidSettings;
      _profileDataDirectoryFactory = profileDataDirectoryFactory;
      if (session == null)
        throw new ArgumentNullException (nameof (session));
      if (profileDataDirectoryFactory == null)
        throw new ArgumentNullException (nameof (profileDataDirectoryFactory));


      _optionsViewModelFactory = new OptionsViewModelFactory (
        session,
        this,
        outlookAccountPasswordProvider,
        availableEventCategories);
      AddCommand = new DelegateCommand (_ => Add());
      CloseCommand = new DelegateCommand (shouldSaveNewOptions => Close((bool)shouldSaveNewOptions));
      DeleteSelectedCommand = new DelegateCommand (_ => DeleteSelected (), _ => CanDeleteSelected);
      CopySelectedCommand = new DelegateCommand (_ => CopySelected (), _ => CanCopySelected);
      MoveSelectedUpCommand = new DelegateCommand (_ => MoveSelectedUp (), _ => CanMoveSelectedUp);
      MoveSelectedDownCommand = new DelegateCommand (_ => MoveSelectedDown (), _ => CanMoveSelectedDown);
    }

    private bool CanMoveSelectedDown => SelectedOrNull != null;
    private bool CanMoveSelectedUp => SelectedOrNull != null;
    private bool CanCopySelected => SelectedOrNull != null;
    private bool CanDeleteSelected => SelectedOrNull != null;

    private void MoveSelectedDown ()
    {
      var selected = SelectedOrNull;
      if (selected != null)
      {
        var index = _options.IndexOf (selected);
        var newIndex = Math.Min (index + 1, _options.Count - 1);
        System.Diagnostics.Debug.WriteLine ($"{index} => {newIndex}");
        _options.Move (index, newIndex);
        selected.IsSelected = true;
      }
    }

    private void MoveSelectedUp ()
    {
      var selected = SelectedOrNull;
      if (selected != null)
      {
        var index = _options.IndexOf (selected);
        var newIndex = Math.Max (index - 1, 0);
        System.Diagnostics.Debug.WriteLine ($"{index} => {newIndex}");
        _options.Move (index, newIndex);
        selected.IsSelected = true;
      }
    }

    private void CopySelected ()
    {
      var selected = SelectedOrNull;
      if (selected != null)
        Copy (selected);
    }

    private void DeleteSelected ()
    {
      var selected = SelectedOrNull;
      if (selected != null)
        Delete (selected);
    }

    OptionsViewModelBase SelectedOrNull => _options.FirstOrDefault (o => o.IsSelected);

    private void Close (bool shouldSaveNewOptions)
    {
      if (shouldSaveNewOptions)
      {
        OptionsViewModelBase firstViewModelWithError;
        string errorMessage;
        if (!Validate (out errorMessage, out firstViewModelWithError))
        {
          MessageBox.Show (errorMessage, "Some Options contain invalid Values",MessageBoxButton.OK,MessageBoxImage.Error);
          if (firstViewModelWithError != null)
            firstViewModelWithError.IsSelected = true;
          return;
        }
      }

      CloseRequested?.Invoke (this, new CloseEventArgs (shouldSaveNewOptions));
    }

    private bool Validate (out string errorMessage, out OptionsViewModelBase firstViewModelWithError)
    {
      StringBuilder errorMessageBuilder = new StringBuilder ();
      bool isValid = true;
      firstViewModelWithError = null;

      foreach (var viewModel in _options)
      {
        StringBuilder currentControlErrorMessageBuilder = new StringBuilder ();

        if (!viewModel.Validate (currentControlErrorMessageBuilder))
        {
          if (errorMessageBuilder.Length > 0)
            errorMessageBuilder.AppendLine ();

          errorMessageBuilder.AppendFormat ("Profile '{0}'", viewModel.Name);
          errorMessageBuilder.AppendLine ();
          errorMessageBuilder.Append (currentControlErrorMessageBuilder);

          isValid = false;
          if (firstViewModelWithError == null)
            firstViewModelWithError = viewModel;
        }
      }

      errorMessage = errorMessageBuilder.ToString ();
      return isValid;
    }

    private void Add ()
    {
      var options = OptionTasks.CreateNewSynchronizationProfileOrNull();
      if (options != null)
      {
        foreach (var vm in _optionsViewModelFactory.Create(new[] {options}, _fixInvalidSettings))
          _options.Add (vm);
        ShowProfile (options.Id);
      }
    }

    public ICommand AddCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand DeleteSelectedCommand { get; }
    public ICommand CopySelectedCommand { get; }
    public ICommand MoveSelectedUpCommand { get; }
    public ICommand MoveSelectedDownCommand { get; }
    public ObservableCollection<OptionsViewModelBase> Options => _options;


    public void SetOptionsCollection (Contracts.Options[] value, Guid? initialSelectedProfileId = null)
    {
      _options.Clear();
      foreach (var vm in _optionsViewModelFactory.Create (value, _fixInvalidSettings))
        _options.Add (vm);

      var initialSelectedProfile =
          (initialSelectedProfileId != null ? _options.FirstOrDefault (o => o.Id == initialSelectedProfileId.Value) : null)
          ?? _options.FirstOrDefault (o => o.IsActive)
          ?? _options.FirstOrDefault();

      if (initialSelectedProfile != null)
        initialSelectedProfile.IsSelected = true;
    }

    public Contracts.Options[] GetOptionsCollection ()
    {
      var optionsCollection = new List<CalDavSynchronizer.Contracts.Options>();
      foreach (var viewModel in _options)
      {
        var options = new CalDavSynchronizer.Contracts.Options();
        viewModel.FillOptions (options);
        optionsCollection.Add (options);
      }
      return optionsCollection.ToArray();
    }

    private void Delete (OptionsViewModelBase viewModel)
    {
      var index = _options.IndexOf (viewModel);
      _options.Remove (viewModel);
      if (_options.Count > 0)
        _options[Math.Max (0, Math.Min (_options.Count - 1, index))].IsSelected = true;
    }

    private void Copy (OptionsViewModelBase viewModel)
    {
      var options = new CalDavSynchronizer.Contracts.Options();
      viewModel.FillOptions (options);
      options.Id = Guid.NewGuid();
      options.Name += " (Copy)";

      var index = _options.IndexOf (viewModel) + 1;

      foreach (var vm in _optionsViewModelFactory.Create (new[] { options }, _fixInvalidSettings))
        _options.Insert (index, vm);

      ShowProfile (options.Id);
    }

    public void RequestCacheDeletion (OptionsViewModelBase viewModel)
    {
     
        s_logger.InfoFormat ("Deleting cache for profile '{0}'", viewModel.Name);

        var profileDataDirectory = _profileDataDirectoryFactory (viewModel.Id);
        if (Directory.Exists (profileDataDirectory))
          Directory.Delete (profileDataDirectory, true);

        MessageBox.Show ("A new intial sync will be performed with the next sync run!", "Profile cache deleted",MessageBoxButton.OK, MessageBoxImage.Information);
    
    }

    public static OptionsCollectionViewModel DesignInstance
    {
      get
      {
        {
          var viewModel = new OptionsCollectionViewModel (
              new DesignOutlookSession(),
              false,
              NullOutlookAccountPasswordProvider.Instance,
              new[] {"Cat1","Cat2"},
              _ => string.Empty);
          var genericOptionsViewModel = GenericOptionsViewModel.DesignInstance;
          genericOptionsViewModel.IsSelected = true;
          viewModel.Options.Add (genericOptionsViewModel);
          viewModel.Options.Add (GenericOptionsViewModel.DesignInstance);
          return viewModel;
        }
      }
    }

    public void ShowProfile (Guid value)
    {
      var selectedProfile = _options.FirstOrDefault (o => o.Id == value);

      if (selectedProfile != null)
        selectedProfile.IsSelected = true;
    }

    public void BringToFront ()
    {
      RequestBringIntoView?.Invoke (this, EventArgs.Empty);
    }
  }
}