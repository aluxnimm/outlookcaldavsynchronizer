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
using CalDavSynchronizer.Ui.Options.Models;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public abstract class OptionsViewModelBase : ModelBase, IOptionsViewModel
  {
    private readonly OptionsModel _model;
    private IEnumerable<IOptionsSection> _sections;
    private IEnumerable<ISubOptionsViewModel> _subOptions;
    private bool _isSelected;
    private bool _isExpanded;

    protected OptionsViewModelBase (IViewOptions options, OptionsModel model)
    {
      if (options == null) throw new ArgumentNullException (nameof (options));
      if (model == null) throw new ArgumentNullException(nameof(model));

      ViewOptions = options;
      _model = model;

      RegisterPropertyChangePropagation(_model, nameof(_model.Name), nameof(Name));
      RegisterPropertyChangePropagation(_model, nameof(_model.IsActive), nameof(IsActive));

    }

    public bool? IsMultipleOptionsTemplateViewModel { get; } = false;
    public abstract OlItemType? OutlookFolderType { get; } 

    public IEnumerable<IOptionsSection> Sections => _sections ?? (_sections = CreateSections());


    public bool IsActive
    {
      get { return _model.IsActive; }
      set { _model.IsActive = value; }
    }
    
    public IEnumerable<ISubOptionsViewModel> Items => _subOptions ?? (_subOptions = CreateSubOptions());
    IEnumerable<ITreeNodeViewModel> ITreeNodeViewModel.Items => Items;

    public bool SupportsIsActive { get; } = true;

    public string Name
    {
      get { return _model.Name; }
      set { _model.Name = value; }
    }

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

    public Guid Id { get; private set; }

    public abstract Contracts.Options GetOptionsOrNull();

    public abstract bool Validate(StringBuilder errorMessageBuilder);
  

    protected abstract IEnumerable<ISubOptionsViewModel> CreateSubOptions ();
    protected abstract IEnumerable<IOptionsSection> CreateSections ();
    public IViewOptions ViewOptions { get; }
  }
}