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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Ui.Options.Models;

namespace CalDavSynchronizer.Ui.Options.ViewModels.Mapping
{
  class CustomPropertyMappingViewModel : ModelBase, ITreeNodeViewModel
  {
    private readonly ICustomPropertiesMappingConfigurationModel _model;

    private bool _isSelected;
    private bool _isExpanded;

    public CustomPropertyMappingViewModel(ICustomPropertiesMappingConfigurationModel model)
    {
      if (model == null) throw new ArgumentNullException(nameof(model));

      _model = model;

      RegisterPropertyChangePropagation(model, nameof(model.MapCustomProperties), nameof(MapCustomProperties));

      Mappings = model.Mappings;
    }

    public string Name { get; } = Strings.Get($"Custom Properties Mapping");


    public IEnumerable<ITreeNodeViewModel> Items { get; } = new ITreeNodeViewModel[0];
    public ObservableCollection<PropertyMappingModel> Mappings { get; }

    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        CheckedPropertyChange(ref _isSelected, value);
      }
    }

    public bool IsExpanded
    {
      get { return _isExpanded; }
      set
      {
        CheckedPropertyChange(ref _isExpanded, value);
      }
    }

    public bool MapCustomProperties
    {
      get { return _model.MapCustomProperties; }
      set { _model.MapCustomProperties = value; }
    }


    public static CustomPropertyMappingViewModel DesignInstance
    {
      get
      {
        var customPropertyMappingViewModel = new CustomPropertyMappingViewModel(new TaskMappingConfigurationModel(new TaskMappingConfiguration()))
        {
          MapCustomProperties = true,
        };

        customPropertyMappingViewModel.Mappings.Add(new PropertyMappingModel(new PropertyMapping {OutlookProperty = "OName", DavProperty = "DName"}));
        customPropertyMappingViewModel.Mappings.Add(new PropertyMappingModel(new PropertyMapping { OutlookProperty = "OSubject", DavProperty = "DSubject" }));

        return customPropertyMappingViewModel;
      }
    }
  }
}
