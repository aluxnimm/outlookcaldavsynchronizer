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
using CalDavSynchronizer.Ui.Options.Models;

namespace CalDavSynchronizer.Ui.Options.ViewModels.Mapping
{
  public class ContactMappingConfigurationViewModel : ModelBase, ISubOptionsViewModel
  {
    private bool _isSelected;
    private bool _isExpanded;
    private readonly ContactMappingConfigurationModel _model;

    public ContactMappingConfigurationViewModel(ContactMappingConfigurationModel model)
    {
      if (model == null) throw new ArgumentNullException(nameof(model));

      _model = model;

      RegisterPropertyChangePropagation(_model, nameof(_model.MapBirthday), nameof(MapBirthday));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapContactPhoto), nameof(MapContactPhoto));
      RegisterPropertyChangePropagation(_model, nameof(_model.KeepOutlookPhoto), nameof(KeepOutlookPhoto));
      RegisterPropertyChangePropagation(_model, nameof(_model.KeepOutlookFileAs), nameof(KeepOutlookFileAs));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapOutlookEmail1ToWork), nameof(MapOutlookEmail1ToWork));
      RegisterPropertyChangePropagation(_model, nameof(_model.FixPhoneNumberFormat), nameof(FixPhoneNumberFormat));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapDistributionLists), nameof(MapDistributionLists));
      RegisterPropertyChangePropagation(_model, nameof(_model.DistributionListType), nameof(DistributionListType));
    }

    public bool MapBirthday
    {
      get { return _model.MapBirthday; }
      set { _model.MapBirthday = value; }
    }

    public bool MapContactPhoto
    {
      get { return _model.MapContactPhoto; }
      set { _model.MapContactPhoto = value; }
    }

    public bool KeepOutlookPhoto
    {
      get { return _model.KeepOutlookPhoto; }
      set { _model.KeepOutlookPhoto = value; }
    }

    public bool KeepOutlookFileAs
    {
      get { return _model.KeepOutlookFileAs; }
      set { _model.KeepOutlookFileAs = value; }
    }

    public bool FixPhoneNumberFormat
    {
      get { return _model.FixPhoneNumberFormat; }
      set { _model.FixPhoneNumberFormat = value; }
    }
    public bool MapOutlookEmail1ToWork
    {
      get { return _model.MapOutlookEmail1ToWork; }
      set { _model.MapOutlookEmail1ToWork = value; }
    }

    public bool MapDistributionLists
    {
      get { return _model.MapDistributionLists; }
      set { _model.MapDistributionLists = value; }
    }

    public DistributionListType DistributionListType
    {
      get { return _model.DistributionListType; }
      set { _model.DistributionListType = value; }
    }

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



    public IList<Item<DistributionListType>> AvailableDistributionListTypes { get; } = new List<Item<DistributionListType>>
                                                                                     {
                                                                                         new Item<DistributionListType> (DistributionListType.Sogo, "SOGo VLIST"),
                                                                                         new Item<DistributionListType> (DistributionListType.VCardGroup, "vCard with KIND:group"),
                                                                                     };

    public static ContactMappingConfigurationViewModel DesignInstance => new ContactMappingConfigurationViewModel(new ContactMappingConfigurationModel(new ContactMappingConfiguration()))
    {
      MapBirthday = true,
      MapContactPhoto = true,
      KeepOutlookPhoto = true,
      KeepOutlookFileAs = true,
      FixPhoneNumberFormat = true,
      MapOutlookEmail1ToWork = true,
      DistributionListType = DistributionListType.Sogo,
      MapDistributionLists = true,
    };





    public string Name => "Contact mapping configuration";
    public IEnumerable<ITreeNodeViewModel> Items { get; } = new ITreeNodeViewModel[0];
    public IEnumerable<ISubOptionsViewModel> SubOptions => new ISubOptionsViewModel[] { };
  }
}