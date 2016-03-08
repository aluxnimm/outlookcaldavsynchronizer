using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using CalDavSynchronizer.Contracts;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public abstract class OptionsViewModelBase : ViewModelBase, IOptionsViewModel
  {
    private readonly IOptionsViewModelParent _parent;
    private bool _isActive;

    private OptionsDisplayType _displayType;
    private string _name;
    private IEnumerable<IOptionsSection> _sections;
    private IEnumerable<IOptionsViewModel> _subOptions;
    private bool _isSelected;

    protected OptionsViewModelBase (IOptionsViewModelParent parent)
    {
      if (parent == null)
        throw new ArgumentNullException (nameof (parent));

      _parent = parent;
      CopyCommand = new DelegateCommand (_ => _parent.RequestCopy (this));
      DeleteCommand = new DelegateCommand (_ => _parent.RequestDeletion (this));
      ClearCacheCommand = new DelegateCommand (_ => _parent.RequestCacheDeletion (this));
    }

    public IEnumerable<IOptionsSection> Sections => _sections ?? (_sections = CreateSections());

    public ICommand CopyCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ClearCacheCommand { get; }

    public bool IsActive
    {
      get { return _isActive; }
      set
      {
        _isActive = value;
        OnPropertyChanged();
      }
    }
    
    public IEnumerable<IOptionsViewModel> SubOptions => _subOptions ?? (_subOptions = CreateSubOptions());

    public string Name
    {
      get { return _name; }
      set
      {
        _name = value;
        OnPropertyChanged();
      }
    }

    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        _isSelected = value;
        OnPropertyChanged ();
      }
    }

    public Guid Id { get; private set; }

    public void SetOptions (CalDavSynchronizer.Contracts.Options options)
    {
      foreach (var section in Sections)
        section.SetOptions (options);

      foreach (var subViewModel in SubOptions)
        subViewModel.SetOptions (options);
      IsActive = !options.Inactive;
      Name = options.Name;
      Id = options.Id;
      _displayType = options.DisplayType;

      SetOptionsOverride (options);
    }

    protected virtual void SetOptionsOverride (CalDavSynchronizer.Contracts.Options options)
    {

    }

    public void FillOptions (CalDavSynchronizer.Contracts.Options options)
    {
      foreach (var section in Sections)
        section.FillOptions (options);

      foreach (var subViewModel in SubOptions)
        subViewModel.FillOptions (options);

      options.DisplayType = _displayType;
      options.Inactive = !IsActive;
      options.Name = Name;
      options.Id = Id;
    }

    public bool Validate (StringBuilder errorBuilder)
    {
      // foreach(var section in Sections)
      //  section.Validate (options);

      //foreach (var subViewModel in SubViewModels)
      //  subViewModel.Validate (options);
      return true;
    }

    protected abstract IEnumerable<IOptionsViewModel> CreateSubOptions ();
    protected abstract IEnumerable<IOptionsSection> CreateSections ();
  }
}