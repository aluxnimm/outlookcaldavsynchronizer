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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;

namespace CalDavSynchronizer.Ui.Options.ViewModels.Mapping
{
  class CustomPropertyMappingViewModel : ViewModelBase, ISubOptionsViewModel
  {
    private bool _mapCustomProperties;

    public string Name { get; } = "Custom properties mapping";


    public IEnumerable<ISubOptionsViewModel> SubOptions { get; } = new ISubOptionsViewModel[0];
    public List<PropertyMapping> Mappings { get; private set; }

    public bool MapCustomProperties
    {
      get { return _mapCustomProperties; }
      set
      {
        CheckedPropertyChange (ref _mapCustomProperties, value);
      }
    }

    public void SetOptions (Contracts.Options options)
    {
      throw new NotSupportedException ();
    }

    public void FillOptions (Contracts.Options options)
    {
      throw new NotSupportedException ();
    }

    public void SetOptions (IPropertyMappingConfiguration mappingConfiguration)
    {
      Mappings = new List<PropertyMapping>(mappingConfiguration.UserDefinedCustomPropertyMappings ?? new PropertyMapping[0]);
      MapCustomProperties = mappingConfiguration.MapCustomProperties;
    }

    public void FillOptions(IPropertyMappingConfiguration mappingConfiguration)
    {
      mappingConfiguration.UserDefinedCustomPropertyMappings = Mappings.ToArray();
      mappingConfiguration.MapCustomProperties = _mapCustomProperties;
    }

    public bool Validate(StringBuilder errorMessageBuilder)
    {
      if (Mappings.Any(m => !m.DavProperty.StartsWith("X-")))
      {
        errorMessageBuilder.AppendLine("- DAV X-Attributes for manual mapped properties have to start with 'X-'");
        return false;
      }

      return true;
    }

    public static CustomPropertyMappingViewModel DesignInstance => new CustomPropertyMappingViewModel()
    {
      MapCustomProperties = true,
      Mappings = new List<PropertyMapping>
      {
        new PropertyMapping {OutlookProperty = "OutlookName", DavProperty = "DavName"},
        new PropertyMapping {OutlookProperty = "OutlookSubject", DavProperty = "DavSubject"}
      }
    };

  }
}
