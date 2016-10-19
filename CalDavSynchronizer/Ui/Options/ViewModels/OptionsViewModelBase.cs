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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using CalDavSynchronizer.Contracts;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public abstract class OptionsViewModelBase : ViewModelBase, IOptionsViewModel
  {
    private readonly IOptionsViewModelParent _parent;
    private bool _isActive;

    private string _name;
    private IEnumerable<IOptionsSection> _sections;
    private IEnumerable<ISubOptionsViewModel> _subOptions;
    private bool _isSelected;

    protected OptionsViewModelBase (IOptionsViewModelParent parent)
    {
      if (parent == null)
        throw new ArgumentNullException (nameof (parent));

      _parent = parent;
      ClearCacheCommand = new DelegateCommand (_ => _parent.RequestCacheDeletion (this));
    }

    public bool? IsMultipleOptionsTemplateViewModel { get; } = false;
    public abstract OlItemType? OutlookFolderType { get; } 

    public IEnumerable<IOptionsSection> Sections => _sections ?? (_sections = CreateSections());

    public ICommand ClearCacheCommand { get; }

    public bool IsActive
    {
      get { return _isActive; }
      set
      {
        CheckedPropertyChange (ref _isActive, value);
      }
    }
    
    public IEnumerable<ISubOptionsViewModel> Items => _subOptions ?? (_subOptions = CreateSubOptions());
    IEnumerable<ITreeNodeViewModel> ITreeNodeViewModel.Items => Items;

    public bool SupportsIsActive { get; } = true;

    public string Name
    {
      get { return _name; }
      set
      {
        CheckedPropertyChange (ref _name, value);
      }
    }

    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        CheckedPropertyChange (ref _isSelected, value);
      }
    }

    public Guid Id { get; private set; }

    public void SetOptions (CalDavSynchronizer.Contracts.Options options)
    {
      foreach (var section in Sections)
        section.SetOptions (options);

      foreach (var subViewModel in Items)
        subViewModel.SetOptions (options);
      IsActive = !options.Inactive;
      Name = options.Name;
      Id = options.Id;

      SetOptionsOverride (options);
    }

    protected virtual void SetOptionsOverride (CalDavSynchronizer.Contracts.Options options)
    {

    }

    public Contracts.Options GetOptionsOrNull ()
    {
      var options = new Contracts.Options();

      foreach (var section in Sections)
        section.FillOptions (options);

      foreach (var subViewModel in Items)
        subViewModel.FillOptions (options);

      options.Inactive = !IsActive;
      options.Name = Name;
      options.Id = Id;

      return options;
    }

    public bool Validate (StringBuilder errorMessageBuilder)
    {
      bool isValid = true;

      foreach (var section in Sections)
        isValid &=  section.Validate (errorMessageBuilder);

      foreach (var subViewModel in Items)
        isValid &= subViewModel.Validate (errorMessageBuilder);

      return isValid;
    }

    protected abstract IEnumerable<ISubOptionsViewModel> CreateSubOptions ();
    protected abstract IEnumerable<IOptionsSection> CreateSections ();
  }
}