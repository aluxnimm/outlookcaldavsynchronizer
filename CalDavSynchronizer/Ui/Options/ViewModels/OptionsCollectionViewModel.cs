using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Ui.Options.ViewModels.Mapping;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public class OptionsCollectionViewModel : IOptionsViewModelParent
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly ObservableCollection<OptionsViewModelBase> _options = new ObservableCollection<OptionsViewModelBase>();
    private readonly IOptionsViewModelFactory _optionsViewModelFactory;
    private readonly bool _fixInvalidSettings;
    public event EventHandler<CloseEventArgs> CloseRequested;
    private readonly Func<Guid, string> _profileDataDirectoryFactory;

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
    }

    private void Close (bool shouldSaveNewOptions)
    {
      CloseRequested?.Invoke (this, new CloseEventArgs (shouldSaveNewOptions));
    }

    private void Add ()
    {
      var options = OptionTasks.CreateNewSynchronizationProfileOrNull();
      if (options != null)
      {
        foreach (var vm in _optionsViewModelFactory.Create (new[] { options }, _fixInvalidSettings))
          _options.Add (vm);
      }
    }

    public ICommand AddCommand { get; }
    public ICommand CloseCommand { get; }
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

    public void RequestDeletion (OptionsViewModelBase viewModel)
    {
      _options.Remove (viewModel);
    }

    public void RequestCopy (OptionsViewModelBase viewModel)
    {
      var options = new CalDavSynchronizer.Contracts.Options();
      viewModel.FillOptions (options);
      options.Id = Guid.NewGuid();
      options.Name += " (Copy)";

      var index = _options.IndexOf (viewModel) + 1;

      foreach (var vm in _optionsViewModelFactory.Create (new[] { options }, _fixInvalidSettings))
        _options.Insert (index, vm);
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
          viewModel.Options.Add (GenericOptionsViewModel.DesignInstance);
          viewModel.Options.Add (GenericOptionsViewModel.DesignInstance);
          return viewModel;
        }
      }
    }
  }
}