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
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using System.Collections.Generic;

namespace CalDavSynchronizer.Ui.Options.Mapping
{
  public partial class TaskMappingConfigurationForm : Form, IConfigurationForm<TaskMappingConfiguration>
  {
    private readonly IList<Item<ReminderMapping>> _availableReminderMappings = new List<Item<ReminderMapping>>()
                                                                               {
                                                                                   new Item<ReminderMapping> (ReminderMapping.@true, "Yes"),
                                                                                   new Item<ReminderMapping> (ReminderMapping.@false, "No"),
                                                                                   new Item<ReminderMapping> (ReminderMapping.JustUpcoming, "Just upcoming reminders")
                                                                               };

    public TaskMappingConfigurationForm ()
    {
      InitializeComponent();
      Item.BindComboBox (_mapReminderComboBox, _availableReminderMappings);
    }

    private void _okButton_Click (object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    public bool Display ()
    {
      return ShowDialog() == DialogResult.OK;
    }

    public TaskMappingConfiguration Options
    {
      get
      {
        return new TaskMappingConfiguration
               {
                  MapReminder = (ReminderMapping)_mapReminderComboBox.SelectedValue,
                  MapPriority = _mapPriorityCheckBox.Checked,
                  MapBody = _mapBodyCheckBox.Checked
               };
      }
      set
      {
        _mapReminderComboBox.SelectedValue = value.MapReminder;
        _mapPriorityCheckBox.Checked = value.MapPriority;
        _mapBodyCheckBox.Checked = value.MapBody;
      }
    }
  }
}