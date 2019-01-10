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

using System.Xml.Serialization;
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Ui.Options.ViewModels.Mapping;

namespace CalDavSynchronizer.Contracts
{
  public class TaskMappingConfiguration : MappingConfigurationBase, IPropertyMappingConfiguration
  {
    public ReminderMapping MapReminder { get; set; }
    public bool MapReminderAsDateTime { get; set; }
    public bool MapPriority { get; set; }
    public bool MapBody { get; set; }
    public bool MapRecurringTasks { get; set; }
    public bool MapStartAndDueAsFloating { get; set; }
    public string TaskCategory { get; set; }
    public bool IsCategoryFilterSticky { get; set; }
    public bool IncludeEmptyTaskCategoryFilter { get; set; }
    public bool InvertTaskCategoryFilter { get; set; }
    public bool MapCustomProperties { get; set; }

    private PropertyMapping[] _userDefinedCustomPropertyMappings;
    public PropertyMapping[] UserDefinedCustomPropertyMappings
    {
      get { return _userDefinedCustomPropertyMappings ?? new PropertyMapping[0]; }
      set { _userDefinedCustomPropertyMappings = value ?? new PropertyMapping[0]; }
    }


    [XmlIgnore]
    public bool UseTaskCategoryAsFilter
    {
      get { return !string.IsNullOrEmpty (TaskCategory); }
    }

    public TaskMappingConfiguration ()
    {
   
    }

    public override TResult Accept<TResult>(IMappingConfigurationBaseVisitor<TResult> visitor)
    {
      return visitor.Visit(this);
    }
  }
}