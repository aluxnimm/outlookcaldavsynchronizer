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
using CalDavSynchronizer.Ui.Options.Mapping;

namespace CalDavSynchronizer.Contracts
{
  public class TaskMappingConfiguration : MappingConfigurationBase
  {
    public ReminderMapping MapReminder { get; set; }
    public bool MapPriority { get; set; }
    public bool MapBody { get; set; }
    public bool MapRecurringTasks { get; set; }

    public TaskMappingConfiguration ()
    {
      MapReminder = ReminderMapping.JustUpcoming;
      MapPriority = true;
      MapBody = true;
      MapRecurringTasks = true;
    }

    public override IConfigurationForm<MappingConfigurationBase> CreateConfigurationForm (IConfigurationFormFactory factory)
    {
      return factory.Create (this);
    }
  }
}