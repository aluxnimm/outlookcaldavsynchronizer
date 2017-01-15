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

using CalDavSynchronizer.Contracts;

namespace CalDavSynchronizer.Ui.Options.Models
{
  public class PropertyMappingModel : ModelBase
  {
    private string _outlookProperty;
    private string _davProperty;

    public PropertyMappingModel()
    {
    }

    public PropertyMappingModel(PropertyMapping data)
    {
      _outlookProperty = data.OutlookProperty;
      _davProperty = data.DavProperty;
    }

    public string OutlookProperty
    {
      get { return _outlookProperty; }
      set { CheckedPropertyChange(ref _outlookProperty, value); }
    }

    public string DavProperty
    {
      get { return _davProperty; }
      set { CheckedPropertyChange(ref _davProperty, value); }
    }

    public PropertyMapping GetData()
    {
      return new PropertyMapping {DavProperty = DavProperty, OutlookProperty = OutlookProperty};
    }
  }
}