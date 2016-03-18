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
using CalDavSynchronizer.Contracts;

namespace CalDavSynchronizer.Ui.Options.ViewModels.Mapping
{
  public class ContactMappingConfigurationViewModel : ViewModelBase, IOptionsViewModel
  {
    private bool _mapBirthday;
    private bool _mapContactPhoto;
    private bool _fixPhoneNumberFormat;
    private bool _isSelected;

    public bool MapBirthday
    {
      get { return _mapBirthday; }
      set
      {
        CheckedPropertyChange (ref _mapBirthday, value);
      }
    }

    public bool MapContactPhoto
    {
      get { return _mapContactPhoto; }
      set
      {
        CheckedPropertyChange (ref _mapContactPhoto, value);
      }
    }

    public bool FixPhoneNumberFormat
    {
      get { return _fixPhoneNumberFormat; }
      set
      {
        CheckedPropertyChange (ref _fixPhoneNumberFormat, value);
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

    public static ContactMappingConfigurationViewModel DesignInstance => new ContactMappingConfigurationViewModel
                                                                         {
                                                                             MapBirthday = true,
                                                                             MapContactPhoto = true,
                                                                             FixPhoneNumberFormat = false
                                                                         };

    
    public void SetOptions (CalDavSynchronizer.Contracts.Options options)
    {
      SetOptions(options.MappingConfiguration as ContactMappingConfiguration ?? new ContactMappingConfiguration());
    }

    public void SetOptions (ContactMappingConfiguration mappingConfiguration)
    {
      MapBirthday = mappingConfiguration.MapBirthday;
      MapContactPhoto = mappingConfiguration.MapContactPhoto;
      FixPhoneNumberFormat = mappingConfiguration.FixPhoneNumberFormat;
    }

    public void FillOptions (CalDavSynchronizer.Contracts.Options options)
    {
      options.MappingConfiguration = new ContactMappingConfiguration
                                     {
                                         MapBirthday = _mapBirthday,
                                         MapContactPhoto = _mapContactPhoto,
                                         FixPhoneNumberFormat = _fixPhoneNumberFormat
                                     };
    }

    public string Name => "Contact mapping configuration";

    public bool Validate (StringBuilder errorMessageBuilder)
    {
      return true;
    }

    public IEnumerable<IOptionsViewModel> SubOptions => new IOptionsViewModel[] { };
  }
}