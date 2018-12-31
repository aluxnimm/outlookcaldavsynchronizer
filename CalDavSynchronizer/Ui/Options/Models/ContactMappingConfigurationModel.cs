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
using System.Text;
using CalDavSynchronizer.Contracts;
using Thought.vCards;

namespace CalDavSynchronizer.Ui.Options.Models
{
  public class ContactMappingConfigurationModel : MappingConfigurationModel
  {
    private bool _mapAnniversary;
    private bool _mapBirthday;
    private bool _mapContactPhoto;
    private bool _keepOutlookPhoto;
    private bool _keepOutlookFileAs;
    private bool _fixPhoneNumberFormat;
    private bool _mapOutlookEmail1ToWork;
    private bool _writeImAsImpp;
    private IMServiceType _defaultImServiceType;
    private bool _mapDistributionLists;
    private bool _isSelected;
    private bool _isExpanded;
    private DistributionListType _distributionListType;

    public ContactMappingConfigurationModel(ContactMappingConfiguration data)
    {
      if (data == null) throw new ArgumentNullException(nameof(data));

      InitializeData(data);
    }

    public bool MapAnniversary
    {
      get { return _mapAnniversary; }
      set
      {
        CheckedPropertyChange (ref _mapAnniversary, value);
      }
    }

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

    public bool KeepOutlookPhoto
    {
      get { return _keepOutlookPhoto; }
      set
      {
        CheckedPropertyChange(ref _keepOutlookPhoto, value);
      }
    }

    public bool KeepOutlookFileAs
    {
      get { return _keepOutlookFileAs; }
      set
      {
        CheckedPropertyChange(ref _keepOutlookFileAs, value);
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

    public bool MapOutlookEmail1ToWork
    {
      get { return _mapOutlookEmail1ToWork; }
      set
      {
        CheckedPropertyChange (ref _mapOutlookEmail1ToWork, value);
      }
    }

    public bool WriteImAsImpp
    {
      get { return _writeImAsImpp; }
      set
      {
        CheckedPropertyChange (ref _writeImAsImpp, value);
      }
    }

    public IMServiceType DefaultImServiceType
    {
      get { return _defaultImServiceType; }
      set
      {
        CheckedPropertyChange (ref _defaultImServiceType, value);
      }
    }

    public bool MapDistributionLists
    {
      get { return _mapDistributionLists; }
      set
      {
        CheckedPropertyChange(ref _mapDistributionLists, value);
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

    public bool IsExpanded
    {
      get { return _isExpanded; }
      set
      {
        CheckedPropertyChange (ref _isExpanded, value);
      }
    }

    public DistributionListType DistributionListType
    {
      get { return _distributionListType; }
      set { CheckedPropertyChange(ref _distributionListType, value); }
    }



    /// <remarks>
    /// InitializeData has to set fields instead of properties, since properties can interfer with each other!
    /// </remarks>
    private void InitializeData(ContactMappingConfiguration mappingConfiguration)
    {
      MapAnniversary = mappingConfiguration.MapAnniversary;
      MapBirthday = mappingConfiguration.MapBirthday;
      MapContactPhoto = mappingConfiguration.MapContactPhoto;
      KeepOutlookPhoto = mappingConfiguration.KeepOutlookPhoto;
      KeepOutlookFileAs = mappingConfiguration.KeepOutlookFileAs;
      FixPhoneNumberFormat = mappingConfiguration.FixPhoneNumberFormat;
      MapOutlookEmail1ToWork = mappingConfiguration.MapOutlookEmail1ToWork;
      WriteImAsImpp = mappingConfiguration.WriteImAsImpp;
      DefaultImServiceType = mappingConfiguration.DefaultImServicType;
      MapDistributionLists = mappingConfiguration.MapDistributionLists;
      DistributionListType = mappingConfiguration.DistributionListType;
    }

    public override MappingConfigurationBase GetData()
    {
      return new ContactMappingConfiguration
      {
        MapAnniversary = _mapAnniversary,
        MapBirthday = _mapBirthday,
        MapContactPhoto = _mapContactPhoto,
        KeepOutlookPhoto = _keepOutlookPhoto,
        KeepOutlookFileAs = _keepOutlookFileAs,
        FixPhoneNumberFormat = _fixPhoneNumberFormat,
        MapOutlookEmail1ToWork = _mapOutlookEmail1ToWork,
        WriteImAsImpp = _writeImAsImpp,
        DefaultImServicType = _defaultImServiceType,
        MapDistributionLists = _mapDistributionLists,
        DistributionListType = _distributionListType
      };
    }
    
    public override void AddOneTimeTasks(Action<OneTimeChangeCategoryTask> add)
    {
    
    }

    public override bool Validate (StringBuilder errorMessageBuilder)
    {
      return true;
    }

  }
}