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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Utilities;

namespace CalDavSynchronizer.Ui
{
  public partial class EventMappingConfigurationForm : Form, IConfigurationForm<EventMappingConfiguration>
  {
    private readonly IList<Item<ReminderMapping>> _availableReminderMappings = new List<Item<ReminderMapping>>()
                                                                               {
                                                                                   new Item<ReminderMapping> (ReminderMapping.@true, "Yes"),
                                                                                   new Item<ReminderMapping> (ReminderMapping.@false, "No"),
                                                                                   new Item<ReminderMapping> (ReminderMapping.JustUpcoming, "Just upcoming reminders"),
                                                                               };

    private Func<ICalDavDataAccess> _calDavDataAccessFactory;


    public EventMappingConfigurationForm (Func<ICalDavDataAccess> calDavDataAccessFactory)
    {
      InitializeComponent();
      Item.BindComboBox (_mapReminderComboBox, _availableReminderMappings);

      _calDavDataAccessFactory = calDavDataAccessFactory;
    }

    private void _okButton_Click (object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    public bool Display ()
    {
      return ShowDialog() == DialogResult.OK;
    }

    public EventMappingConfiguration Options
    {
      get
      {
        return new EventMappingConfiguration
               {
                   MapAttendees = _mapAttendeesCheckBox.Checked,
                   MapBody = _mapBodyCheckBox.Checked,
                   MapReminder = (ReminderMapping) _mapReminderComboBox.SelectedValue,
                   EventCategory = _categoryTextBox.Text
               };
      }
      set
      {
        _mapAttendeesCheckBox.Checked = value.MapAttendees;
        _mapBodyCheckBox.Checked = value.MapBody;
        _mapReminderComboBox.SelectedValue = value.MapReminder;
        _categoryTextBox.Text = value.EventCategory;
        _mapColorCheckBox.Enabled = !string.IsNullOrEmpty(value.EventCategory);
        _calendarColorButton.Enabled = !string.IsNullOrEmpty(value.EventCategory);
      }
    }

    private async void _calendarColorButton_Click(object sender, EventArgs e)
    {
    }

    private async void _mapColorCheckBox_CheckedChanged(object sender, EventArgs e)
    {
      if (_mapColorCheckBox.Checked)
      {
        string serverColor = await _calDavDataAccessFactory().GetCalendarColorNoThrow();

        if (!string.IsNullOrEmpty(serverColor))
        {
          Color c = ColorHelper.HexToColor(serverColor);
          Color mappedColor = ColorHelper.FindMatchingCategoryColor (c);

          _calendarColorButton.BackColor = mappedColor;
        }
      }
    }

    private void _categoryTextBox_TextChanged(object sender, EventArgs e)
    {
      if (!string.IsNullOrEmpty(_categoryTextBox.Text))
      {
        _mapColorCheckBox.Enabled = true;
        _calendarColorButton.Enabled = true;
      }
      else
      {
        _mapColorCheckBox.Enabled = false;
        _calendarColorButton.Enabled = false;
      }
    }
  }
}