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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Ui.Options.Models;
using Thought.vCards;

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

      RegisterPropertyChangePropagation(_model, nameof(_model.MapAnniversary), nameof(MapAnniversary));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapBirthday), nameof(MapBirthday));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapContactPhoto), nameof(MapContactPhoto));
      RegisterPropertyChangePropagation(_model, nameof(_model.KeepOutlookPhoto), nameof(KeepOutlookPhoto));
      RegisterPropertyChangePropagation(_model, nameof(_model.KeepOutlookFileAs), nameof(KeepOutlookFileAs));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapOutlookEmail1ToWork), nameof(MapOutlookEmail1ToWork));
      RegisterPropertyChangePropagation(_model, nameof(_model.WriteImAsImpp), nameof(WriteImAsImpp));
      RegisterPropertyChangePropagation(_model, nameof(_model.FixPhoneNumberFormat), nameof(FixPhoneNumberFormat));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapDistributionLists), nameof(MapDistributionLists));
      RegisterPropertyChangePropagation(_model, nameof(_model.DistributionListType), nameof(DistributionListType));
    }

    public bool MapAnniversary
    {
      get { return _model.MapAnniversary; }
      set { _model.MapAnniversary = value; }
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
    public bool WriteImAsImpp
    {
      get { return _model.WriteImAsImpp; }
      set { _model.WriteImAsImpp = value; }
    }

    public IMServiceType DefaultImServiceType
    {
      get { return _model.DefaultImServiceType; }
      set { _model.DefaultImServiceType = value; }
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

    public IList<Item<IMServiceType>> AvailableImServiceTypes => 
      Enum.GetValues (typeof (IMServiceType)).Cast<IMServiceType>().
      Where (i => i!=IMServiceType.Unspecified).Select (i => new Item<IMServiceType> (i, i.ToString())).OrderBy (i => i.Name).ToList();

    public IList<Item<DistributionListType>> AvailableDistributionListTypes { get; } = new List<Item<DistributionListType>>
                                                                                     {
                                                                                         new Item<DistributionListType> (DistributionListType.Sogo,  Strings.Get($"SOGo VLIST")),
                                                                                         new Item<DistributionListType> (DistributionListType.VCardGroup,  Strings.Get($"vCard with KIND:group")),
                                                                                         new Item<DistributionListType> (DistributionListType.VCardGroupWithUid,  Strings.Get($"iCloud group")),
                                                                                     };

    public static ContactMappingConfigurationViewModel DesignInstance => new ContactMappingConfigurationViewModel(new ContactMappingConfigurationModel(new ContactMappingConfiguration()))
    {
      MapAnniversary = true,
      MapBirthday = true,
      MapContactPhoto = true,
      KeepOutlookPhoto = true,
      KeepOutlookFileAs = true,
      FixPhoneNumberFormat = true,
      MapOutlookEmail1ToWork = true,
      WriteImAsImpp = true,
      DefaultImServiceType = IMServiceType.AIM,
      DistributionListType = DistributionListType.Sogo,
      MapDistributionLists = true,
    };





    public string Name => Strings.Get($"Contact Mapping Configuration");
    public IEnumerable<ITreeNodeViewModel> Items { get; } = new ITreeNodeViewModel[0];
    public IEnumerable<ISubOptionsViewModel> SubOptions => new ISubOptionsViewModel[] { };
  }
}