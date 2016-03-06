using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CalDavSynchronizer.Contracts;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public class OptionsCollectionViewModel : IOptionsViewModelParent
  {
    private readonly ObservableCollection<OptionsViewModelBase> _options = new ObservableCollection<OptionsViewModelBase>();
    private readonly IOptionsViewModelFactory _optionsViewModelFactory;
    private readonly bool _fixInvalidSettings;
    public event EventHandler<CloseEventArgs> CloseRequested;

    public OptionsCollectionViewModel (NameSpace session, bool fixInvalidSettings, IOutlookAccountPasswordProvider outlookAccountPasswordProvider)
    {
      _fixInvalidSettings = fixInvalidSettings;
      if (session == null)
        throw new ArgumentNullException (nameof (session));

      _optionsViewModelFactory = new OptionsViewModelFactory (session, this, outlookAccountPasswordProvider);
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


    public CalDavSynchronizer.Contracts.Options[] OptionsCollection
    {
      get
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
      set
      {
        _options.Clear();
        foreach (var vm in _optionsViewModelFactory.Create (value, _fixInvalidSettings))
          _options.Add (vm);
      }
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
    }

    public static OptionsCollectionViewModel DesignInstance
    {
      get
      {
        {
          var viewModel = new OptionsCollectionViewModel (
              new DesignOutlookSession(),
              false,
              NullOutlookAccountPasswordProvider.Instance);
          viewModel.Options.Add (GenericOptionsViewModel.DesignInstance);
          viewModel.Options.Add (GenericOptionsViewModel.DesignInstance);
          return viewModel;
        }
      }
    }
  }
}