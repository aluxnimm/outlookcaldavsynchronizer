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
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.ProfileTypes;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels.Mapping;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public class OptionsCollectionViewModel : ModelBase, IOptionsViewModelParent, ISynchronizationProfilesViewModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly ObservableCollection<IOptionsViewModel> _options = new ObservableCollection<IOptionsViewModel> ();
    private readonly IProfileTypeRegistry _profileTypeRegistry;
    private readonly IReadOnlyDictionary<IProfileType, IProfileModelFactory> _profileModelFactoriesByType;
    private readonly bool _expandAllSyncProfiles;
    private readonly IUiService _uiService;
    public event EventHandler<CloseEventArgs> CloseRequested;
    private readonly Func<Guid, string> _profileDataDirectoryFactory;
    private readonly IOptionTasks _optionTasks;

    public event EventHandler RequestBringIntoView;

    public OptionsCollectionViewModel (
      bool expandAllSyncProfiles,
      Func<Guid, string> profileDataDirectoryFactory,
      IUiService uiService,
      IOptionTasks optionTasks,
      IProfileTypeRegistry profileTypeRegistry,
      Func<IOptionsViewModelParent, IProfileType, IProfileModelFactory> profileModelFactoryFactory,
      IViewOptions viewOptions)
    {
      _optionTasks = optionTasks;
      ViewOptions = viewOptions;
      _profileDataDirectoryFactory = profileDataDirectoryFactory;
      _uiService = uiService;
      if (profileDataDirectoryFactory == null)
        throw new ArgumentNullException (nameof (profileDataDirectoryFactory));
      if (optionTasks == null) throw new ArgumentNullException(nameof(optionTasks));
      if (viewOptions == null) throw new ArgumentNullException(nameof(viewOptions));

      _expandAllSyncProfiles = expandAllSyncProfiles;

      _profileTypeRegistry = profileTypeRegistry;
      _profileModelFactoriesByType = profileTypeRegistry.AllTypes.ToDictionary(t => t, t => profileModelFactoryFactory(this, t));

      RegisterPropertyChangeHandler(viewOptions, nameof(viewOptions.IsAdvancedViewEnabled), () =>
      {
        if (!viewOptions.IsAdvancedViewEnabled) OnAdvancedViewDisabled();
      });

      AddCommand = new DelegateCommand (_ => Add());
      AddMultipleCommand = new DelegateCommand (_ => AddMultiple());
      CloseCommand = new DelegateCommand (shouldSaveNewOptions => Close((bool)shouldSaveNewOptions));
      DeleteSelectedCommand = new DelegateCommandHandlingRequerySuggested (_ => DeleteSelected (), _ => CanDeleteSelected);
      ClearCacheOfSelectedCommand = new DelegateCommandHandlingRequerySuggested (_ => ClearCacheOfSelected (), _ => CanClearCacheOfSelected);
      CopySelectedCommand = new DelegateCommandHandlingRequerySuggested (_ => CopySelected (), _ => CanCopySelected);
      MoveSelectedUpCommand = new DelegateCommandHandlingRequerySuggested (_ => MoveSelectedUp (), _ => CanMoveSelectedUp);
      MoveSelectedDownCommand = new DelegateCommandHandlingRequerySuggested (_ => MoveSelectedDown (), _ => CanMoveSelectedDown);
      OpenProfileDataDirectoryCommand = new DelegateCommandHandlingRequerySuggested (_ => OpenProfileDataDirectory (), _ => CanOpenProfileDataDirectory);
      ExpandAllCommand = new DelegateCommandHandlingRequerySuggested (_ => ExpandAll (), _ => _options.Count > 0);
      CollapseAllCommand = new DelegateCommandHandlingRequerySuggested (_ => CollapseAll (), _ => _options.Count > 0);
      ExportAllCommand = new DelegateCommandHandlingRequerySuggested (_ => ExportAll (), _ => _options.Count > 0);
      ImportCommand = new DelegateCommandHandlingRequerySuggested (_ => Import (), _ => true);
    }

    public IViewOptions ViewOptions { get; }

    private void OnAdvancedViewDisabled()
    {
      var optionWithSelectedChild = _options.FirstOrDefault(o => o.Items.Any(IsSelfOrAncestorSelected));
      if (optionWithSelectedChild != null)
        optionWithSelectedChild.IsSelected = true;
    }

    private static bool IsSelfOrAncestorSelected(ITreeNodeViewModel viewModel)
    {
      return viewModel.IsSelected || viewModel.Items.Any(IsSelfOrAncestorSelected);
    }

    private void Import()
    {
      var fileName = _uiService.ShowOpenDialog (Strings.Get($"Import profiles"));
      if (fileName == null)
        return;

      var reportBuilder = new StringBuilder ();
      var newOptions = _optionTasks.LoadOptions(fileName);
      var mergedOptions = _optionTasks.ProfileExportProcessor.PrepareAndMergeForImport(GetOptionsCollection(), newOptions, s => reportBuilder.AppendLine (s));

      SetOptionsCollection(mergedOptions);

      reportBuilder.AppendLine (Strings.Get($"Sucessfully imported {newOptions.Length} profile(s) from '{fileName}'."));

      _uiService.ShowReport (Strings.Get($"Export profiles"), reportBuilder.ToString ());
    }

    private void ExportAll()
    {
      var reportBuilder = new StringBuilder();

      var profiles = GetOptionsCollection();
      _optionTasks.ProfileExportProcessor.PrepareForExport(profiles, s => reportBuilder.AppendLine(s));

      var fileName = _uiService.ShowSaveDialog(Strings.Get($"Export profiles"));
      if (fileName != null)
      {
        _optionTasks.SaveOptions(profiles, fileName);
        reportBuilder.AppendLine (Strings.Get($"Sucessfully exported {profiles.Length} profile(s) to '{fileName}'."));
      }
      else
      {
        reportBuilder.AppendLine(Strings.Get($"Export cancelled by user."));
      }

      _uiService.ShowReport(Strings.Get($"Export profiles"), reportBuilder.ToString());
    }

    private bool CanMoveSelectedDown => SelectedOrNull != null;
    private bool CanMoveSelectedUp => SelectedOrNull != null;
    private bool CanCopySelected => SelectedOrNull != null;
    private bool CanDeleteSelected => SelectedOrNull != null;
    private bool CanClearCacheOfSelected => SelectedOrNull != null;
    private bool CanOpenProfileDataDirectory => SelectedOrNull != null;

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

    private void ClearCacheOfSelected()
    {
      var selected = SelectedOrNull;
      if (selected != null)
        ClearCache(selected);
    }

    private void OpenProfileDataDirectory()
    {
      var selected = SelectedOrNull;
      if (selected != null)
      {
        var profileDataDirectory = _profileDataDirectoryFactory (selected.Model.Id);
        if (Directory.Exists(profileDataDirectory))
          System.Diagnostics.Process.Start(profileDataDirectory);
        else
          MessageBox.Show(Strings.Get($"The selected profile has no data directory."), Strings.Get($"Operation aborted"), MessageBoxButton.OK);
      }
    }

    private void CollapseAll()
    {
      ExpandCollapseAll(_options, false);
    }

    private void ExpandAll ()
    {
      ExpandCollapseAll (_options, true);
    }

    private void ExpandCollapseAll (IEnumerable<ITreeNodeViewModel> nodes, bool isExpanded)
    {
      foreach (var node in nodes)
      {
        ExpandCollapseAll(node.Items, isExpanded);
        node.IsExpanded = isExpanded;
      }
    }

    IOptionsViewModel SelectedOrNull => _options.FirstOrDefault (o => o.IsSelected);

    private void Close (bool shouldSaveNewOptions)
    {
      if (shouldSaveNewOptions)
      {
        IOptionsViewModel firstViewModelWithError;
        string errorMessage;
        if (!Validate (out errorMessage, out firstViewModelWithError))
        {
          _uiService.ShowErrorDialog (errorMessage, Strings.Get($"Some options contain invalid values"));
          if (firstViewModelWithError != null)
            firstViewModelWithError.IsSelected = true;
          return;
        }
      }

      CloseRequested?.Invoke (this, new CloseEventArgs (shouldSaveNewOptions));
    }

    private bool Validate (out string errorMessage, out IOptionsViewModel firstViewModelWithError)
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

          errorMessageBuilder.AppendFormat (Strings.Get($"Profile '{viewModel.Name}'"));
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
      var type = QueryProfileType();
      if (type != null)
      {
        var profileModelFactoryFactory = _profileModelFactoriesByType[type];
        var viewModel = profileModelFactoryFactory.CreateViewModel(profileModelFactoryFactory.CreateModelFromData(type.CreateOptions()));
        _options.Add(viewModel);
        ShowProfile(viewModel.Model.Id);
      }
    }

    private void AddMultiple ()
    {
      var type = QueryProfileType();
      if (type != null)
      {
        var profileModelFactoryFactory = _profileModelFactoriesByType[type];
        var viewModel = profileModelFactoryFactory.CreateTemplateViewModel();
        _options.Add(viewModel);
        ShowProfile(viewModel.Model.Id);
      }
    }
    
    private IProfileType QueryProfileType()
    {
      return _uiService.QueryProfileType(_profileTypeRegistry.AllTypes);
    }
    
    public ICommand AddCommand { get; }
    public ICommand AddMultipleCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand DeleteSelectedCommand { get; }
    public ICommand ClearCacheOfSelectedCommand { get; }
    public ICommand CopySelectedCommand { get; }
    public ICommand MoveSelectedUpCommand { get; }
    public ICommand MoveSelectedDownCommand { get; }
    public ICommand OpenProfileDataDirectoryCommand { get; }
    public ICommand ExpandAllCommand { get; }
    public ICommand CollapseAllCommand { get; }
    public ICommand ExportAllCommand { get; }
    public ICommand ImportCommand { get; }

    public ObservableCollection<IOptionsViewModel> Options => _options;


    public void SetOptionsCollection (Contracts.Options[] value, Guid? initialSelectedProfileId = null)
    {
      _options.Clear();

      foreach (var data in value)
      {
        var profileType = _profileTypeRegistry.DetermineType(data);
        var profileModelFactory = _profileModelFactoriesByType[profileType];
        _options.Add(profileModelFactory.CreateViewModel(profileModelFactory.CreateModelFromData(data)));
      }

      var initialSelectedProfile =
          (initialSelectedProfileId != null ? _options.FirstOrDefault (o => o.Model.Id == initialSelectedProfileId.Value) : null)
          ?? _options.FirstOrDefault (o => o.IsActive)
          ?? _options.FirstOrDefault();

      if (initialSelectedProfile != null)
        initialSelectedProfile.IsSelected = true;

      if (_options.Count > 0 && _expandAllSyncProfiles)
        ExpandAll();
    }

    public Contracts.Options[] GetOptionsCollection ()
    {
      return _options.Where(o => !o.IsMultipleOptionsTemplateViewModel).Select(o => o.Model.CreateData()).ToArray();
    }

    public OneTimeChangeCategoryTask[] GetOneTimeTasks()
    {
      var oneTimeTasks = new List<OneTimeChangeCategoryTask>();
      foreach (var options in _options.Where(o => !o.IsMultipleOptionsTemplateViewModel))
        options.Model.AddOneTimeTasks(oneTimeTasks.Add);
      return oneTimeTasks.ToArray();
    }

    private void Delete (IOptionsViewModel viewModel)
    {
      var index = _options.IndexOf (viewModel);
      _options.Remove (viewModel);
      if (_options.Count > 0)
        _options[Math.Max (0, Math.Min (_options.Count - 1, index))].IsSelected = true;
    }

    private void Copy(IOptionsViewModel viewModel)
    {
      var modelCopy = viewModel.Model.Clone();
      modelCopy.Name += " (Copy)";

      var index = _options.IndexOf(viewModel) + 1;

      var newViewModel = modelCopy.ModelFactory.CreateViewModel(modelCopy);
      _options.Insert(index, newViewModel);

      ShowProfile(newViewModel.Model.Id);
    }

    private void ClearCache(IOptionsViewModel viewModel)
    {
        s_logger.InfoFormat ("Deleting cache for profile '{0}'", viewModel.Name);

        var profileDataDirectory = _profileDataDirectoryFactory (viewModel.Model.Id);
        if (Directory.Exists (profileDataDirectory))
          Directory.Delete (profileDataDirectory, true);

        MessageBox.Show (Strings.Get($"A new intial sync will be performed with the next sync run!"), Strings.Get($"Profile cache deleted"),MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void RequestRemoval (IOptionsViewModel viewModel)
    {
      Delete (viewModel);
    }

    public void RequestAdd (IReadOnlyCollection<OptionsModel> options)
    {
      foreach (var data in options)
      {
        _options.Add(data.ModelFactory.CreateViewModel(data));
      }

      if (options.Any())
        ShowProfile (options.First().Id);
    }

    public static IViewOptions DesignViewOptions => new ViewOptions (false);

    public static OptionsCollectionViewModel DesignInstance
    {
      get
      {
        {
          var viewModel = new OptionsCollectionViewModel(
            true,
            _ => string.Empty,
            NullUiService.Instance,
            NullOptionTasks.Instance,
            ProfileTypeRegistry.Instance,
            (parent,type) => DesignProfileModelFactory.Instance,
            DesignViewOptions);
              
          var genericOptionsViewModel = GenericOptionsViewModel.DesignInstance;
          genericOptionsViewModel.IsSelected = true;
          viewModel.Options.Add(genericOptionsViewModel);
          viewModel.Options.Add(GenericOptionsViewModel.DesignInstance);
          return viewModel;
        }
      }
    }

    public void ShowProfile (Guid value)
    {
      var selectedProfile = _options.FirstOrDefault (o => o.Model.Id == value);

      if (selectedProfile != null)
        selectedProfile.IsSelected = true;
    }

    public void BringToFront ()
    {
      RequestBringIntoView?.Invoke (this, EventArgs.Empty);
    }
  }
}